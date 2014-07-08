using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TShockAPI.DB;
using TShockAPI.Extensions;
using Wolfje.Plugins.SEconomy.Extensions;
using System.Data;
using System.Diagnostics;

namespace Wolfje.Plugins.SEconomy.Journal.MySQLJournal {
	public class MySQLTransactionJournal : ITransactionJournal {
		protected string connectionString;
		protected List<IBankAccount> bankAccounts;
		protected MySqlConnection mysqlConnection;

		public MySQLTransactionJournal(SEconomy instance, Configuration.SQLConnectionProperties sqlProperties)
		{
			if (string.IsNullOrEmpty(sqlProperties.DbOverrideConnectionString) == false) {
				this.connectionString = sqlProperties.DbOverrideConnectionString;
			}
			
			this.connectionString = string.Format("server={0};user id={2};database={1};password={3}", sqlProperties.DbHost, sqlProperties.DbName, 
				sqlProperties.DbUsername, sqlProperties.DbPassword);
			this.SEconomyInstance = instance;

			this.mysqlConnection = new MySqlConnection(connectionString);
		}

		#region ITransactionJournal Members

		public event EventHandler<BankTransferEventArgs> BankTransferCompleted;
		public event EventHandler<PendingTransactionEventArgs> BankTransactionPending;
		public event EventHandler<JournalLoadingPercentChangedEventArgs> JournalLoadingPercentChanged;

		public SEconomy SEconomyInstance { get; set; }

		public List<IBankAccount> BankAccounts
		{
			get { return bankAccounts; }
		}

		public IEnumerable<ITransaction> Transactions
		{
			get { return null; }
		}

		public MySqlConnection Connection
		{
			get
			{
				return mysqlConnection;
			}
		}

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
			long id = 0;
			string query = @"INSERT INTO `bank_account` 
									(user_account_name, world_id, flags, flags2, description)
								  VALUES (@0, @1, @2, @3, @4);";

			try {
				Connection.QueryIdentity(query, out id, Account.UserAccountName, Account.WorldID, 
					(int)Account.Flags, 0, Account.Description);
			} catch (Exception ex) {
				TShockAPI.Log.ConsoleError(" seconomy mysql: sql error adding bank account: " + ex.ToString());
				return null;
			}

			Account.BankAccountK = id;
			BankAccounts.Add(Account);
			
			return Account;
		}

		public IBankAccount GetBankAccountByName(string UserAccountName)
		{
			return bankAccounts.FirstOrDefault(i => i.UserAccountName == UserAccountName);
		}

		public IBankAccount GetBankAccount(long BankAccountK)
		{
			return bankAccounts.FirstOrDefault(i => i.BankAccountK == BankAccountK);
		}

		public IEnumerable<IBankAccount> GetBankAccountList(long BankAccountK)
		{
			return BankAccounts.Where(i => i.BankAccountK == BankAccountK);
		}

		public void DeleteBankAccount(long BankAccountK)
		{
			int affected = Connection.Query("DELETE FROM `bank_account` WHERE `bank_account_id` = @0", BankAccountK);
		}

		public bool JournalSaving { get; set; }

		public bool BackupsEnabled { get; set; }

		public void SaveJournal()
		{
			return; //stub
		}

		public async Task SaveJournalAsync()
		{
			await Task.FromResult<object>(null); //stub
		}

