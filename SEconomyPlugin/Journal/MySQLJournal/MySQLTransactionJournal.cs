using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolfje.Plugins.SEconomy.Journal.XMLJournal;

namespace Wolfje.Plugins.SEconomy.Journal.MySQLJournal {
	public class MySQLTransactionJournal : ITransactionJournal {
		protected string connectionString;
		protected List<IBankAccount> bankAccounts;
		
		public MySQLTransactionJournal(SEconomy instance, Configuration.SQLConnectionProperties sqlProperties)
		{
			if (string.IsNullOrEmpty(sqlProperties.DbOverrideConnectionString) == false) {
				this.connectionString = sqlProperties.DbOverrideConnectionString;
			}

			this.connectionString = string.Format("metadata=res://*/Journal.MySQLJournal.Database.MySQLJournal.csdl|res://*/Journal.MySQLJournal.Database.MySQLJournal.ssdl|res://*/Journal.MySQLJournal.Database.MySQLJournal.msl;provider=MySql.Data.MySqlClient;provider connection string=\"server={0};user id={2};database={1};password={3}\"", sqlProperties.DbHost, sqlProperties.DbName, sqlProperties.DbUsername, sqlProperties.DbPassword);
			this.SEconomyInstance = instance;
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

		public Database.Context GetContext()
		{
			return new Database.Context(connectionString);
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
			using (Database.Context db = GetContext()) {
				try {
					Database.bank_account acct = new Database.bank_account() {
						user_account_name = Account.UserAccountName,
						description = Account.Description,
						flags = (int)Account.Flags,
						world_id = Account.WorldID
					};

					db.bank_account.AddObject(acct);
					db.SaveChanges();
					Account.BankAccountK = acct.bank_account_id;
					BankAccounts.Add(Account);
				} catch (Exception ex) {
					TShockAPI.Log.ConsoleError(" seconomy mysql: sql error adding bank account: " + ex.ToString());
					return null;
				}
			}
			
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
			using (Database.Context db = GetContext()) {
				Database.bank_account account = db.bank_account.FirstOrDefault(i => i.bank_account_id == BankAccountK);
				if (account == null) {
					return;
				}

				db.bank_account.DeleteObject(account);
				db.SaveChanges();
			}
			bankAccounts.RemoveAll(i => i.BankAccountK == BankAccountK);
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
			Database.Context db = GetContext();
			long bankAccountCount = 0;
			int oldPercent = 0;
			JournalLoadingPercentChangedEventArgs parsingArgs = new JournalLoadingPercentChangedEventArgs() {
				Label = "SQL"
			};

			try {
				if (JournalLoadingPercentChanged != null) {
					JournalLoadingPercentChanged(this, parsingArgs);
				}

				if (db.DatabaseExists() == false) {
					db.CreateDatabase();
					db.SaveChanges();
				}

				bankAccounts = new List<IBankAccount>();
				bankAccountCount = db.bank_account.Count();

				for (int i = 0; i <= bankAccountCount; i++) {
					Database.bank_account acc = null;
					MySQLBankAccount sqlAccount = null;
					double percentComplete = (double)i / (double)bankAccountCount * 100;

					if ((acc = db.bank_account.AsEnumerable().ElementAtOrDefault(i)) == null) {
						continue;
					}

					sqlAccount = new MySQLBankAccount(this) {
						BankAccountK = acc.bank_account_id,
						Description = acc.description,
						Flags = (BankAccountFlags)Enum.Parse(typeof(BankAccountFlags), acc.flags.ToString()),
						UserAccountName = acc.user_account_name,
						WorldID = acc.world_id
					};

					sqlAccount.SyncBalance();
					bankAccounts.Add(sqlAccount);

					if (oldPercent != (int)percentComplete) {
						parsingArgs.Percent = (int)percentComplete;
						if (JournalLoadingPercentChanged != null) {
							JournalLoadingPercentChanged(this, parsingArgs);
						}
						oldPercent = (int)percentComplete;
					}
				}

				parsingArgs.Percent = 100;
				if (JournalLoadingPercentChanged != null) {
					JournalLoadingPercentChanged(this, parsingArgs);
				}
			} catch(Exception ex) {
				TShockAPI.Log.ConsoleError(" seconomy mysql: db error in LoadJournal: " + ex.ToString());
			} finally {
				db.Dispose();
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

		Database.bank_account_transaction BeginSourceTransaction(Database.Context context, long BankAccountK, Money Amount, string Message)
		{
			IBankAccount bankAccount = GetBankAccount(BankAccountK);
			Database.bank_account_transaction txn = null;
			if (bankAccount == null) {
				return null;
			}

			txn = new Database.bank_account_transaction() {
				bank_account_fk = bankAccount.BankAccountK,
				amount = (-1) * Amount,
				flags = (int)Journal.BankAccountTransactionFlags.FundsAvailable,
				transaction_date_utc = DateTime.UtcNow
			};

			if (string.IsNullOrEmpty(Message) == false) {
				txn.message = Message;
			}

			context.bank_account_transaction.AddObject(txn);

			return txn;
		}

		Database.bank_account_transaction FinishEndTransaction(Database.Context context, IBankAccount ToAccount, Money Amount, string Message)
		{
			IBankAccount bankAccount = GetBankAccount(ToAccount.BankAccountK);
			Database.bank_account_transaction txn = null;
			if (bankAccount == null) {
				return null;
			}

			txn = new Database.bank_account_transaction() {
				bank_account_fk = bankAccount.BankAccountK,
				amount = Amount,
				flags = (int)Journal.BankAccountTransactionFlags.FundsAvailable,
				transaction_date_utc = DateTime.UtcNow
			};

			if (string.IsNullOrEmpty(Message) == false) {
				txn.message = Message;
			}

			context.bank_account_transaction.AddObject(txn);

			return txn;
		}

		public int BindTransactions(ref Database.bank_account_transaction Source, ref Database.bank_account_transaction Dest)
		{
			if (Source == null || Dest == null) {
				return -1;
			}

			Source.bank_account_transaction_inverse = Dest;
			Dest.bank_account_transaction_inverse = Source;

			return 0;
		}

		public BankTransferEventArgs TransferBetween(IBankAccount FromAccount, IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			PendingTransactionEventArgs pendingTransaction = new PendingTransactionEventArgs(FromAccount, ToAccount, Amount, Options, TransactionMessage, JournalMessage);
			Database.bank_account sourceAccount = null;
			Database.bank_account_transaction sourceTran, destTran;
			Database.Context db = GetContext();
			BankTransferEventArgs args = new BankTransferEventArgs() {
				TransferSucceeded = false
			};

			if (ToAccount == null && TransferMaySucceed(FromAccount, ToAccount, Amount, Options) == false) {
				return args;
			}

			if ((sourceAccount = db.bank_account.FirstOrDefault(i => i.bank_account_id == FromAccount.BankAccountK)) == null) {
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
				sourceTran = BeginSourceTransaction(db, FromAccount.BankAccountK, Amount, JournalMessage);
				
				if (sourceTran == null) {
					throw new Exception("BeginSourceTransaction failed");
				}

				destTran = FinishEndTransaction(db, ToAccount, Amount, JournalMessage);
				if (destTran == null) {
					throw new Exception("FinishEndTransaction failed");
				}

				db.SaveChanges();

				if (BindTransactions(ref sourceTran, ref destTran) < 0) {
					throw new Exception("BindTransactions failed");
				}

				db.SaveChanges();
			} catch (Exception ex) {
				TShockAPI.Log.ConsoleError(" seconomy mysql: database error in transfer:" + ex.ToString());
				args.Exception = ex;
				return args;
			} finally {
				db.Dispose();
			}
			
			FromAccount.SyncBalance();
			ToAccount.SyncBalance();

			args.TransferSucceeded = true;
			if (BankTransferCompleted != null) {
				BankTransferCompleted(this, args);
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
