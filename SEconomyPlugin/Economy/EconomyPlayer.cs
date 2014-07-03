using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Economy {

	public class EconomyPlayer {
		public Journal.IBankAccount BankAccount { get; internal set; }
		public PlayerControlFlags LastKnownState { get; internal set; }
		public DateTime IdleSince { get; internal set; }
		public int Index { get; set; }
		public TShockAPI.TSPlayer TSPlayer
		{
			get
			{
				if (Index < 0) {
					return TShockAPI.TSServerPlayer.Server;
				} else {
					return TShockAPI.TShock.Players[Index];
				}
			}
		}

		public EconomyPlayer(int index)
		{
			this.Index = index;
		}

		#region "Events"

		// <summary>
		// Fires when a bank account is successully loaded.
		// </summary>
		//public static event EventHandler PlayerBankAccountLoaded;
		#endregion


		/// <summary>
		/// Returns a TimeSpan representing the amount of time the user has been idle for
		/// </summary>
		public TimeSpan TimeSinceIdle
		{
			get
			{
				return DateTime.Now.Subtract(this.IdleSince);
			}
		}


		/// <summary>
		/// Ensures a bank account exists for the logged-in user and makes sure it's loaded properly.
		/// </summary>
		public async Task<Journal.IBankAccount> EnsureBankAccountExistsAsync()
		{
			Journal.IBankAccount account = SEconomyPlugin.Instance.RunningJournal.GetBankAccountByName(this.TSPlayer.UserAccountName);
			if (account == null) {
				account = await CreateAccountAsync();
			}

			this.BankAccount = account;
			await BankAccount.SyncBalanceAsync();
			return account;
		}

		async Task<Journal.IBankAccount> CreateAccountAsync()
		{
			Money startingMoney;
			Journal.IBankAccount newAccount = SEconomyPlugin.Instance.RunningJournal.AddBankAccount(this.TSPlayer.UserAccountName, Terraria.Main.worldID, Journal.BankAccountFlags.Enabled, "");

			TShockAPI.Log.ConsoleInfo(string.Format("seconomy: bank account for {0} created.", TSPlayer.UserAccountName));
			if (Money.TryParse(SEconomyPlugin.Instance.Configuration.StartingMoney, out startingMoney) && startingMoney > 0) {
				await SEconomyPlugin.Instance.WorldAccount.TransferToAsync(newAccount, startingMoney, Journal.BankAccountTransferOptions.AnnounceToReceiver, "starting out.", "starting out.");
			}

			return newAccount;
		}

		async void LoadBankAccount(long BankAccountK)
		{
			Journal.IBankAccount account = SEconomyPlugin.Instance.RunningJournal.GetBankAccount(BankAccountK);
			if (account == null) {
				TShockAPI.Log.ConsoleError(string.Format("seconomy: bank account for {0} failed.", TSPlayer.UserAccountName));
				this.TSPlayer.SendErrorMessage("It appears you don't have a bank account.");
				return;
			}

			this.BankAccount = account;
			await BankAccount.SyncBalanceAsync();
			TShockAPI.Log.ConsoleInfo(string.Format("seconomy: bank account for {0} loaded.", TSPlayer.UserAccountName));
		}
	}
}