		public void LoadJournal()
		{
			long bankAccountCount = 0, tranCount = 0;
			int index = 0, oldPercent = 0;
			double percentComplete = 0;
			JournalLoadingPercentChangedEventArgs parsingArgs = new JournalLoadingPercentChangedEventArgs() {
				Label = "Loading"
			};

			try {
				if (JournalLoadingPercentChanged != null) {
					JournalLoadingPercentChanged(this, parsingArgs);
				}
				
				//TODO: Create schema

				bankAccounts = new List<IBankAccount>();
				bankAccountCount = Connection.QueryScalar<long>("SELECT COUNT(*) from `bank_account`;");
				tranCount = Connection.QueryScalar<long>("SELECT COUNT(*) FROM `bank_account_transaction`;");
				
				QueryResult bankAccountResult = Connection.QueryReader("SELECT * FROM `bank_account`;");
				
				Action<int> percentCompleteFunc = i => {
					percentComplete = (double)i / (double)bankAccountCount * 100;

					if (oldPercent != (int)percentComplete) {
						parsingArgs.Percent = (int)percentComplete;
						if (JournalLoadingPercentChanged != null) {
							JournalLoadingPercentChanged(this, parsingArgs);
						}
						oldPercent = (int)percentComplete;
					}
				};

				foreach (var acc in bankAccountResult.AsEnumerable()) {
					MySQLBankAccount sqlAccount = null;
					sqlAccount = new MySQLBankAccount(this) {
						BankAccountK = acc.Get<long>("bank_account_id"),
						Description = acc.Get<string>("description"),
						Flags = (BankAccountFlags)Enum.Parse(typeof(BankAccountFlags), acc.Get<int>("flags").ToString()),
						UserAccountName = acc.Get<string>("user_account_name"),
						WorldID = acc.Get<long>("world_id")
					};

					sqlAccount.SyncBalance();
					lock (bankAccounts) {
						bankAccounts.Add(sqlAccount);
					}

					Interlocked.Increment(ref index);
					percentCompleteFunc(index);
				}

				parsingArgs.Percent = 100;
				if (JournalLoadingPercentChanged != null) {
					JournalLoadingPercentChanged(this, parsingArgs);
				}

				Console.WriteLine("\r\n");
				Console.WriteLine(" Journal clean: {0} accounts, {1} transactions", BankAccounts.Count(), tranCount);
			} catch(Exception ex) {
				TShockAPI.Log.ConsoleError(" seconomy mysql: db error in LoadJournal: " + ex.ToString());
			} finally {
				
			}
		}

		public Task LoadJournalAsync()
		{
			return Task.Run(() => LoadJournal());
		}

		public void BackupJournal()
		{
			return; //stub
		}

		public async Task BackupJournalAsync()
		{
			await Task.FromResult<object>(null);
		}

		public Task SquashJournalAsync()
		{
			throw new NotImplementedException();
		}

		bool TransferMaySucceed(IBankAccount FromAccount, IBankAccount ToAccount, Money MoneyNeeded, Journal.BankAccountTransferOptions Options)
		{
			if (FromAccount == null || ToAccount == null) {
				return false;
			}

			return ((FromAccount.IsSystemAccount || FromAccount.IsPluginAccount || ((Options & Journal.BankAccountTransferOptions.AllowDeficitOnNormalAccount) == Journal.BankAccountTransferOptions.AllowDeficitOnNormalAccount)) || (FromAccount.Balance >= MoneyNeeded && MoneyNeeded > 0));
		}

		ITransaction BeginSourceTransaction(long BankAccountK, Money Amount, string Message)
		{
			MySQLTransaction trans = null;
			long idenitity = -1;
			string query = @"INSERT INTO `bank_account_transaction` 
								(bank_account_fk, amount, message, flags, flags2, transaction_date_utc)
							VALUES (@0, @1, @2, @3, @4, @5);
							";
			IBankAccount account = null;
			if ((account = GetBankAccount(BankAccountK)) == null) {
				return null;
			}
			trans = new MySQLTransaction(account) {
				Amount = (-1) * Amount,
				BankAccountFK = account.BankAccountK,
				Flags = BankAccountTransactionFlags.FundsAvailable,
				Message = Message,
				TransactionDateUtc = DateTime.UtcNow
			};

			try {
				Connection.QueryIdentity(query, out idenitity, trans.BankAccountFK, (long)trans.Amount, trans.Message,
					(int)BankAccountTransactionFlags.FundsAvailable, 0, DateTime.UtcNow);
			} catch (Exception ex) {
				TShockAPI.Log.ConsoleError(" seconomy mysql: Database error in BeginSourceTransaction: " + ex.Message);
				return null;
			}

			trans.BankAccountTransactionK = idenitity;

			return trans;
		}

