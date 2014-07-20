using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;
using Wolfje.Plugins.SEconomy.Journal;

namespace Wolfje.Plugins.SEconomy {
	/// <summary>
	/// Disposable wrapper for all data handlers that SEconomy depends on to run
	/// </summary>
	public class EventHandlers : IDisposable {
        public SEconomy Parent { get; protected set; }
        protected System.Timers.Timer PayRunTimer { get; set; }

		public EventHandlers(SEconomy Parent)
		{
			this.Parent = Parent;
            if (Parent.Configuration.PayIntervalMinutes > 0) {
                PayRunTimer = new System.Timers.Timer(Parent.Configuration.PayIntervalMinutes * 60000);
                PayRunTimer.Elapsed += PayRunTimer_Elapsed;
                PayRunTimer.Start();
            }

			TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += PlayerHooks_PlayerPostLogin;
			Parent.RunningJournal.BankTransferCompleted += BankAccount_BankTransferCompleted;
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

			ServerApi.Hooks.GamePostInitialize.Register(this.Parent.PluginInstance, GameHooks_PostInitialize);
			ServerApi.Hooks.ServerJoin.Register(this.Parent.PluginInstance, ServerHooks_Join);
			ServerApi.Hooks.ServerLeave.Register(this.Parent.PluginInstance, ServerHooks_Leave);
			ServerApi.Hooks.NetGetData.Register(this.Parent.PluginInstance, NetHooks_GetData);
		}

