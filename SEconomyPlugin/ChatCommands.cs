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
				args.Player.SendInfoMessage("This server is running SEconomy v{0}", Parent.PluginInstance.Version);
				args.Player.SendInfoMessage("You can:");

				args.Player.SendInfoMessage("* View your balance with /bank bal");

				if (args.Player.Group.HasPermission("bank.transfer")) {
					args.Player.SendInfoMessage("* Trade players with /bank pay <player> <amount>");
				}

				if (args.Player.Group.HasPermission("bank.viewothers")) {
					args.Player.SendInfoMessage("* View other people's balance with /bank bal <player>");
				}

				if (args.Player.Group.HasPermission("bank.worldtransfer")) {
					args.Player.SendInfoMessage("* Spawn/delete money with /bank give|take <player> <amount>");
				}

				if (args.Player.Group.HasPermission("bank.mgr")) {
					args.Player.SendInfoMessage("* Spawn the account manager GUI on the server with /bank mgr");
				}

				if (args.Player.Group.HasPermission("bank.savejournal")) {
					args.Player.SendInfoMessage("* Save the journal with /bank savejournal");
				}

				if (args.Player.Group.HasPermission("bank.loadjournal")) {
					args.Player.SendInfoMessage("* Load the journal with /bank loadjournal");
				}

				if (args.Player.Group.HasPermission("bank.squashjournal")) {
					args.Player.SendInfoMessage("* Compress the journal with /bank squashjournal");
				}

				return;
			}

			if (args.Parameters[0].Equals("reset", StringComparison.CurrentCultureIgnoreCase)) {
				if (args.Player.Group.HasPermission("seconomy.reset")) {
					if (args.Parameters.Count >= 2 && !string.IsNullOrEmpty(args.Parameters[1])) {
						Economy.EconomyPlayer targetPlayer = Parent.GetEconomyPlayerSafe(args.Parameters[1]);

						if (targetPlayer != null && targetPlayer.BankAccount != null) {
							args.Player.SendInfoMessage("seconomy reset: Resetting " + targetPlayer.TSPlayer.Name + "'s account.");
							targetPlayer.BankAccount.Transactions.Clear();
							await targetPlayer.BankAccount.SyncBalanceAsync();
							args.Player.SendInfoMessage("seconomy reset:  Reset complete.");
						} else {
							args.Player.SendErrorMessage("seconomy reset: Cannot find player \"" + args.Parameters[1] + "\" or no bank account found.");
						}
					}
				} else {
					args.Player.SendErrorMessage("seconomy reset: You do not have permission to perform this command.");
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
						args.Player.SendErrorMessage("bank balance: your account is disabled");
					} else {
						args.Player.SendInfoMessage("{1} balance: {0} {2}", selectedPlayer.BankAccount.Balance.ToLongString(true), namePrefix, selectedPlayer.BankAccount.IsAccountEnabled ? "" : "(disabled)");
					}
				} else {
					args.Player.SendInfoMessage("bank balance: Cannot find player or no bank account.");
				}
			} else if (args.Parameters[0].Equals("mgr")) {
				if (args.Player.Group.HasPermission("bank.mgr")) {

					if (args.Player is TShockAPI.TSServerPlayer) {

						Thread t = new Thread(() => {
							Forms.CAccountManagementWnd wnd = new Forms.CAccountManagementWnd(Parent);
							TShockAPI.Log.ConsoleInfo("seconomy management: opening bank manager window");

							//writing the journal is not possible when you're fucking with it in the manager
							//last thing you want is for half baked changes to be pushed to disk
							Parent.RunningJournal.BackupsEnabled = false;

							try {
								wnd.ShowDialog();
							} catch (Exception ex) {
								TShockAPI.Log.ConsoleError("seconomy mgr: Window closed because it crashed: " + ex.ToString());
							}

							Parent.RunningJournal.BackupsEnabled = true;
							TShockAPI.Log.ConsoleInfo("seconomy management: window closed");
						});

						t.SetApartmentState(ApartmentState.STA);
						t.Start();
					} else {
						args.Player.SendErrorMessage("Only the console can do that.");
					}
				}

			} else if (args.Parameters[0].Equals("savejournal")) {
				if (args.Player.Group.HasPermission("bank.savejournal")) {
					args.Player.SendInfoMessage("seconomy xml: Backing up transaction journal.");

					await Parent.RunningJournal.SaveJournalAsync();
				}

			} else if (args.Parameters[0].Equals("loadjournal")) {
				if (args.Player.Group.HasPermission("bank.loadjournal")) {
					args.Player.SendInfoMessage("seconomy xml: Loading transaction journal from file");

					await Parent.RunningJournal.LoadJournalAsync();
				}

			} else if (args.Parameters[0].Equals("squashjournal", StringComparison.CurrentCultureIgnoreCase)) {
				if (args.Player.Group.HasPermission("bank.squashjournal")) {

					await Parent.RunningJournal.SquashJournalAsync();
					await Parent.RunningJournal.SaveJournalAsync();
				} else {
					args.Player.SendErrorMessage("bank squashjournal: You do not have permission to perform this command.");
				}
			} else if (args.Parameters[0].Equals("dump", StringComparison.CurrentCultureIgnoreCase)) {
				await Task.Factory.StartNew(() => {
					TShockAPI.Log.ConsoleInfo("seconomy dump: starting dump");
					Parent.RunningJournal.DumpSummary();
					TShockAPI.Log.ConsoleInfo("seconomy dump: dump finished");
				});
			} else if (args.Parameters[0].Equals("ena", StringComparison.CurrentCultureIgnoreCase)
				|| args.Parameters[0].Equals("enable", StringComparison.CurrentCultureIgnoreCase)
				|| args.Parameters[0].Equals("dis", StringComparison.CurrentCultureIgnoreCase)
				|| args.Parameters[0].Equals("disable", StringComparison.CurrentCultureIgnoreCase)) {
				//Account enable

				//Flag to set the account to
				bool enableAccount = args.Parameters[0].Equals("ena", StringComparison.CurrentCultureIgnoreCase) || args.Parameters[0].Equals("enable", StringComparison.CurrentCultureIgnoreCase);

				if (args.Player.Group.HasPermission("bank.modifyothers")) {
					if (args.Parameters.Count >= 2) {
						selectedPlayer = Parent.GetEconomyPlayerSafe(args.Parameters[1]);
					}

					if (selectedPlayer != null) {
						namePrefix = selectedPlayer.TSPlayer.Name + "'s";
					}
				}

				if (selectedPlayer != null && selectedPlayer.BankAccount != null) {
					//selectedPlayer.BankAccount.SetAccountEnabled(args.Player.Index, enableAccount);
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
							args.Player.SendErrorMessage("Cannot find player by the name of {0}.", args.Parameters[1]);
						} else {
							if (Money.TryParse(args.Parameters[2], out amount)) {

								//Instruct the world bank to give the player money.
								caller.BankAccount.TransferTo(selectedPlayer.BankAccount, amount, Journal.BankAccountTransferOptions.AnnounceToReceiver | Journal.BankAccountTransferOptions.AnnounceToSender | Journal.BankAccountTransferOptions.IsPlayerToPlayerTransfer, "", string.Format("SE: tfr: {0} to {1} for {2}", caller.TSPlayer.Name, selectedPlayer.TSPlayer.Name, amount.ToString()));
							} else {
								args.Player.SendErrorMessage("bank give: \"{0}\" isn't a valid amount of money.", args.Parameters[2]);
							}
						}
					} else {
						args.Player.SendErrorMessage("Usage: /bank pay <Player> <Amount>");
					}
				} else {
					args.Player.SendErrorMessage("bank pay: You don't have permission to do that.");
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
							args.Player.SendErrorMessage("Cannot find player by the name of {0}.", args.Parameters[1]);
						} else {
							if (Money.TryParse(args.Parameters[2], out amount)) {

								//eliminate a double-negative.  saying "take Player -1p1c" will give them 1 plat 1 copper!
								if (args.Parameters[0].Equals("take", StringComparison.CurrentCultureIgnoreCase) && amount > 0) {
									amount = -amount;
								}

								//Instruct the world bank to give the player money.
								Parent.WorldAccount.TransferTo(selectedPlayer.BankAccount, amount, Journal.BankAccountTransferOptions.AnnounceToReceiver, "", string.Format("SE: pay: {0} to {1} ", amount.ToString(), selectedPlayer.TSPlayer.Name));
							} else {
								args.Player.SendErrorMessage("bank give: \"{0}\" isn't a valid amount of money.", args.Parameters[2]);
							}
						}
					} else {
						args.Player.SendErrorMessage("Usage: /bank give|take <Player> <Amount");
					}
				} else {
					args.Player.SendErrorMessage("bank give: You don't have permission to do that.");
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
