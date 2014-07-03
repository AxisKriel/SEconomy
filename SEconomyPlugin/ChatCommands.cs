using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TShockAPI.Extensions;

namespace Wolfje.Plugins.SEconomy {
	internal class ChatCommands : IDisposable {
		SEconomy Parent { get; set; }

		internal ChatCommands(SEconomy parent)
		{
			this.Parent = parent;
			TShockAPI.Commands.ChatCommands.Add(new TShockAPI.Command(Chat_BankCommand, "bank") { AllowServer = true });
		}

		protected async void Chat_BankCommand(TShockAPI.CommandArgs args)
		{
			//The initator of the command with bank account...
			Economy.EconomyPlayer selectedPlayer = Parent.GetEconomyPlayerSafe(args.Player.Index);
			Economy.EconomyPlayer caller = Parent.GetEconomyPlayerSafe(args.Player.Index);
			string namePrefix = "Your";

			if (args.Parameters.Count == 0) {
				args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(27, "This server is running SEconomy v{0} by Wolfje"), Parent.PluginInstance.Version);
				args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(28, "Download here: http://plugins.tw.id.au"));
				args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(29, "You can:"));

				args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(30, "* View your balance with /bank bal"));

				if (args.Player.Group.HasPermission("bank.transfer")) {
					args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(31, "* Trade players with /bank pay <player> <amount>"));
				}

				if (args.Player.Group.HasPermission("bank.viewothers")) {
					args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(32, "* View other people's balance with /bank bal <player>"));
				}

				if (args.Player.Group.HasPermission("bank.worldtransfer")) {
					args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(33, "* Spawn/delete money with /bank give|take <player> <amount>"));
				}

				if (args.Player.Group.HasPermission("bank.mgr")) {
					args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(34, "* Spawn the account manager GUI on the server with /bank mgr"));
				}

				if (args.Player.Group.HasPermission("bank.savejournal")) {
					args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(35, "* Save the journal with /bank savejournal"));
				}

				if (args.Player.Group.HasPermission("bank.loadjournal")) {
					args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(36, "* Load the journal with /bank loadjournal"));
				}

				if (args.Player.Group.HasPermission("bank.squashjournal")) {
					args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(37, "* Compress the journal with /bank squashjournal"));
				}

				return;
			}

			if (args.Parameters[0].Equals("reset", StringComparison.CurrentCultureIgnoreCase)) {
				if (args.Player.Group.HasPermission("seconomy.reset")) {
					if (args.Parameters.Count >= 2 && !string.IsNullOrEmpty(args.Parameters[1])) {
						Economy.EconomyPlayer targetPlayer = Parent.GetEconomyPlayerSafe(args.Parameters[1]);

						if (targetPlayer != null && targetPlayer.BankAccount != null) {
							args.Player.SendInfoMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(38, "seconomy reset: Resetting {0}'s account."), targetPlayer.TSPlayer.Name));
							targetPlayer.BankAccount.Transactions.Clear();
							await targetPlayer.BankAccount.SyncBalanceAsync();
							args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(39, "seconomy reset:  Reset complete."));
						} else {
							args.Player.SendErrorMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(40, "seconomy reset: Cannot find player \"{0}\" or no bank account found."), args.Parameters[1]));
						}
					}
				} else {
					args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(41, "seconomy reset: You do not have permission to perform this command."));
				}
			}

			//Bank balance
			if (args.Parameters[0].Equals("bal", StringComparison.CurrentCultureIgnoreCase)
				|| args.Parameters[0].Equals("balance", StringComparison.CurrentCultureIgnoreCase)) {


				//The command supports viewing other people's balance if the caller has permission
				if (args.Player.Group.HasPermission("bank.viewothers")) {
					if (args.Parameters.Count >= 2) {
						selectedPlayer = Parent.GetEconomyPlayerSafe(args.Parameters[1]);
					}

					if (selectedPlayer != null) {
						namePrefix = selectedPlayer.TSPlayer.Name + "'s";
					}
				}

				if (selectedPlayer != null && selectedPlayer.BankAccount != null) {
					if (!selectedPlayer.BankAccount.IsAccountEnabled && !args.Player.Group.HasPermission("bank.viewothers")) {
						args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(42, "bank balance: your account is disabled"));
					} else {
						args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(43, "balance: {0} {1}"), selectedPlayer.BankAccount.Balance.ToLongString(true), 
							selectedPlayer.BankAccount.IsAccountEnabled ? "" : SEconomyPlugin.Locale.StringOrDefault(44, "(disabled)"));
					}
				} else {
					args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(45, "bank balance: Cannot find player or no bank account."));
				}
			} else if (args.Parameters[0].Equals("mgr")) {
				if (args.Player.Group.HasPermission("bank.mgr")) {

					if (args.Player is TShockAPI.TSServerPlayer) {
						Thread t = new Thread(() => {
							Forms.CAccountManagementWnd wnd = new Forms.CAccountManagementWnd(Parent);
							TShockAPI.Log.ConsoleInfo(SEconomyPlugin.Locale.StringOrDefault(46,"seconomy mgr: opening bank manager window"));

							//writing the journal is not possible when you're fucking with it in the manager
							//last thing you want is for half baked changes to be pushed to disk
							Parent.RunningJournal.BackupsEnabled = false;

							try {
								wnd.ShowDialog();
							} catch (Exception ex) {
								TShockAPI.Log.ConsoleError(SEconomyPlugin.Locale.StringOrDefault(47, "seconomy mgr: Window closed because it crashed: ") + ex.ToString());
							}

							Parent.RunningJournal.BackupsEnabled = true;
							TShockAPI.Log.ConsoleInfo(SEconomyPlugin.Locale.StringOrDefault(48, "seconomy management: window closed"));
						});

						t.SetApartmentState(ApartmentState.STA);
						t.Start();
					} else {
						args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(49, "Only the console can do that."));
					}
				}

			} else if (args.Parameters[0].Equals("savejournal")) {
				if (args.Player.Group.HasPermission("bank.savejournal")) {
					args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(50, "seconomy xml: Backing up transaction journal."));

					await Parent.RunningJournal.SaveJournalAsync();
				}

			} else if (args.Parameters[0].Equals("loadjournal")) {
				if (args.Player.Group.HasPermission("bank.loadjournal")) {
					args.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(51, "seconomy xml: Loading transaction journal from file"));

					await Parent.RunningJournal.LoadJournalAsync();
				}

			} else if (args.Parameters[0].Equals("squashjournal", StringComparison.CurrentCultureIgnoreCase)) {
				if (args.Player.Group.HasPermission("bank.squashjournal")) {
					await Parent.RunningJournal.SquashJournalAsync();
					await Parent.RunningJournal.SaveJournalAsync();
				} else {
					args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(52, "bank squashjournal: You do not have permission to perform this command."));
				}
			} else if (args.Parameters[0].Equals("pay", StringComparison.CurrentCultureIgnoreCase)
				|| args.Parameters[0].Equals("transfer", StringComparison.CurrentCultureIgnoreCase)
				|| args.Parameters[0].Equals("tfr", StringComparison.CurrentCultureIgnoreCase)) {
				//Player-to-player transfer

				if (selectedPlayer.TSPlayer.Group.HasPermission("bank.transfer")) {
					// /bank pay wolfje 1p
					if (args.Parameters.Count >= 3) {
						selectedPlayer = Parent.GetEconomyPlayerSafe(args.Parameters[1]);
						Money amount = 0;

						if (selectedPlayer == null) {
							args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(53, "Cannot find player by the name of {0}."), args.Parameters[1]);
						} else {
							if (Money.TryParse(args.Parameters[2], out amount)) {

								//Instruct the world bank to give the player money.
								caller.BankAccount.TransferTo(selectedPlayer.BankAccount, amount, Journal.BankAccountTransferOptions.AnnounceToReceiver | Journal.BankAccountTransferOptions.AnnounceToSender | Journal.BankAccountTransferOptions.IsPlayerToPlayerTransfer, "", string.Format("SE: tfr: {0} to {1} for {2}", caller.TSPlayer.Name, selectedPlayer.TSPlayer.Name, amount.ToString()));
							} else {
								args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(54, "bank give: \"{0}\" isn't a valid amount of money."), args.Parameters[2]);
							}
						}
					} else {
						args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(55, "Usage: /bank pay [Player] [Amount]"));
					}
				} else {
					args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(56, "bank pay: You don't have permission to do that."));
				}

			} else if (args.Parameters[0].Equals("give", StringComparison.CurrentCultureIgnoreCase)
			   || args.Parameters[0].Equals("take", StringComparison.CurrentCultureIgnoreCase)) {
				//World-to-player transfer

				if (selectedPlayer.TSPlayer.Group.HasPermission("bank.worldtransfer")) {
					// /bank give wolfje 1p
					if (args.Parameters.Count >= 3) {
						selectedPlayer = Parent.GetEconomyPlayerSafe(args.Parameters[1]);
						Money amount = 0;

						if (selectedPlayer == null) {
							args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(52, "Cannot find player by the name of {0}."), args.Parameters[1]);
						} else {
							if (Money.TryParse(args.Parameters[2], out amount)) {

								//eliminate a double-negative.  saying "take Player -1p1c" will give them 1 plat 1 copper!
								if (args.Parameters[0].Equals("take", StringComparison.CurrentCultureIgnoreCase) && amount > 0) {
									amount = -amount;
								}

								//Instruct the world bank to give the player money.
								Parent.WorldAccount.TransferTo(selectedPlayer.BankAccount, amount, Journal.BankAccountTransferOptions.AnnounceToReceiver, "", string.Format("SE: pay: {0} to {1} ", amount.ToString(), selectedPlayer.TSPlayer.Name));
							} else {
								args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(53, "bank give: \"{0}\" isn't a valid amount of money."), args.Parameters[2]);
							}
						}
					} else {
						args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(57, "Usage: /bank give|take [Player] [Amount]"));
					}
				} else {
					args.Player.SendErrorMessage(SEconomyPlugin.Locale.StringOrDefault(58, "bank give: You don't have permission to do that."));
				}
			}
		}


		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing == true) {
				TShockAPI.Command bankCommand = TShockAPI.Commands.ChatCommands.FirstOrDefault(i => i.Name == "bank" && i.CommandDelegate == Chat_BankCommand);
				if (bankCommand != null) {
					TShockAPI.Commands.ChatCommands.Remove(bankCommand);
				}
			}
		}
	}
}
