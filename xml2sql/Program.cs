using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TShockAPI.DB;
using TShockAPI.Extensions;
using MySql.Data.MySqlClient;
using MySql.Data.Common;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;
using Wolfje.Plugins.SEconomy.Configuration;
using Wolfje.Plugins.SEconomy.Extensions;
using Wolfje.Plugins.SEconomy.Journal.XMLJournal;

namespace xml2sql {
	public class Program {
		public static event EventHandler<JournalLoadingPercentChangedEventArgs> JournalLoadingPercentChanged;

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(" xml2sql - an XML to SQL journal importer");
			Console.WriteLine(" For use with SEconomy Update 15 or newer");
			Console.WriteLine();

			Process();
		}

		static void Process()
		{
			SEconomy sec = new SEconomy(null);
			SEconomyPlugin.Instance = sec;
			XmlTransactionJournal journal = null;
			string connectionString = null;
			int oldPercent = 0, skipped = 0;
			Dictionary<long, long> oldNewTransactions = new Dictionary<long, long>();
			MySqlConnection sqlConnection = null;
			JournalLoadingPercentChangedEventArgs args = new JournalLoadingPercentChangedEventArgs() {
				Label = "Accounts"
			};

			sec.Configuration = Config.FromFile(Config.BaseDirectory + Path.DirectorySeparatorChar + "seconomy.config.json");
			journal = new XmlTransactionJournal(sec, Config.BaseDirectory + Path.DirectorySeparatorChar + "SEconomy.journal.xml.gz");
			connectionString = string.Format("server={0};user id={2};database={1};password={3}",
				sec.Configuration.SQLConnectionProperties.DbHost, sec.Configuration.SQLConnectionProperties.DbName, sec.Configuration.SQLConnectionProperties.DbUsername, sec.Configuration.SQLConnectionProperties.DbPassword);
			JournalLoadingPercentChanged += journal_JournalLoadingPercentChanged;
			journal.JournalLoadingPercentChanged += journal_JournalLoadingPercentChanged;
			journal.LoadJournal();

			Console.WriteLine();

			sqlConnection = new MySqlConnection(connectionString);

			Console.WriteLine("Your SEconomy database will be flushed.  All accounts, and transactions will be deleted before the import.");
			Console.Write("Continue? [y/n] ");

			if (Console.ReadKey().KeyChar != 'y') {
				return;
			}

			Console.WriteLine();

			sqlConnection.Query(string.Format("DELETE FROM `{0}`.`bank_account`;", sec.Configuration.SQLConnectionProperties.DbName));
			sqlConnection.Query(string.Format("ALTER TABLE `{0}`.`bank_account` AUTO_INCREMENT 0;", sec.Configuration.SQLConnectionProperties.DbName));
			sqlConnection.Query(string.Format("DELETE FROM `{0}`.`bank_account_transaction`;", sec.Configuration.SQLConnectionProperties.DbName));
			sqlConnection.Query(string.Format("ALTER TABLE `{0}`.`bank_account_transaction` AUTO_INCREMENT 0;", sec.Configuration.SQLConnectionProperties.DbName));

			Console.WriteLine("This will probably take a while...\r\n");
			Console.WriteLine();
			if (JournalLoadingPercentChanged != null) {
				JournalLoadingPercentChanged(null, args);
			}

			//using (Context ctx = new Context(connectionString)) {
			//	if (ctx.DatabaseExists() == false) {
			//		Console.WriteLine("Your SEconomy database does not exist.  Create it?");
			//		Console.Write("[y/n] ");
			//		if (Console.ReadKey().KeyChar != 'y') {
			//			return;
			//		}

			//		ctx.CreateDatabase();
			//	}

			for (int i = 0; i < journal.BankAccounts.Count; i++) {
				IBankAccount account = journal.BankAccounts.ElementAtOrDefault(i);
				double percentComplete = (double)i / (double)journal.BankAccounts.Count * 100;
				long id = -1;

				if (account == null) {
					continue;
				}

				string query = @"INSERT INTO `bank_account` 
									(user_account_name, world_id, flags, flags2, description, old_bank_account_k)
								  VALUES (@0, @1, @2, @3, @4, @5);";

				try {
					sqlConnection.QueryIdentity(query, out id, account.UserAccountName, account.WorldID,
						(int)account.Flags, 0, account.Description, account.BankAccountK);
				} catch (Exception ex) {
					TShockAPI.Log.ConsoleError(" seconomy mysql: sql error adding bank account: " + ex.ToString());
					continue;
				}

				for (int t = 0; t < account.Transactions.Count(); t++) {
					long tranId = -1;
					ITransaction transaction = account.Transactions.ElementAtOrDefault(t);
					string txQuery = @"INSERT INTO `bank_account_transaction` 
										(bank_account_fk, amount, message, flags, flags2, transaction_date_utc, old_bank_account_transaction_k)
										VALUES (@0, @1, @2, @3, @4, @5, @6);";

					try {
						sqlConnection.QueryIdentity(txQuery, out tranId, id, (long)transaction.Amount, transaction.Message,
							(int)BankAccountTransactionFlags.FundsAvailable, (int)transaction.Flags2, DateTime.UtcNow, 
							transaction.BankAccountTransactionK);
					} catch (Exception ex) {
						TShockAPI.Log.ConsoleError(" seconomy mysql: Database error in BeginSourceTransaction: " + ex.Message);
						continue;
					}
				}

				if (oldPercent != (int)percentComplete) {
					args.Percent = (int)percentComplete;
					if (JournalLoadingPercentChanged != null) {
						JournalLoadingPercentChanged(null, args);
					}
					oldPercent = (int)percentComplete;
				}
			}

			Console.WriteLine("import complete", skipped);
			Console.WriteLine("Press any key to exit");
			Console.Read();
		}

		static void journal_JournalLoadingPercentChanged(object sender, JournalLoadingPercentChangedEventArgs e)
		{
			ConsoleEx.WriteBar(e);
		}

		protected static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\ServerPlugins";
			string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
			if (File.Exists(assemblyPath) == false) return null;
			Assembly assembly = Assembly.LoadFrom(assemblyPath);
			return assembly;
		}
	}
}
