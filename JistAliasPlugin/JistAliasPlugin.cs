using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolfje.Plugins.SEconomy.CmdAliasModule;

namespace Wolfje.Plugins.SEconomy.JistAliasPlugin {


	public class JistAliasPlugin {
		public static readonly List<JScriptAliasCommand> jsAliases = new List<JScriptAliasCommand>();

		public void ParseJSCommands()
		{
			lock (jsAliases) {
				foreach (JScriptAliasCommand jCmd in jsAliases) {
					TShockAPI.Command newCommand = new TShockAPI.Command(jCmd.Permissions, ChatCommand_AliasExecuted, new string[] { jCmd.CommandAlias, "cmdalias." + jCmd.CommandAlias }) { AllowServer = true };
					TShockAPI.Commands.ChatCommands.Add(newCommand);
				}
			}
		}

		private async void ChatCommand_AliasExecuted(TShockAPI.CommandArgs e)
		{
			string commandIdentifier = "";
			KeyValuePair<string, AliasCommand> cooldownReference;
			TimeSpan timeSinceLastUsedCommand = TimeSpan.MaxValue;
			Journal.BankTransferEventArgs trans = null;
			Money commandCost = 0;
			Economy.EconomyPlayer ePlayer = null;
			
			foreach (JScriptAliasCommand alias in jsAliases.Where(i => i.CommandAlias == commandIdentifier)) {
				if (alias == null) {
					return;
				}

				//cooldown key is a pair of the user's character name, and the command they have called.
				//cooldown value is a DateTime they last used the command.
				cooldownReference = new KeyValuePair<string, AliasCommand>(e.Player.Name, alias);
				if (CmdAliasModule.CmdAliasPlugin.Instance.CooldownList.ContainsKey(cooldownReference)) {
					//UTC time so we don't get any daylight saving shit cuntery
					timeSinceLastUsedCommand = DateTime.UtcNow.Subtract(CmdAliasModule.CmdAliasPlugin.Instance.CooldownList[cooldownReference]);
				}

				if (timeSinceLastUsedCommand.TotalSeconds <= alias.CooldownSeconds
					&& e.Player.Group.HasPermission("aliascmd.bypasscooldown") == false) {
					e.Player.SendErrorMessage("{0}: You need to wait {1:0} more seconds to be able to use that.", 
						alias.CommandAlias,
						(alias.CooldownSeconds - timeSinceLastUsedCommand.TotalSeconds));
					return;
				}

				if (string.IsNullOrEmpty(alias.Cost) == true
					|| Money.TryParse(alias.Cost, out commandCost) == false
					|| e.Player.Group.HasPermission("aliascmd.bypasscost") == true
					|| commandCost <= 0) {
					//Command is free
					//Jist.JistPlugin.CallScriptFunction(alias.func, e.Player, e.Parameters);
					return;
				}

				if ((ePlayer = SEconomyPlugin.Instance.GetEconomyPlayerSafe(e.Player.Index)) == null) {
					e.Player.SendErrorMessage("This command costs money and you don't have a bank account.  Please log in first.");
					return;
				}

				if (ePlayer.BankAccount.IsAccountEnabled == false) {
					e.Player.SendErrorMessage("You cannot use this command because your account is disabled.");
					return;
				}

				if (ePlayer.BankAccount.Balance <= commandCost) {
					e.Player.SendErrorMessage("This command costs {0}. You need {1} more to be able to use this.", commandCost.ToLongString(),
						((Money)(ePlayer.BankAccount.Balance - commandCost)).ToLongString());
					return;
				}

				//Take money off the player, and indicate that this is a payment for something tangible.
				trans = await ePlayer.BankAccount.TransferToAsync(SEconomyPlugin.Instance.WorldAccount,
					commandCost,
					Journal.BankAccountTransferOptions.AnnounceToSender | Journal.BankAccountTransferOptions.IsPayment,
					"",
					string.Format("AC: {0} cmd {1}", ePlayer.TSPlayer.Name, alias.CommandAlias));

				if (trans.TransferSucceeded == false) {
					e.Player.SendErrorMessage("Your payment failed.");
					return;
				}

				//Jist.JistPlugin.CallScriptFunction(alias.func, ePlayer.TSPlayer, e.Parameters);

				//populate the cooldown list.  This dictionary does not go away when people leave so they can't
				//reset cooldowns by simply logging out or disconnecting.  They can reset it however by logging into 
				//a different account.
				if (CmdAliasModule.CmdAliasPlugin.Instance.CooldownList.ContainsKey(cooldownReference)) {
					CmdAliasModule.CmdAliasPlugin.Instance.CooldownList[cooldownReference] = DateTime.UtcNow;
				} else {
					CmdAliasModule.CmdAliasPlugin.Instance.CooldownList.Add(cooldownReference, DateTime.UtcNow);
				}
			}
		}
	}
}