		ITransaction FinishEndTransaction(IBankAccount ToAccount, Money Amount, string Message)
		{
			MySQLTransaction trans = null;
			long idenitity = -1;
			string query = @"INSERT INTO `bank_account_transaction` 
								(bank_account_fk, amount, message, flags, flags2, transaction_date_utc)
							VALUES (@0, @1, @2, @3, @4, @5);
							";
			IBankAccount account = null;
			if ((account = GetBankAccount(ToAccount.BankAccountK)) == null) {
				return null;
			}
			trans = new MySQLTransaction(account) {
				Amount = Amount,
				BankAccountFK = account.BankAccountK,
				Flags = BankAccountTransactionFlags.FundsAvailable,
				Message = Message,
				TransactionDateUtc = DateTime.UtcNow
			};

			try {
				Connection.QueryIdentity(query, out idenitity, trans.BankAccountFK, (long)trans.Amount, trans.Message,
					(int)BankAccountTransactionFlags.FundsAvailable, 0, DateTime.UtcNow);
			} catch (Exception ex) {
				TShockAPI.Log.ConsoleError(" seconomy mysql: Database error in FinishEndTransaction: " + ex.Message);
				return null;
			}

			trans.BankAccountTransactionK = idenitity;

			return trans;
		}

		public void BindTransactions(long SourceBankTransactionK, long DestBankTransactionK)
		{
			int updated = -1;
			string query = @"UPDATE `bank_account_transaction` 
							 SET `bank_account_transaction_fk` = @0
							 WHERE `bank_account_transaction_id` = @1";

			try {
				if ((updated = Connection.Query(query, SourceBankTransactionK, DestBankTransactionK)) != 1) {
					TShockAPI.Log.ConsoleError(" seconomy mysql:  Error in BindTransactions: updated row count was " + updated);
				}

				if ((updated = Connection.Query(query, DestBankTransactionK, SourceBankTransactionK)) != 1) {
					TShockAPI.Log.ConsoleError(" seconomy mysql:  Error in BindTransactions: updated row count was " + updated);
				}
			} catch (Exception ex) {
				TShockAPI.Log.ConsoleError(" seconomy mysql: Database error in BindTransactions: " + ex.Message);
				return;
			}
		}


