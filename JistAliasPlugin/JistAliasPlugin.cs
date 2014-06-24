using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolfje.Plugins.SEconomy.CmdAliasModule;

namespace Wolfje.Plugins.SEconomy.JistAliasPlugin {
	
    
	public class JistAliasPlugin {
		public static readonly List<JScriptAliasCommand> jsAliases = new List<JScriptAliasCommand>();

        public void ParseJSCommands() {
            lock (jsAliases) {
                foreach (JScriptAliasCommand jCmd in jsAliases) {
                    TShockAPI.Command newCommand = new TShockAPI.Command(jCmd.Permissions, ChatCommand_AliasExecuted, new string[] { jCmd.CommandAlias, "cmdalias." + jCmd.CommandAlias }) { AllowServer = true };
                    TShockAPI.Commands.ChatCommands.Add(newCommand);
                }
            }
        }

		private void ChatCommand_AliasExecuted(TShockAPI.CommandArgs e)
		{
			string commandIdentifier = "";

			foreach (JScriptAliasCommand alias in jsAliases.Where(i => i.CommandAlias == commandIdentifier)) {

				if (alias != null) {
					TimeSpan timeSinceLastUsedCommand = TimeSpan.MaxValue;
					//cooldown key is a pair of the user's character name, and the command they have called.
					//cooldown value is a DateTime they last used the command.
					KeyValuePair<string, AliasCommand> cooldownReference = new KeyValuePair<string, AliasCommand>(e.Player.Name, alias);

					if (CmdAliasModule.CmdAliasPlugin.CooldownList.ContainsKey(cooldownReference)) {
						//UTC time so we don't get any daylight saving shit cuntery
						timeSinceLastUsedCommand = DateTime.UtcNow.Subtract(CmdAliasModule.CmdAliasPlugin.CooldownList[cooldownReference]);
					}

					//has the time elapsed greater than the cooldown period?
					if (timeSinceLastUsedCommand.TotalSeconds >= alias.CooldownSeconds || e.Player.Group.HasPermission("aliascmd.bypasscooldown")) {
						Money commandCost = 0;
						Economy.EconomyPlayer ePlayer = SEconomyPlugin.GetEconomyPlayerSafe(e.Player.Index);

						if (!string.IsNullOrEmpty(alias.Cost) && Money.TryParse(alias.Cost, out commandCost) && !e.Player.Group.HasPermission("aliascmd.bypasscost") && commandCost > 0) {
							if (ePlayer.BankAccount != null) {

								if (!ePlayer.BankAccount.IsAccountEnabled) {
									e.Player.SendErrorMessageFormat("You cannot use this command because your account is disabled.");
								} else if (ePlayer.BankAccount.Balance >= commandCost) {

									//Take money off the player, and indicate that this is a payment for something tangible.
									Journal.BankTransferEventArgs trans = ePlayer.BankAccount.TransferTo(SEconomyPlugin.WorldAccount, commandCost, Journal.BankAccountTransferOptions.AnnounceToSender | Journal.BankAccountTransferOptions.IsPayment, "", string.Format("AC: {0} cmd {1}", ePlayer.TSPlayer.Name, alias.CommandAlias));
									if (trans.TransferSucceeded) {
										//DoCommands(alias, ePlayer.TSPlayer, e.Parameters);
										//Jist.JistPlugin.CallScriptFunction(alias.func, ePlayer.TSPlayer, e.Parameters);
									} else {
										e.Player.SendErrorMessageFormat("Your payment failed.");
									}
								} else {
									e.Player.SendErrorMessageFormat("This command costs {0}. You need {1} more to be able to use this.", commandCost.ToLongString(), ((Money)(ePlayer.BankAccount.Balance - commandCost)).ToLongString());
								}
							} else {
								e.Player.SendErrorMessageFormat("This command costs money and you don't have a bank account.  Please log in first.");
							}
						} else {
							//Command is free
							//Jist.JistPlugin.CallScriptFunction(alias.func, e.Player, e.Parameters);
						}

						//populate the cooldown list.  This dictionary does not go away when people leave so they can't
						//reset cooldowns by simply logging out or disconnecting.  They can reset it however by logging into 
						//a different account.
						if (CmdAliasModule.CmdAliasPlugin.CooldownList.ContainsKey(cooldownReference)) {
							CmdAliasModule.CmdAliasPlugin.CooldownList[cooldownReference] = DateTime.UtcNow;
						} else {
							CmdAliasModule.CmdAliasPlugin.CooldownList.Add(cooldownReference, DateTime.UtcNow);
						}

					} else {
						e.Player.SendErrorMessageFormat("{0}: You need to wait {1:0} more seconds to be able to use that.", alias.CommandAlias, (alias.CooldownSeconds - timeSinceLastUsedCommand.TotalSeconds));
					}
				}
			}
		}
	}
}
