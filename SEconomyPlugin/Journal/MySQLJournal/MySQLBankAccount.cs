using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Wolfje.Plugins.SEconomy.Journal.MySQLJournal {
	public class MySQLBankAccount : IBankAccount {
		protected MySQLTransactionJournal journal;

		public MySQLBankAccount(MySQLTransactionJournal journal)
		{
			this.journal = journal;
		}

		#region IBankAccount Members

		public ITransactionJournal OwningJournal
		{
			get { return journal; }
		}

		public long BankAccountK { get; set; }

		public string OldBankAccountK { get; set; }

		public string UserAccountName { get; set; }

		public long WorldID { get; set; }

		public BankAccountFlags Flags { get; set; }

		public string Description { get; set; }

		public Money Balance { get; set; }

		public Economy.EconomyPlayer Owner
		{
			get
			{
				return OwningJournal.SEconomyInstance.GetEconomyPlayerByBankAccountNameSafe(this.UserAccountName);
			}
		}

		public bool IsAccountEnabled
		{
			get { return (this.Flags & Journal.BankAccountFlags.Enabled) == Journal.BankAccountFlags.Enabled; }
		}

		public bool IsSystemAccount
		{
			get { return (this.Flags & Journal.BankAccountFlags.SystemAccount) == Journal.BankAccountFlags.SystemAccount; }
		}

		public bool IsLockedToWorld
		{
			get { return (this.Flags & Journal.BankAccountFlags.LockedToWorld) == Journal.BankAccountFlags.LockedToWorld; }
		}

		public bool IsPluginAccount
		{
			get { return (this.Flags & Journal.BankAccountFlags.PluginAccount) == Journal.BankAccountFlags.PluginAccount; }
		}

		public List<ITransaction> Transactions
		{
			get { return null; }
		}

		public ITransaction AddTransaction(ITransaction Transaction)
		{
			throw new NotSupportedException("AddTransaction via interface is not supported for SQL journals.  To transfer money between accounts, use the TransferTo methods instead.");
		}

		public void ResetAccountTransactions(long BankAccountK)
		{
			using (var db = journal.GetContext()) {
				Database.bank_account bankAccount = null;
				
				if ((bankAccount = db.bank_account.FirstOrDefault(i => i.bank_account_id == BankAccountK)) == null) {
					return;
				}

				try {
					bankAccount.bank_account_transactions.Clear();
					db.SaveChanges();
					this.Balance = 0;
				} catch (Exception ex) {
					TShockAPI.Log.ConsoleError(" seconomy mysql:  database error in ResetAccountTransactions: " + ex.ToString());
				}
			}
		}

		public async Task ResetAccountTransactionsAsync(long BankAccountK)
		{
			await Task.Run(() => ResetAccountTransactions(BankAccountK));
		}

		public void SyncBalance()
		{
			using (var db = journal.GetContext()) {
				IQueryable<Database.bank_account_transaction> accountTrans = db.bank_account_transaction.Where(i => i.bank_account_fk == this.BankAccountK);
				if (accountTrans == null || accountTrans.Count() == 0) {
					this.Balance = 0;
					return;
				}

				this.Balance = accountTrans.Sum(i => i.amount);
			}
		}

		public async Task SyncBalanceAsync()
		{
			await Task.Run(() => SyncBalance());
		}

		public BankTransferEventArgs TransferTo(IBankAccount Account, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			return journal.TransferBetween(this, Account, Amount, Options, TransactionMessage, JournalMessage);
		}

		public async Task<BankTransferEventArgs> TransferToAsync(int Index, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			Economy.EconomyPlayer ePlayer = null;
			if ((ePlayer = OwningJournal.SEconomyInstance.GetEconomyPlayerSafe(Index)) == null) {
				return null;
			}

			return await Task.Factory.StartNew(() => TransferTo(ePlayer.BankAccount, Amount, Options, TransactionMessage, JournalMessage));
		}

		public async Task<BankTransferEventArgs> TransferToAsync(IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			return await Task.Factory.StartNew(() => TransferTo(ToAccount, Amount, Options, TransactionMessage, JournalMessage));
		}

		#endregion
	}
}
