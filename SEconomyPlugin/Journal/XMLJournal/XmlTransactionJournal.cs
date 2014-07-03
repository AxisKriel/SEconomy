using System;

using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Globalization;
using System.Threading;
using System.Collections;
using System.Text;

namespace Wolfje.Plugins.SEconomy.Journal {

	/// <summary>
	/// Holds an XML representation of the SEconomy transaction journal.
	/// </summary>
	public class XmlTransactionJournal : ITransactionJournal {
		public SEconomy SEconomyInstance { get; set; }

		readonly List<IBankAccount> bankAccounts = new List<IBankAccount>();
		internal static long _bankAccountSeed;
		internal static long _transactionSeed;

		internal string path;
		internal System.Timers.Timer JournalBackupTimer { get; set; }

		public event EventHandler<PendingTransactionEventArgs> BankTransactionPending;
		public event EventHandler<BankTransferEventArgs> BankTransferCompleted;

		/// <summary>
		/// Returns the version of the XML schema built into this dll
		/// </summary>
		public readonly Version XmlSchemaVersion = new Version(1, 3, 0);

		public bool JournalSaving { get; set; }
		public bool BackupsEnabled { get; set; }

		public XmlTransactionJournal(SEconomy Parent, string JournalSavePath)
		{
			this.SEconomyInstance = Parent;
			this.path = JournalSavePath;

			if (Parent.Configuration.JournalBackupMinutes > 0) {
				JournalBackupTimer = new System.Timers.Timer(Parent.Configuration.JournalBackupMinutes * 60000);
				JournalBackupTimer.Elapsed += JournalBackupTimer_Elapsed;
				this.BackupsEnabled = true;
			}

			LoadJournal();
		}

		/// <summary>
		/// Occurs when the journal backup timer fires, and performs a journal save.
		/// </summary>
		protected async void JournalBackupTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (this.BackupsEnabled == false || this.JournalSaving == true) {
				return;
			}

			await SaveJournalAsync();
		}

