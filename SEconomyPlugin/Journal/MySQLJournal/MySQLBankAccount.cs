using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI.DB;
using TShockAPI.Extensions;
using Wolfje.Plugins.SEconomy.Extensions;


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
			get {
				List<ITransaction> tranList = new List<ITransaction>();

				using (QueryResult reader = journal.Connection.QueryReader("SELECT * FROM `bank_account_transaction` WHERE `bank_account_fk` = " + this.BankAccountK + ";")) {
					foreach (IDataReader item in reader.AsEnumerable()) {
						MySQLTransaction bankTrans = new MySQLTransaction((IBankAccount)this) {
							BankAccountFK = item.Get<long>("bank_account_fk"),
							BankAccountTransactionK = item.Get<long>("bank_account_transaction_id"),
							BankAccountTransactionFK = item.Get<long?>("bank_account_transaction_fk").GetValueOrDefault(-1L),
							Flags = (BankAccountTransactionFlags)item.Get<int>("flags"),
							TransactionDateUtc = item.GetDateTime(reader.Reader.GetOrdinal("transaction_date_utc")),
							Amount = item.Get<long>("amount"),
							Message = item.Get<string>("message")
						};

						tranList.Add(bankTrans);
					}
				}

				return tranList;
			}
		}

		public ITransaction AddTransaction(ITransaction Transaction)
		{
			throw new NotSupportedException("AddTransaction via interface is not supported for SQL journals.  To transfer money between accounts, use the TransferTo methods instead.");
		}

		public void ResetAccountTransactions(long BankAccountK)
		{
			try {
				journal.Connection.Query("DELETE FROM `bank_account_transaction` WHERE `bank_account_fk` = " + this.BankAccountK + ";");
			} catch {
				TShockAPI.Log.ConsoleError(" seconomy mysql: MySQL command error in ResetAccountTransactions");
			}
		}
		public async Task ResetAccountTransactionsAsync(long BankAccountK)
		{
			await Task.Run(() => ResetAccountTransactions(BankAccountK));
		}

		public async Task SyncBalanceAsync()
		{
			await Task.Run(() => SyncBalance());
		}

		public void SyncBalance()
		{
			try {
				this.Balance = Convert.ToInt64(journal.Connection.QueryScalar<decimal>("SELECT IFNULL(SUM(Amount), 0) FROM `bank_account_transaction` WHERE `bank_account_transaction`.`bank_account_fk` = " + this.BankAccountK + ";"));
			} catch (Exception ex) {
				TShockAPI.Log.ConsoleError(" seconomy mysql: SQL error in SyncBalance: " + ex.Message);
			}
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

		public override string ToString()
		{
			return string.Format("MySQLBankAccount {0} UserAccountName={1} Balance={2}", this.BankAccountK, this.UserAccountName, this.BankAccountK);
		}
	}
}