		public BankTransferEventArgs TransferBetween(IBankAccount FromAccount, IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			long accountCount = -1;
			PendingTransactionEventArgs pendingTransaction = new PendingTransactionEventArgs(FromAccount, ToAccount, Amount, Options, TransactionMessage, JournalMessage);
			ITransaction sourceTran, destTran;
			BankTransferEventArgs args = new BankTransferEventArgs() {
				TransferSucceeded = false
			};
			string accountVerifyQuery = @"SELECT COUNT(*)
										  FROM `bank_account`
										  WHERE	`bank_account_id` = @0;";

			Stopwatch sw = new Stopwatch();
			if (SEconomyInstance.Configuration.EnableProfiler == true) {
				sw.Start();
			}
			if (ToAccount == null && TransferMaySucceed(FromAccount, ToAccount, Amount, Options) == false) {
				return args;
			}

			if ( (accountCount = Connection.QueryScalar<long>(accountVerifyQuery, FromAccount.BankAccountK)) != 1) {
				TShockAPI.Log.ConsoleError(" seconomy mysql: Source account " + FromAccount.BankAccountK + " does not exist.");
				return args;
			}

			if ( (accountCount = Connection.QueryScalar<long>(accountVerifyQuery, ToAccount.BankAccountK)) != 1) {
				TShockAPI.Log.ConsoleError(" seconomy mysql: Source account " + FromAccount.BankAccountK + " does not exist.");
				return args;
			}

			if (BankTransactionPending != null) {
				BankTransactionPending(this, pendingTransaction);
			}

			args.Amount = pendingTransaction.Amount;
			args.SenderAccount = pendingTransaction.FromAccount;
			args.ReceiverAccount = pendingTransaction.ToAccount;
			args.TransferOptions = Options;
			args.TransactionMessage = pendingTransaction.TransactionMessage;

			try {
				if ((sourceTran = BeginSourceTransaction(FromAccount.BankAccountK, Amount, JournalMessage)) == null) {
					throw new Exception("BeginSourceTransaction failed");
				}

				if ((destTran = FinishEndTransaction(ToAccount, Amount, JournalMessage)) == null) {
					throw new Exception("FinishEndTransaction failed");
				}

				BindTransactions(sourceTran.BankAccountTransactionK, destTran.BankAccountTransactionK);
			} catch (Exception ex) {
				TShockAPI.Log.ConsoleError(" seconomy mysql: database error in transfer:" + ex.ToString());
				args.Exception = ex;
				return args;
			}
			
			FromAccount.SyncBalance();
			ToAccount.SyncBalance();

			args.TransferSucceeded = true;
			if (BankTransferCompleted != null) {
				BankTransferCompleted(this, args);
			}

			if (SEconomyInstance.Configuration.EnableProfiler == true) {
				sw.Stop();
				TShockAPI.Log.ConsoleInfo("seconomy mysql: transfer took {0} ms", sw.ElapsedMilliseconds);
			}

			return args;
		}

		public async Task<BankTransferEventArgs> TransferBetweenAsync(IBankAccount FromAccount, IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			return await Task.Run(() => TransferBetween(FromAccount, ToAccount, Amount, Options, TransactionMessage, JournalMessage));
		}

		public IBankAccount GetWorldAccount()
		{
			IBankAccount worldAccount = null;

			//World account matches the current world, ignore.
			if ((SEconomyInstance.WorldAccount != null && SEconomyInstance.WorldAccount.WorldID == Terraria.Main.worldID)
				|| Terraria.Main.worldID == 0) {
				return null;
			}

			worldAccount = (from i in bankAccounts
							where (i.Flags & Journal.BankAccountFlags.SystemAccount) == Journal.BankAccountFlags.SystemAccount
							   && (i.Flags & Journal.BankAccountFlags.PluginAccount) == 0
							   && i.WorldID == Terraria.Main.worldID
							select i).FirstOrDefault();

			//world account does not exist for this world ID, create one
			if (worldAccount == null) {
				//This account is always enabled, locked to the world it's in and a system account (ie. can run into deficit) but not a plugin account
				IBankAccount newWorldAcc = AddBankAccount("SYSTEM", Terraria.Main.worldID, Journal.BankAccountFlags.Enabled | Journal.BankAccountFlags.LockedToWorld | Journal.BankAccountFlags.SystemAccount, "World account for world " + Terraria.Main.worldName);

				worldAccount = newWorldAcc;
			}

			if (worldAccount != null) {
				//Is this account listed as enabled?
				bool accountEnabled = (worldAccount.Flags & Journal.BankAccountFlags.Enabled) == Journal.BankAccountFlags.Enabled;

				if (!accountEnabled) {
					TShockAPI.Log.ConsoleError(string.Format(SEconomyPlugin.Locale.StringOrDefault(60, "The world account for world {0} is disabled.  Currency will not work for this game."), Terraria.Main.worldName));
					return null;
				}
			} else {
				TShockAPI.Log.ConsoleError(SEconomyPlugin.Locale.StringOrDefault(61, "There was an error loading the bank account for this world.  Currency will not work for this game."));
			}

			return worldAccount;
		}

		public void DumpSummary()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{

		}

		#endregion
	}
}