		/// <summary>
		/// Returns a world account for the current running world.  If it does not exist, one gets created and then returned.
		/// </summary>
		public IBankAccount GetWorldAccount()
		{
			IBankAccount worldAccount = null;

			//World account matches the current world, ignore.
			if (SEconomyInstance.WorldAccount != null && SEconomyInstance.WorldAccount.WorldID == Terraria.Main.worldID) {
				return null;
			}

			if (Terraria.Main.worldID > 0) {
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
						TShockAPI.Log.ConsoleError("The world account for world " + Terraria.Main.worldName + " is disabled.  Currency will not work for this game.");
						return null;
					}
				} else {
					TShockAPI.Log.ConsoleError("There was an error loading the bank account for this world.  Currency will not work for this game.");
				}
			}

			return worldAccount;
		}


		/// <summary>
		/// Delfates a GZip byte array and returns the uncompressed data.
		/// </summary>
		byte[] GZipDecompress(byte[] CompressedData)
		{
			byte[] deflatedData;

			using (MemoryStream outStream = new MemoryStream()) {
				using (GZipStream gzStream = new GZipStream(new MemoryStream(CompressedData), CompressionMode.Decompress, false)) {
					gzStream.CopyTo(outStream);
				}

				//Copy the deflated stream in its entirety onto the stack
				deflatedData = outStream.ToArray();
			}

			return deflatedData;
		}


		public static readonly object __writeLock = new object();
		private static readonly Random _rng = new Random();
		private const string _chars = "1234567890abcdefghijklmnopqrstuvwxyz";
		/// <summary>
		/// Thread-safely generates a random sequence of characters
		/// </summary>
		public static string RandomString(int Size)
		{
			char[] buffer = new char[Size];

			for (int i = 0; i < Size; i++) {
				int charSeed;
				lock (_rng) {
					charSeed = _rng.Next(_chars.Length);
				}
				buffer[i] = _chars[charSeed];
			}

			return new string(buffer);
		}

		XDocument NewJournal()
		{
			string journalComment =
@"

This is the SEconomy transaction journal file. 

You have probably guessed by now this is an XML format, this file persists all the transactions and bankaccounts 
in your server instance.  This file is not written to actively, all transaction processing is done in memory and 
coped out to disk every time the backup runs.

Editing this file here isn't going to make your changes persist, once edited you will need to execute /bank loadjournal 
in the server console to resync the in-memory journal with this one.  Be aware that you will lose any in-memory changes 
from now until when the file was writte, this usually results in a minor rollback of people's money.

Obviously it would be retarded to use that command on a journal that is months old.....
";

			string journalAccountComment =
				@"
BankAccounts Collection

This element holds all the bank accounts for a running server. Each BankAccount has a unique account number (starting from 1) and more attributes:

* UserAccountName - The login name of the TShock account this bank account is linked to
* WorldID - The WorldID that the account was created from, this is used when LockedToWorld is set and you want to lock bank accounts to worlds, otherwise they
            are static and are loaded in whichever world you create on the server.
* Flags - A bit-bashed set of flags for the account that control the state of it.  Look in the source for BankAccountFlags for a definition of what the bits do.

Please note, BankAccount elements do not keep a running total of their balance, that is done through summing all Transaction amounts 
(by XPath /Journal/Transactions/Transaction[@BankAccountFK=BankAccountK]/@Amount) linked to this account.
";

			string journalTransComment =
				@"
Transaction Collection

This element holds all the transactions for the current running server.  Each transaction is double-entry accounted, 
which means that a transaction is essentially done twice, representing the loss of money on one account, and the gain 
of money in the destination account or vice-versa.

A double-entry account journal must have two transactions; a source and a destination, and the amounts in each must be 
the inverse of eachother: If money is to be transferred away from a source account the source amount must be negative 
and the destination amount must be positive; and conversely if money is to be transferred into a source account the 
source amount must be postitive and the destination amount must be negative.

A Transaction has these following attributes:

* BankAccountTransactionK - A unique number identifying this transaction
* BankAccountFK - The unique identifier of the BankAccount element this transaction comes from
* Amount - The amount of money this transaction was for; positive for a gain in money, negative for a loss
* Flags - A bit-set flag of transaction options (See source for BankAccountTransferOptions for what they do)
* Flags2 - Unused
* BankAccountTransactionFK - A unique identifier of the opposite side of this double-entry transaction, therefore binding them together.
";

			return new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
								 new XComment(journalComment),
											new XElement("Journal",
												new XAttribute("Schema", new Version(1, 3, 0).ToString()),
												new XElement("BankAccounts", new XComment(journalAccountComment))
											));

		}

		public List<IBankAccount> BankAccounts
		{
			get
			{
				return bankAccounts;
			}
		}

		public IEnumerable<ITransaction> Transactions
		{
			get
			{
				return bankAccounts.SelectMany(i => i.Transactions);
			}
		}


		public IBankAccount AddBankAccount(string UserAccountName, long WorldID, BankAccountFlags Flags, string Description)
		{
			return AddBankAccount(new XmlBankAccount(this) {
				UserAccountName = UserAccountName,
				WorldID = WorldID,
				Flags = Flags,
				Description = Description,
			});
		}

		public IBankAccount AddBankAccount(IBankAccount Account)
		{
			Account.BankAccountK = Interlocked.Increment(ref _bankAccountSeed);

			this.bankAccounts.Add(Account);

			return Account;
		}

		public IBankAccount GetBankAccountByName(string UserAccountName)
		{
			return bankAccounts.FirstOrDefault(i => i.UserAccountName == UserAccountName);
		}

		public IBankAccount GetBankAccount(long BankAccountK)
		{
			for (int i = 0; i < bankAccounts.Count; ++i) {
				IBankAccount acct = bankAccounts[i];

				if (acct.BankAccountK == BankAccountK) {
					return acct;
				}
			}

			return null;
		}

		public IEnumerable<IBankAccount> GetBankAccountList(long BankAccountK)
		{
			return BankAccounts.Where(i => i.BankAccountK == BankAccountK);
		}

		public void DumpSummary()
		{
			StringBuilder sb = new StringBuilder();

			var qAccounts = from i in bankAccounts
							group i by i.BankAccountK into g
							select new {
								name = g.Key,
								count = g.Count()
							};


			foreach (var summary in qAccounts.OrderByDescending(i => i.count)) {
				sb.AppendLine(string.Format("{0},{1}", summary.name, summary.count));
			}

			System.IO.File.WriteAllText(Config.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "test.csv", sb.ToString());
		}

		public void DeleteBankAccount(long BankAccountK)
		{
			bankAccounts.RemoveAll(i => i.BankAccountK == BankAccountK);
		}

		public void SaveJournal()
		{
			try {
				XDocument journalXml = NewJournal();
				XElement bankAccountRoot = journalXml.Element("Journal").Element("BankAccounts");
				JournalSaving = true;

				foreach (IBankAccount account in BankAccounts) {
					XElement bankAccountNode = new XElement("BankAccount");

					bankAccountNode.Add(new XElement("Transactions"));

					bankAccountNode.SetAttributeValue("UserAccountName", account.UserAccountName);
					bankAccountNode.SetAttributeValue("WorldID", account.WorldID);
					bankAccountNode.SetAttributeValue("Flags", account.Flags);
					bankAccountNode.SetAttributeValue("Description", account.Description);
					bankAccountNode.SetAttributeValue("BankAccountK", account.BankAccountK);

					lock (account.__transactionLock) {
						foreach (ITransaction transaction in account.Transactions) {
							XElement transactionNode = new XElement("Transaction");

							transactionNode.SetAttributeValue("BankAccountTransactionK", transaction.BankAccountTransactionK);
							transactionNode.SetAttributeValue("BankAccountTransactionFK", transaction.BankAccountTransactionFK);
							transactionNode.SetAttributeValue("Flags", transaction.Flags);
							transactionNode.SetAttributeValue("Flags2", transaction.Flags2);
							transactionNode.SetAttributeValue("Message", transaction.Message);
							transactionNode.SetAttributeValue("TransactionDateUtc", transaction.TransactionDateUtc.ToString("s", CultureInfo.InvariantCulture));
							transactionNode.SetAttributeValue("Amount", transaction.Amount.Value);

							bankAccountNode.Element("Transactions").Add(transactionNode);
						}
					}

					bankAccountRoot.Add(bankAccountNode);
				}

				lock (__writeLock) {
					try {
						File.Delete(path + ".bak");
					} catch {
						//ignored
					}

					//Make a shadow copy of the written file.  This ensures that if a journal does corrupt then they
					//atleast have a full working copy backed up.w
					try {
						if (File.Exists(path) == true) {
							File.Move(path, path + ".bak");
						}
					} catch {
						TShockAPI.Log.ConsoleError("seconomy backup: Cannot copy {0} to {1}, shadow backups will not work!", path, path + ".bak");
					}

					Console.WriteLine("seconomy journal: writing to disk");
					try {
						using (FileStream fs = new FileStream(path + ".tmp", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
							fs.SetLength(0);

							using (GZipStream gzCompressor = new GZipStream(fs, CompressionMode.Compress)) {
								using (XmlTextWriter xmlWriter = new XmlTextWriter(gzCompressor, System.Text.Encoding.UTF8)) {
									xmlWriter.Formatting = Formatting.None;
									journalXml.WriteTo(xmlWriter);
								}
							}
						}
					} catch {
						TShockAPI.Log.ConsoleError("seconomy journal: Saving your journal failed!");

						if (File.Exists(path + ".tmp") == true) {
							try {
								File.Delete(path + ".tmp");
							} catch {
								TShockAPI.Log.ConsoleError("seconomy journal: Cannot delete temporary file!");
								throw;
							}
						}
					}

					if (File.Exists(path + ".tmp") == true) {
						try {
							File.Move(path + ".tmp", path);
						} catch {
							TShockAPI.Log.ConsoleError("seconomy journal: Cannot delete temporary file!");
							throw;
						}
					}

					Console.WriteLine("seconomy journal: finished backing up.");
				}
			} catch {
				Console.WriteLine("seconomy journal: There was an error saving your journal.  Make sure you have backups.");
			} finally {
				JournalSaving = false;
			}
		}

		public Task SaveJournalAsync()
		{
			return Task.Factory.StartNew(() => SaveJournal());
		}

		public void LoadJournal()
		{

			ConsoleColor origColour = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;

			Console.WriteLine("SEconomy is loading its journal.");
			Console.WriteLine();

			try {
				byte[] fileData = new byte[0];
			initPoint:
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write("loading journal");
				try {
					fileData = File.ReadAllBytes(path);
				} catch (Exception ex) {
					Console.ForegroundColor = ConsoleColor.DarkCyan;

					if (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException) {
						ConsoleEx.WriteAtEnd(2, ConsoleColor.Red, "[not found, creating new]\r\n");

						SaveJournal();
						//yes there are valid uses for goto, don't judge me fool
						goto initPoint;
					} else if (ex is System.Security.SecurityException) {
						ConsoleEx.WriteAtEnd(2, ConsoleColor.Red, "[denied]\r\n");
					} else {
						ConsoleEx.WriteAtEnd(2, ConsoleColor.Red, "[failed]\r\n");
					}
				}

				ConsoleEx.WriteAtEnd(2, ConsoleColor.Green, "[OK: {0} kB]\r\n", fileData.LongLength / 1024);
				Console.ForegroundColor = ConsoleColor.Yellow;

				Console.Write("decompressing journal");
				byte[] uncompressedData;
				try {
					uncompressedData = GZipDecompress(fileData);
				} catch {
					ConsoleEx.WriteAtEnd(2, ConsoleColor.Red, "[failed]\r\n");
					return;
				}

				Console.ForegroundColor = ConsoleColor.Green;

				ConsoleEx.WriteAtEnd(2, ConsoleColor.Green, "[OK: gz {0} kB]\r\n", uncompressedData.LongLength / 1024);
				Console.ForegroundColor = ConsoleColor.Yellow;

				Console.Write("parsing accounts");

				try {
					Hashtable bankAccountMap = new Hashtable();
					Hashtable transactionMap = new Hashtable();

					//You can't use XDocument.Parse when you have an XDeclaration for some even dumber reason than the write
					//issue, XDocument has to be constructed from a .net 3.5 XmlReader in this case
					//or you get a parse exception complaining about literal content.
					using (MemoryStream ms = new MemoryStream(uncompressedData)) {
						using (XmlTextReader xmlStream = new XmlTextReader(ms)) {
							XDocument doc = XDocument.Load(xmlStream);
							var bankAccountList = doc.XPathSelectElements("/Journal/BankAccounts/BankAccount");
							int bankAccountCount = bankAccountList.Count();
							int i = 0;
							int oldPercent = 0;

							foreach (XElement elem in bankAccountList) {
								long bankAccountK = 0L;
								double percentComplete = (double)i / (double)bankAccountCount * 100;

								var account = new XmlBankAccount(this) {
									Description = elem.Attribute("Description").Value,
									UserAccountName = elem.Attribute("UserAccountName").Value,
									WorldID = long.Parse(elem.Attribute("WorldID").Value),
									Flags = (BankAccountFlags)Enum.Parse(typeof(BankAccountFlags), elem.Attribute("Flags").Value)
								};

								if (!long.TryParse(elem.Attribute("BankAccountK").Value, out bankAccountK)) {
									//we've overwritten the old bank account key, add it to the old/new map
									bankAccountK = Interlocked.Increment(ref _bankAccountSeed);
									bankAccountMap.Add(elem.Attribute("BankAccountK").Value, bankAccountK);
								}

								account.BankAccountK = bankAccountK;

								//parse transactions under this node
								if (elem.Element("Transactions") != null) {
									foreach (XElement txElement in elem.Element("Transactions").Elements("Transaction")) {
										long transactionK = 0L;
										long transactionFK = 0;
										long amount = long.Parse(txElement.Attribute("Amount").Value);
										DateTime transactionDate;
										BankAccountTransactionFlags flags;

										long.TryParse(txElement.Attribute("BankAccountTransactionK").Value, out transactionK);
										long.TryParse(txElement.Attribute("BankAccountTransactionFK").Value, out transactionFK);
										DateTime.TryParse(txElement.Attribute("TransactionDateUtc").Value, out transactionDate);
										Enum.TryParse<BankAccountTransactionFlags>(txElement.Attribute("Flags").Value, out flags);

										//ignore orphaned transactions
										var trans = new XmlTransaction(account) {
											Amount = amount,
											BankAccountTransactionK = transactionK,
											BankAccountTransactionFK = transactionFK,
											TransactionDateUtc = transactionDate,
											Flags = flags
										};

										trans.Message = txElement.Attribute("Message") != null ? txElement.Attribute("Message").Value : null;

										account.AddTransaction(trans);
									}
								}

								if (oldPercent != (int)percentComplete) {
									ConsoleEx.WriteAtEnd(4, ConsoleColor.Yellow, "[{0}%]", (int)percentComplete);
									oldPercent = (int)percentComplete;
								}

								Interlocked.Increment(ref i);

								bankAccounts.Add(account);
							}

							ConsoleEx.WriteAtEnd(4, ConsoleColor.Green, "[OK]");

							Interlocked.Exchange(ref _bankAccountSeed, bankAccounts.Count() > 0 ? bankAccounts.Max(sum => sum.BankAccountK) : 0);

							//delete transactions with duplicate IDs

							var qAccounts = from summary in bankAccounts
											group summary by summary.BankAccountK into g
											where g.Count() > 1
											select new {
												name = g.Key,
												count = g.Count()
											};

							long[] duplicateAccounts = qAccounts.Select(pred => pred.name).ToArray();

							int removedAccounts = bankAccounts.RemoveAll(pred => duplicateAccounts.Contains(pred.BankAccountK));

							if (removedAccounts > 0) {
								TShockAPI.Log.Warn("seconomy journal: removed " + removedAccounts + " accounts with duplicate IDs.");
							}

							//transactions in the old schema.
							int tranCount = doc.XPathSelectElements("/Journal/Transactions/Transaction").Count();
							i = 0; //reset index

							foreach (XElement elem in doc.XPathSelectElements("/Journal/Transactions/Transaction")) {
								//Parallel.ForEach(doc.XPathSelectElements("/Journal/Transactions/Transaction"), (elem) => {
								double percentComplete = (double)i / (double)tranCount * 100;
								long bankAccountFK = 0L;
								long transactionK = 0L;
								long amount = long.Parse(elem.Attribute("Amount").Value);

								if (!long.TryParse(elem.Attribute("BankAccountFK").Value, out bankAccountFK)) {
									if (bankAccountMap.ContainsKey(elem.Attribute("BankAccountFK").Value)) {
										Interlocked.Exchange(ref bankAccountFK, (long)bankAccountMap[elem.Attribute("BankAccountFK").Value]);
									}
								}

								IBankAccount bankAccount = GetBankAccount(bankAccountFK);


								long.TryParse(elem.Attribute("BankAccountTransactionK").Value, out transactionK);

								//ignore orphaned transactions
								if (bankAccount != null) {
									var trans = new XmlTransaction(bankAccount) {
										Amount = amount,
										BankAccountTransactionK = transactionK
									};

									if (elem.Attribute("BankAccountTransactionFK") != null) {
										trans.CustomValues.Add(XmlTransaction.kXmlTransactionOldTransactonFK, elem.Attribute("BankAccountTransactionFK").Value);
									}

									trans.Message = elem.Attribute("Message") != null ? elem.Attribute("Message").Value : null;

									bankAccount.AddTransaction(trans);

									transactionMap.Add(elem.Attribute("BankAccountTransactionK").Value, trans.BankAccountTransactionK);
								}


								//try {
								//    trans.TransactionDateUtc = DateTime.Parse(elem.Attribute("TransactionDateUtc").Value);
								//} catch {
								//    trans.TransactionDateUtc = DateTime.UtcNow;
								//}
								//trans.Flags = (BankAccountTransactionFlags)Enum.Parse(typeof(BankAccountTransactionFlags), elem.Attribute("Flags").Value);

								//if (elem.Attribute("Flags2") != null) {
								//    trans.Flags2 = (BankAccountTransactionFlags)Enum.Parse(typeof(BankAccountTransactionFlags), elem.Attribute("Flags2").Value);

								//}


								if (oldPercent != (int)percentComplete) {
									ConsoleEx.WriteAtEnd(4, ConsoleColor.Yellow, "[{0}%]", (int)percentComplete);
									oldPercent = (int)percentComplete;
								}

								Interlocked.Increment(ref i);
							}//);

							Console.ForegroundColor = ConsoleColor.Yellow;

							Console.Write("\r\nupgrading transactions");
							int txCount = Transactions.Count();
							int x = 0;


							foreach (IBankAccount account in bankAccounts) {
								foreach (ITransaction trans in account.Transactions) {
									double pcc = (double)x / (double)txCount * 100;

									//assigns the transactionK according to the hashmap of the old key stored as custom values
									if (trans.CustomValues.ContainsKey(XmlTransaction.kXmlTransactionOldTransactonFK)) {
										object value = transactionMap[trans.CustomValues[XmlTransaction.kXmlTransactionOldTransactonFK]];

										trans.BankAccountTransactionFK = value != null ? (long)value : -1L;
									}
									if (oldPercent != (int)pcc) {
										ConsoleEx.WriteAtEnd(4, ConsoleColor.Yellow, "[{0}%]", (int)pcc);
										oldPercent = (int)pcc;
									}
									Interlocked.Increment(ref x);
									trans.CustomValues.Clear();
								}
							}

							bankAccountMap = null;
							transactionMap = null;

							var accountCount = bankAccounts.Count;

							ConsoleEx.WriteAtEnd(2, ConsoleColor.Green, "[OK: xml {0} acc, {1} tx]\r\n", accountCount, Transactions.Count());
							Console.ForegroundColor = ConsoleColor.Yellow;
						}
					}

				} catch(Exception ex) {
					ConsoleEx.WriteAtEnd(2, ConsoleColor.Red, "[corrupt]\r\n");
					TShockAPI.Log.ConsoleError(ex.ToString());
					Console.WriteLine("Your transaction journal appears to be corrupt and transactions have been lost.\n\nYou will start with a clean journal.\nYour old journal file has been move to SEconomy.journal.xml.gz.corrupt");
					File.Move(path, path + "." + DateTime.Now.ToFileTime().ToString() + ".corrupt");

					SaveJournal();

					//yes there are valid uses for goto, don't judge me fool
					goto initPoint;
				}
			} finally {
				Console.ForegroundColor = origColour;

				Console.WriteLine();
			}

			System.GC.Collect();
		}

		public Task LoadJournalAsync()
		{
			return Task.Factory.StartNew(() => LoadJournal());
		}

		public void BackupJournal()
		{
			SaveJournal();
		}

		public async Task BackupJournalAsync()
		{
			await Task.Factory.StartNew(() => BackupJournal());
		}

		public async Task SquashJournalAsync()
		{
			int bankAccountCount = BankAccounts.Count();
			bool responsibleForTurningBackupsBackOn = false;

			Console.WriteLine("seconomy xml: beginning Squash");

			if (SEconomyInstance.RunningJournal.BackupsEnabled == true) {
				SEconomyInstance.RunningJournal.BackupsEnabled = false;
				responsibleForTurningBackupsBackOn = true;
			}

			for (int i = 0; i < bankAccountCount; i++) {
				IBankAccount account = BankAccounts.ElementAtOrDefault(i);
				if (account == null) {
					continue;
				}


				// Add the squished summary
				ITransaction squash = new XmlTransaction(account) {
					Amount = account.Transactions.Sum(x => x.Amount),
					Flags = BankAccountTransactionFlags.FundsAvailable | BankAccountTransactionFlags.Squashed,
					TransactionDateUtc = DateTime.UtcNow,
					Message = "Transaction squash"
				};

				account.Transactions.Clear();
				account.AddTransaction(squash);
			}

			//abandon the old journal and assign the squashed one
			Console.WriteLine("re-syncing online accounts.");

			foreach (Journal.IBankAccount account in BankAccounts) {
				IBankAccount runtimeAccount = GetBankAccount(account.BankAccountK);

				if (runtimeAccount != null && runtimeAccount.Owner != null) {
					Console.WriteLine("re-syncing {0}", runtimeAccount.Owner.TSPlayer.Name);
					await runtimeAccount.SyncBalanceAsync();
				}
			}

			await SaveJournalAsync();
			if (responsibleForTurningBackupsBackOn) {
				/* 
				 * the backups could already have been disabled by something else.  
				 * We don't want to be the ones turning it back on
				 */
				SEconomyInstance.RunningJournal.BackupsEnabled = true;
			}
		}

		#region "transfers"

		bool TransferMaySucceed(IBankAccount FromAccount, IBankAccount ToAccount, Money MoneyNeeded, Journal.BankAccountTransferOptions Options)
		{
			if (FromAccount == null || ToAccount == null) {
				return false;
			}

			return ((FromAccount.IsSystemAccount || FromAccount.IsPluginAccount || ((Options & Journal.BankAccountTransferOptions.AllowDeficitOnNormalAccount) == Journal.BankAccountTransferOptions.AllowDeficitOnNormalAccount)) || (FromAccount.Balance >= MoneyNeeded && MoneyNeeded > 0));
		}

		ITransaction BeginSourceTransaction(long BankAccountK, Money Amount, string Message)
		{
			IBankAccount bankAccount = GetBankAccount(BankAccountK);
			ITransaction sourceTran = new XmlTransaction(bankAccount);

			sourceTran.Flags = Journal.BankAccountTransactionFlags.FundsAvailable;
			sourceTran.TransactionDateUtc = DateTime.UtcNow;
			sourceTran.Amount = (Amount * (-1));

			if (!string.IsNullOrEmpty(Message)) {
				sourceTran.Message = Message;
			}

			return bankAccount.AddTransaction(sourceTran);
		}

		ITransaction FinishEndTransaction(long SourceBankTransactionKey, IBankAccount ToAccount, Money Amount, string Message)
		{
			ITransaction destTran = new XmlTransaction(ToAccount);

			destTran.BankAccountFK = ToAccount.BankAccountK;
			destTran.Flags = Journal.BankAccountTransactionFlags.FundsAvailable;
			destTran.TransactionDateUtc = DateTime.UtcNow;
			destTran.Amount = Amount;
			destTran.BankAccountTransactionFK = SourceBankTransactionKey;

			if (!string.IsNullOrEmpty(Message)) {
				destTran.Message = Message;
			}

			return ToAccount.AddTransaction(destTran);
		}

		void BindTransactions(ref ITransaction SourceTransaction, ref ITransaction DestTransaction)
		{
			SourceTransaction.BankAccountTransactionFK = DestTransaction.BankAccountTransactionK;
			DestTransaction.BankAccountTransactionFK = SourceTransaction.BankAccountTransactionK;
		}

		public Task<BankTransferEventArgs> TransferBetweenAsync(IBankAccount FromAccount, IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			return Task.Factory.StartNew(() => TransferBetween(FromAccount, ToAccount, Amount, Options, TransactionMessage, JournalMessage));
		}

		public BankTransferEventArgs TransferBetween(IBankAccount FromAccount, IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			BankTransferEventArgs args = new BankTransferEventArgs();
			Guid profile = Guid.Empty;

			try {
				if (ToAccount != null && TransferMaySucceed(FromAccount, ToAccount, Amount, Options)) {
					PendingTransactionEventArgs pendingTransaction = new PendingTransactionEventArgs(FromAccount, ToAccount, Amount, Options, TransactionMessage, JournalMessage);

					if (BankTransactionPending != null) {
						BankTransactionPending(this, pendingTransaction);
					}

					args.Amount = pendingTransaction.Amount;
					args.SenderAccount = pendingTransaction.FromAccount;
					args.ReceiverAccount = pendingTransaction.ToAccount;
					args.TransferOptions = Options;
					args.TransferSucceeded = false;
					args.TransactionMessage = pendingTransaction.TransactionMessage;

					if (pendingTransaction.IsCancelled) {
						return args;
					}

					//insert the source negative transaction
					ITransaction sourceTran = BeginSourceTransaction(FromAccount.BankAccountK, Amount, JournalMessage);
					if (sourceTran != null) {
						//insert the destination inverse transaction
						ITransaction destTran = FinishEndTransaction(sourceTran.BankAccountTransactionK, ToAccount, Amount, JournalMessage);

						if (destTran != null) {
							//perform the double-entry binding
							BindTransactions(ref sourceTran, ref destTran);

							args.TransactionID = sourceTran.BankAccountTransactionK;

							//update balances
							FromAccount.Balance += (Amount * (-1));
							ToAccount.Balance += Amount;

							//transaction complete
							args.TransferSucceeded = true;
						}
					}
				} else {
					args.TransferSucceeded = false;

					/*
					 * concept: ??????
					 * if the amount coming from "this" account is a negative then the "sender account" needs to know the transfer failed.
					 * if the amount coming from "this" acount is a positive then the "reciever account" needs to know the transfer failed.
					 */
					if (ToAccount.IsSystemAccount == false && ToAccount.IsPluginAccount == false && FromAccount.Owner != null) {
						if (Amount < 0) {
							FromAccount.Owner.TSPlayer.SendErrorMessage("Invalid amount.");
						} else {
							FromAccount.Owner.TSPlayer.SendErrorMessage("You need {0} more money to make this payment.", ((Money)(FromAccount.Balance - Amount)).ToLongString());
						}
					}
				}
			} catch (Exception ex) {
				args.Exception = ex;
				args.TransferSucceeded = false;
			}

			if (BankTransferCompleted != null) {
				BankTransferCompleted(this, args);
			}

			return args;
		}

		#endregion

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				if (this.JournalBackupTimer != null) {
					this.JournalBackupTimer.Stop();
					this.JournalBackupTimer.Elapsed -= JournalBackupTimer_Elapsed;
					this.JournalBackupTimer.Dispose();
				}
			}
		}
	}
}

