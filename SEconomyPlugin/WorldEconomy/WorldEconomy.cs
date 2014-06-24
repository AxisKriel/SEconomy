using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

using TerrariaApi.Server;

namespace Wolfje.Plugins.SEconomy.WorldEconomy {

    /// <summary>
    /// World economy. Provides monetary gain and loss as a result of interaction in the world, including mobs and players
    /// </summary>
    public class WorldEconomy {

        /// <summary>
        /// Format for this dictionary:
        /// Key: NPC
        /// Value: A list of players who have done damage to the NPC
        /// </summary>
        static volatile Dictionary<Terraria.NPC, List<PlayerDamage>> DamageDictionary = new Dictionary<Terraria.NPC, List<PlayerDamage>>();

        /// <summary>
        /// Format for this dictionary:
        /// * key: Player ID
        /// * value: Last player hit ID
        /// </summary>
        static volatile Dictionary<int, int> PVPDamage = new Dictionary<int, int>();

        /// <summary>
        /// synch object for access to the dictionary.  You MUST obtain a mutex through this object to access the volatile dictionary member.
        /// </summary>
        static readonly object __dictionaryLock = new object();

        /// <summary>
        /// synch object for access to the pvp dictionary.  You MUST obtain a mutex through this object to access the volatile dictionary member.
        /// </summary>
        static readonly object __pvpLock = new object();

        /// <summary>
        /// Synch object for access to the packet handler critical sections, forcing packets to be marshalled in a serialized manner.
        /// </summary>
        static readonly object __packetHandlerMutex = new object();

        /// <summary>
        /// World configuration node, from TShock\SEconomy\SEconomy.WorldConfig.json
        /// </summary>
        public static Configuration.WorldConfiguration.WorldConfig WorldConfiguration { get; private set; }

        /// <summary>
        /// Initializes the world economy interface
        /// </summary>
        public static void InitializeWorldEconomy() {
            WorldConfiguration = Configuration.WorldConfiguration.WorldConfig.LoadConfigurationFromFile("tshock" + System.IO.Path.DirectorySeparatorChar + "SEconomy" + System.IO.Path.DirectorySeparatorChar + "SEconomy.WorldConfig.json");

            ServerApi.Hooks.NetGetData.Register(SEconomyPlugin.Instance, NetHooks_GetData);
            ServerApi.Hooks.NetSendData.Register(SEconomyPlugin.Instance, NetHooks_SendData);
            ServerApi.Hooks.GameUpdate.Register(SEconomyPlugin.Instance, Game_Update);
        }

        private static void Game_Update(EventArgs args) {
            foreach (Terraria.NPC npc in Terraria.Main.npc) {
                if (npc == null || npc.townNPC == true) {
                    continue;
                }

                if (npc.active == false) {
                    GiveRewardsForNPC(npc);
                }
            }
        }

        /// <summary>
        /// Cleans up and destroys the world economy interface
        /// </summary>
        public static void DestroyWorldEconomy() {
            ServerApi.Hooks.NetSendData.Deregister(SEconomyPlugin.Instance, NetHooks_SendData);
            ServerApi.Hooks.GameUpdate.Deregister(SEconomyPlugin.Instance, Game_Update);
        }

        #region "NPC Reward handling"
        /// <summary>
        /// Adds damage done by a player to an NPC slot.  When the NPC dies the rewards for it will fill out.
        /// </summary>
        static void AddNPCDamage(Terraria.NPC NPC, Terraria.Player Player, int Damage) {
            List<PlayerDamage> damageList = null;
            PlayerDamage playerDamage = null;

            if (Player == null || NPC.active == false) {
                return;
            }

            lock (__dictionaryLock) {
                if (DamageDictionary.ContainsKey(NPC)) {
                    damageList = DamageDictionary[NPC];
                } else {
                    damageList = new List<PlayerDamage>(1);
                    DamageDictionary.Add(NPC, damageList);
                }
            }

            playerDamage = damageList.FirstOrDefault(i => i.Player == Player);
            if (playerDamage == null) {
                playerDamage = new PlayerDamage() { Player = Player };
                damageList.Add(playerDamage);
            }

            //increment the damage into either the new or existing slot damage in the dictionary
            //If the damage is greater than the NPC's health then it was a one-shot kill and the damage should be capped.
            playerDamage.Damage += NPC != null && Damage > NPC.lifeMax ? NPC.lifeMax : Damage;
        }

