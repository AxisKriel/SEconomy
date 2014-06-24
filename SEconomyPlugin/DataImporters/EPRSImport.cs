using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TShockAPI;
using TShockAPI.DB;

namespace Wolfje.Plugins.SEconomy.DataImporters {
    public class EPRSImport {

        /// <summary>
        /// Imports from an EPRS database into the SEconomy journal.
        /// </summary>
        public static void Import() {
            List<EPRSAccount> eprsAccounts = new List<EPRSAccount>();

            Log.ConsoleInfo("seconomy import:  Importing from EPRS database");
            try {
                using (QueryResult reader = TShockAPI.TShock.DB.QueryReader("SELECT * FROM serverpointaccounts")) {
                    while (reader.Read()) {
                        EPRSAccount account = new EPRSAccount();

                        account.Name = reader.Get<string>("name");
                        account.Amount = reader.Get<long>("amount");
                        //don't bother about accounts that don't have any balance.
                        if (account.Amount > 0) {
                            eprsAccounts.Add(account);
                        }
                    }
                }
            } catch {
                Log.ConsoleError("seconomy import:  There doesn't appear to be an EPRS instance on this version of tshock, or the database could not be accessed.  The import did not succeed.");
                return;
            }

            if (eprsAccounts.Count > 0) {
                Log.ConsoleInfo("seconomy import:  " + eprsAccounts.Count + " accounts.");
                int index = 0;

                foreach (EPRSAccount epAccount in eprsAccounts) {
                    //don't readd duplicate accounts, if it exists, just tag the imported account onto the actual one.
                    Journal.IBankAccount account = SEconomyPlugin.RunningJournal.GetBankAccountByName(epAccount.Name);

                    if (account == null) {
                        account = SEconomyPlugin.RunningJournal.AddBankAccount(epAccount.Name, Terraria.Main.worldID, Journal.BankAccountFlags.Enabled | Journal.BankAccountFlags.LockedToWorld, "EPRS Imported account using /bank eprs import");
                    }

                    //don't reimport the same amount of cash.
                    Journal.ITransaction trans = SEconomyPlugin.RunningJournal.Transactions.FirstOrDefault(i => i.Amount == epAccount.Amount && i.Message == "EPRS Import" && i.BankAccountFK == account.BankAccountK);
                    if (trans == null) {
                        //give them the cash from the EPRS database
                        SEconomyPlugin.WorldAccount.TransferTo(account, epAccount.Amount, Journal.BankAccountTransferOptions.AnnounceToReceiver | Journal.BankAccountTransferOptions.IsPayment, "EPRS Import", "EPRS Import");
                    }

                    string line = string.Format("\r[{2:p} {0}/{1}] import {3}", index, eprsAccounts.Count, Convert.ToDecimal(index) / eprsAccounts.Count, epAccount.Name);
                    SEconomyPlugin.FillWithSpaces(ref line);
                    Console.Write(line);

                    index++;
                }

                Log.ConsoleInfo("seconomy import: complete.");
            } else {
                Log.ConsoleError("seconomy import:  There doesn't appear to be any accounts to import.  There may not be an EPRS instance, or there was an error in the database.");
            }
        }

        /// <summary>
        /// Imports from an EPRS database into the SEconomy journal.
        /// </summary>
        public static Task ImportAsync() {
            return Task.Factory.StartNew(() => {
                Import();
            });
        }

    }

    class EPRSAccount {
        public string Name { get; set; }
        public long Amount { get; set; }
    }
}