		/// <summary>
		/// Default messages sent to players when a transaction happens within SEconomy.
		/// </summary>
		protected void BankAccount_BankTransferCompleted(object s, Journal.BankTransferEventArgs e)
		{
            TSPlayer sender, receiver;

			if (e.ReceiverAccount == null
				|| (e.TransferOptions & Journal.BankAccountTransferOptions.SuppressDefaultAnnounceMessages) 
				== Journal.BankAccountTransferOptions.SuppressDefaultAnnounceMessages) {
				return;
			}

            sender = TShockAPI.TShock.Players.FirstOrDefault(i => i != null && i.UserAccountName == e.SenderAccount.UserAccountName);
            receiver = TShockAPI.TShock.Players.FirstOrDefault(i => i != null && i.UserAccountName == e.ReceiverAccount.UserAccountName);

            if ((e.TransferOptions & Journal.BankAccountTransferOptions.IsPlayerToPlayerTransfer) == Journal.BankAccountTransferOptions.IsPlayerToPlayerTransfer) {
				if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToReceiver) == Journal.BankAccountTransferOptions.AnnounceToReceiver && e.ReceiverAccount != null && receiver != null) {
					receiver.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(16, "You {3} {0} from {1}. Transaction # {2}"), e.Amount.ToLongString(), 
						sender != null ? sender.Name : SEconomyPlugin.Locale.StringOrDefault(17, "The server"), e.TransactionID, 
						e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(18, "received") : SEconomyPlugin.Locale.StringOrDefault(19, "sent")), Color.Orange);
				}
				if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToSender) == Journal.BankAccountTransferOptions.AnnounceToSender && sender != null) {
					sender.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(16, "You {3} {0} from {1}. Transaction # {2}"), e.Amount.ToLongString(), receiver.Name, e.TransactionID,
						e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(19, "sent") : SEconomyPlugin.Locale.StringOrDefault(18, "received")), Color.Orange);
				}

				//Everything else, including world to player, and player to world.
			} else {
				if ((e.TransferOptions & Journal.BankAccountTransferOptions.IsPayment) == Journal.BankAccountTransferOptions.IsPayment) {
					if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToSender) == Journal.BankAccountTransferOptions.AnnounceToSender && sender != null) {
						sender.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(20, "You {0} {1}{2}"), 
							e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(21, "paid") : SEconomyPlugin.Locale.StringOrDefault(22, "got paid"), e.Amount.ToLongString(),
							!string.IsNullOrEmpty(e.TransactionMessage) ? SEconomyPlugin.Locale.StringOrDefault(25, " for ") + e.TransactionMessage : ""), Color.Orange);
					}

					if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToReceiver) == Journal.BankAccountTransferOptions.AnnounceToReceiver && receiver != null) {
						receiver.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(20, "You {0} {1}{2}"), e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(22, "got paid") 
							: SEconomyPlugin.Locale.StringOrDefault(22, "paid"), e.Amount.ToLongString(),
							!string.IsNullOrEmpty(e.TransactionMessage) ? SEconomyPlugin.Locale.StringOrDefault(25, " for ") + e.TransactionMessage : ""), Color.Orange);
					}
				} else {
					if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToSender) == Journal.BankAccountTransferOptions.AnnounceToSender && sender != null) {
						sender.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(20, "You {0} {1}{2}"), e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(23, "lost") 
							: SEconomyPlugin.Locale.StringOrDefault(24, "gained"), e.Amount.ToLongString(), 
							!string.IsNullOrEmpty(e.TransactionMessage) ? SEconomyPlugin.Locale.StringOrDefault(25, " for ") + e.TransactionMessage : ""), Color.Orange);
					}

					if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToReceiver) == Journal.BankAccountTransferOptions.AnnounceToReceiver && receiver != null) {
						receiver.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(20, "You {0} {1}{2}"), e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(24, "gained")
							: SEconomyPlugin.Locale.StringOrDefault(23, "lost"), e.Amount.ToLongString(), 
							!string.IsNullOrEmpty(e.TransactionMessage) ? SEconomyPlugin.Locale.StringOrDefault(25, " for ") + e.TransactionMessage : ""), Color.Orange);
					}
				}
			}
		}

		/// <summary>
		/// Occurs when the engine receives a packet from the client
		/// </summary>
		protected void NetHooks_GetData(GetDataEventArgs args)
		{
			byte index = default(byte);
			PlayerControlFlags playerState = default(PlayerControlFlags);
            Terraria.Player player;

			if (args.MsgID != PacketTypes.PlayerUpdate 
				|| (index = args.Msg.readBuffer[args.Index]) < 0
                || (player = Terraria.Main.player.ElementAtOrDefault(args.Msg.whoAmI)) == null) {
				return;
			}

			if ((playerState = (PlayerControlFlags)args.Msg.readBuffer[args.Index + 1]) != PlayerControlFlags.Idle) {
                Parent.UpdatePlayerIdle(player);
			}
		}

		/// <summary>
		/// Occurs when someone leaves the game.
		/// </summary>
		protected void ServerHooks_Leave(LeaveEventArgs args)
		{
            Parent.RemovePlayerIdleCache(Terraria.Main.player.ElementAtOrDefault(args.Who));
		}

		/// <summary>
		/// Occurs when someone joins the game.
		/// </summary>
		protected void ServerHooks_Join(JoinEventArgs args)
		{
            
		}

		protected async void GameHooks_PostInitialize(EventArgs e)
		{
			await Parent.BindToWorldAsync();
		}

		protected async void PlayerHooks_PlayerPostLogin(TShockAPI.Hooks.PlayerPostLoginEventArgs e)
		{
            IBankAccount account;

            if ((account = Parent.GetBankAccount(e.Player)) == null) {
                if ((account = await Parent.CreatePlayerAccountAsync(e.Player)) == null) {
                    TShockAPI.Log.ConsoleError("seconomy error:  Creating account for {0} failed.", e.Player.Name);
                }
                return;
            }

            await account.SyncBalanceAsync();
			await Task.Delay(2500);

			e.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(26, "You have {0}."), 
                account.Balance.ToLongString());
		}

		/// <summary>
		/// Occurs when the pay run timer is elapsed, and gives the IntervalPayAmount to each player
		/// that has not been idle since the configured idle threshold.  Idle players are not paid.
		/// </summary>
		protected void PayRunTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Money payAmount = 0;
            TimeSpan? idleSince;
            IBankAccount playerAccount;

			if (Parent.Configuration.PayIntervalMinutes <= 0 
                || string.IsNullOrEmpty(Parent.Configuration.IntervalPayAmount) == true
				|| Money.TryParse(Parent.Configuration.IntervalPayAmount, out payAmount) == false 
                || payAmount <= 0) {
					return;
			}

            foreach (TSPlayer player in TShockAPI.TShock.Players) {
                if (player == null 
                    || Parent == null
                    || (idleSince = Parent.PlayerIdleSince(player.TPlayer)) == null
                    || idleSince.Value.TotalMinutes > Parent.Configuration.IdleThresholdMinutes
                    || (playerAccount = Parent.GetBankAccount(player)) == null) {
                    continue;
                }

                Parent.WorldAccount.TransferTo(playerAccount, 
                    payAmount, 
                    Journal.BankAccountTransferOptions.AnnounceToReceiver, 
                    "being awesome",
                    "being awesome");
            }
		}

		/// <summary>
		/// Occurs when an exception happens on a task thread.
		/// </summary>
		protected void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (e.Observed == true) {
				return;
			}

			TShockAPI.Log.ConsoleError(SEconomyPlugin.Locale.StringOrDefault(27, "seconomy async: error occurred on a task thread: ") + e.Exception.Flatten().ToString());
			e.SetObserved();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing == true) {
				TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= PlayerHooks_PlayerPostLogin;
				Parent.RunningJournal.BankTransferCompleted -= BankAccount_BankTransferCompleted;
				TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
                PayRunTimer.Elapsed -= PayRunTimer_Elapsed;
                PayRunTimer.Dispose();

				ServerApi.Hooks.GamePostInitialize.Deregister(this.Parent.PluginInstance, GameHooks_PostInitialize);
				ServerApi.Hooks.ServerJoin.Deregister(this.Parent.PluginInstance, ServerHooks_Join);
				ServerApi.Hooks.ServerLeave.Deregister(this.Parent.PluginInstance, ServerHooks_Leave);
				ServerApi.Hooks.NetGetData.Deregister(this.Parent.PluginInstance, NetHooks_GetData);
			}
		}
	}
}
