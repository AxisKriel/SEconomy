using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Wolfje.Plugins.SEconomy.Journal.MySQLHistory
{
	public class MySQLHistoryJournal : ITransactionJournal
	{
		protected string connectionString;
		protected Configuration.SQLConnectionProperties sqlProperties;
		protected SEconomy instance;

		protected MySqlConnection mysqlConnection;

		public MySQLHistoryJournal(SEconomy instance, Configuration.SQLConnectionProperties props)
		{
			this.instance = instance;
			this.sqlProperties = props;

			this.connectionString = string.Format("server={0};user id={1};password={2};connect timeout=60;", 
				sqlProperties.DbHost,
				sqlProperties.DbUsername, sqlProperties.DbPassword);

			if (string.IsNullOrEmpty(sqlProperties.DbOverrideConnectionString) == false) {
				this.connectionString = sqlProperties.DbOverrideConnectionString;
			}

			this.mysqlConnection = new MySqlConnection(connectionString);
		}

		public MySqlConnection Connection {
			get {
				return new MySqlConnection(connectionString + "database=" + sqlProperties.DbName);
			}
		}

		public MySqlConnection ConnectionNoCatalog {
			get {
				return new MySqlConnection(connectionString);
			}
		}

		#region ITransactionJournal implementation

		public event EventHandler<PendingTransactionEventArgs> BankTransactionPending;
		public event EventHandler<BankTransferEventArgs> BankTransferCompleted;

		public IBankAccount AddBankAccount(string UserAccountName, long WorldID, BankAccountFlags Flags, string iDonoLol)
		{
			return AddBankAccount(new MySQLBankAccount(this) {
				UserAccountName = UserAccountName,
				Description = iDonoLol,
				WorldID = WorldID,
				Flags = Flags
			});
		}
		public IBankAccount AddBankAccount(IBankAccount Account)
		{
			throw new NotImplementedException();
		}


		public IBankAccount GetBankAccountByName(string UserAccountName)
		{
			throw new NotImplementedException();
		}
		public IBankAccount GetBankAccount(long BankAccountK)
		{
			throw new NotImplementedException();
		}
		public IEnumerable<IBankAccount> GetBankAccountList(long BankAccountK)
		{
			throw new NotImplementedException();
		}
		public Task DeleteBankAccountAsync(long BankAccountK)
		{
			throw new NotImplementedException();
		}
		public void SaveJournal()
		{
			throw new NotImplementedException();
		}
		public Task SaveJournalAsync()
		{
			throw new NotImplementedException();
		}
		public bool LoadJournal()
		{
			throw new NotImplementedException();
		}
		public Task<bool> LoadJournalAsync()
		{
			throw new NotImplementedException();
		}
		public void BackupJournal()
		{
			throw new NotImplementedException();
		}
		public Task BackupJournalAsync()
		{
			throw new NotImplementedException();
		}
		public Task SquashJournalAsync()
		{
			throw new NotImplementedException();
		}
		public BankTransferEventArgs TransferBetween(IBankAccount FromAccount, IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			throw new NotImplementedException();
		}
		public Task<BankTransferEventArgs> TransferBetweenAsync(IBankAccount FromAccount, IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			throw new NotImplementedException();
		}
		public IBankAccount GetWorldAccount()
		{
			throw new NotImplementedException();
		}
		public void DumpSummary()
		{
			throw new NotImplementedException();
		}
		public void CleanJournal(PurgeOptions options)
		{
			throw new NotImplementedException();
		}
		public SEconomy SEconomyInstance {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}
		public List<IBankAccount> BankAccounts {
			get {
				throw new NotImplementedException();
			}
		}
		public IEnumerable<ITransaction> Transactions {
			get {
				throw new NotImplementedException();
			}
		}
		public bool JournalSaving {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}
		public bool BackupsEnabled {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}
		#endregion
		#region IDisposable implementation
		public void Dispose()
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}