        /// <summary>
        /// Should occur when an NPC dies; gives rewards out to all the players that hit it.
        /// </summary>
        static void GiveRewardsForNPC(Terraria.NPC NPC) {
            List<PlayerDamage> playerDamageList = null;
            Economy.EconomyPlayer ePlayer = null;
            Money rewardMoney = 0L;

            lock (__dictionaryLock) {
                if (DamageDictionary.ContainsKey(NPC)) {
                    playerDamageList = DamageDictionary[NPC];

                    if (DamageDictionary.Remove(NPC) == false) {
                        TShockAPI.Log.ConsoleError("seconomy: world economy: Remove of NPC after reward failed.  This is an internal error.");
                    }
                }
            }

            if (playerDamageList == null) {
                return;
            }

            if ((NPC.boss && WorldConfiguration.MoneyFromBossEnabled) || (!NPC.boss && WorldConfiguration.MoneyFromNPCEnabled)) {
                foreach (PlayerDamage damage in playerDamageList) {
                    if (damage.Player == null) {
                        continue;
                    }

                    ePlayer = SEconomyPlugin.GetEconomyPlayerSafe(damage.Player.whoAmi);
                    rewardMoney = Convert.ToInt64(Math.Round(WorldConfiguration.MoneyPerDamagePoint * damage.Damage));

                    //load override by NPC type, this allows you to put a modifier on the base for a specific mob type.
                    Configuration.WorldConfiguration.NPCRewardOverride overrideReward = WorldConfiguration.Overrides.FirstOrDefault(i => i.NPCID == NPC.type);
                    if (overrideReward != null) {
                        rewardMoney = Convert.ToInt64(Math.Round(overrideReward.OverridenMoneyPerDamagePoint * damage.Damage));
                    }

                    //if the user doesn't have a bank account or the reward for the mob is 0 (It could be) skip it
                    if (ePlayer != null && ePlayer.BankAccount != null && rewardMoney > 0 && ePlayer.TSPlayer.Group.HasPermission("seconomy.world.mobgains")) {
                        Journal.CachedTransaction fund = new Journal.CachedTransaction() {
                            Aggregations = 1,
                            Amount = rewardMoney,
                            DestinationBankAccountK = ePlayer.BankAccount.BankAccountK,
                            Message = NPC.name,
                            SourceBankAccountK = SEconomyPlugin.WorldAccount.BankAccountK
                        };

                        if ((NPC.boss && WorldConfiguration.AnnounceBossKillGains) || (!NPC.boss && WorldConfiguration.AnnounceNPCKillGains)) {
                            fund.Options |= Journal.BankAccountTransferOptions.AnnounceToReceiver;
                        }

                        //commit it to the transaction cache
                        Journal.TransactionJournal.AddCachedTransaction(fund);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Assigns the last player slot to a victim in PVP
        /// </summary>
        static void PlayerHitPlayer(int HitterSlot, int VictimSlot) {
            lock (__dictionaryLock) {
                if (PVPDamage.ContainsKey(VictimSlot)) {
                    PVPDamage[VictimSlot] = HitterSlot;
                } else {
                    PVPDamage.Add(VictimSlot, HitterSlot);
                }
            }
        }

        /// <summary>
        /// Runs when a player dies, and hands out penalties if enabled, and rewards for PVP
        /// </summary>
        static void ProcessDeath(int DeadPlayerSlot, bool PVPDeath) {
            TShockAPI.TSPlayer deadPlayer = TShockAPI.TShock.Players[DeadPlayerSlot];
            int lastHitterSlot = -1;

            //get the last hitter ID out of the dictionary

            lock (__dictionaryLock) {
                if (PVPDamage.ContainsKey(DeadPlayerSlot)) {
                    lastHitterSlot = PVPDamage[DeadPlayerSlot];

                    PVPDamage.Remove(DeadPlayerSlot);
                }
            }

            if (deadPlayer != null && !deadPlayer.Group.HasPermission("seconomy.world.bypassdeathpenalty")) {
                Economy.EconomyPlayer eDeadPlayer = SEconomyPlugin.GetEconomyPlayerSafe(DeadPlayerSlot);

                if (eDeadPlayer != null && eDeadPlayer.BankAccount != null) {
                    Journal.CachedTransaction playerToWorldTx = Journal.CachedTransaction.NewTransactionToWorldAccount();

                    //The penalty defaults to a percentage of the players' current balance.
                    Money penalty = (long)Math.Round(Convert.ToDouble(eDeadPlayer.BankAccount.Balance.Value) * (Convert.ToDouble(WorldConfiguration.DeathPenaltyPercentValue) * Math.Pow(10, -2)));

                    if (penalty > 0) {
                        playerToWorldTx.SourceBankAccountK = eDeadPlayer.BankAccount.BankAccountK;
                        playerToWorldTx.Message = "dying";
                        playerToWorldTx.Options = Journal.BankAccountTransferOptions.MoneyTakenOnDeath | Journal.BankAccountTransferOptions.AnnounceToSender;
                        playerToWorldTx.Amount = penalty;

                        //the dead player loses money unconditionally
                        Journal.TransactionJournal.AddCachedTransaction(playerToWorldTx);

                        //but if it's a PVP death, the killer gets the losers penalty if enabled
                        if (PVPDeath && WorldConfiguration.MoneyFromPVPEnabled && WorldConfiguration.KillerTakesDeathPenalty) {
                            Economy.EconomyPlayer eKiller = SEconomyPlugin.GetEconomyPlayerSafe(lastHitterSlot);

                            if (eKiller != null && eKiller.BankAccount != null) {
                                Journal.CachedTransaction worldToPlayerTx = Journal.CachedTransaction.NewTranasctionFromWorldAccount();

                                worldToPlayerTx.DestinationBankAccountK = eKiller.BankAccount.BankAccountK;
                                worldToPlayerTx.Amount = penalty;
                                worldToPlayerTx.Message = "killing " + eDeadPlayer.TSPlayer.Name;
                                worldToPlayerTx.Options = Journal.BankAccountTransferOptions.AnnounceToReceiver;

                                Journal.TransactionJournal.AddCachedTransaction(worldToPlayerTx);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when the server has received a message from the client.
        /// </summary>
        private static void NetHooks_GetData(GetDataEventArgs args) {
            byte[] bufferSegment = null;
            Terraria.Player player = null;

            if ((player = Terraria.Main.player.ElementAtOrDefault(args.Msg.whoAmI)) == null) {
                return;
            }

            bufferSegment = new byte[args.Length];
            System.Array.Copy(args.Msg.readBuffer, args.Index, bufferSegment, 0, args.Length);

            lock (__packetHandlerMutex) {
                if (args.MsgID == PacketTypes.NpcStrike) {
                    Terraria.NPC npc = null;
                    Packets.DamageNPC dmgPacket = Packets.PacketMarshal.MarshalFromBuffer<Packets.DamageNPC>(bufferSegment);

                    if (dmgPacket.NPCID < 0 || dmgPacket.NPCID > Terraria.Main.npc.Length
                        || args.Msg.whoAmI < 0 || dmgPacket.NPCID > Terraria.Main.player.Length) {
                        return;
                    }

                    if ((npc = Terraria.Main.npc.ElementAtOrDefault(dmgPacket.NPCID)) == null) {
                        return;
                    }

                    AddNPCDamage(npc, player, dmgPacket.Damage);
                }
            }
        }

        /// <summary>
        /// Occurs when the server has a chunk of data to send
        /// </summary>
        public static void NetHooks_SendData(SendDataEventArgs e) {
            if (e.MsgId == PacketTypes.PlayerDamage) {
                //occurs when a player hits another player.  ignoreClient is the player that hit, e.number is the 
                //player that got hit, and e.number4 is a flag indicating PvP damage

                if (Convert.ToBoolean(e.number4) && Terraria.Main.player[e.number] != null) {
                    PlayerHitPlayer(e.ignoreClient, e.number);
                }
            } else if (e.MsgId == PacketTypes.PlayerKillMe) {
                //Occrs when the player dies.
                ProcessDeath(e.number, Convert.ToBoolean(e.number4));
            }
        }
    }

    /// <summary>
    /// Damage structure, wraps a player slot and the amount of damage they have done.
    /// </summary>
    public class PlayerDamage {
        public Terraria.Player Player;
        public int Damage;
    }

}

