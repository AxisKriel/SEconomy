using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wolfje.Plugins.SEconomy.Journal {

    public sealed partial class TransactionJournal {

		/// <summary>
		/// List of uncommitted funds
		/// </summary>
        static ConcurrentQueue<CachedTransaction> CachedTransactions { get; set; }
        static readonly System.Timers.Timer UncommittedFundTimer = new System.Timers.Timer(5000);
        
        public static void InitializeTransactionCache() {
            CachedTransactions = new ConcurrentQueue<CachedTransaction>();
            UncommittedFundTimer.Elapsed += UncommittedFundTimer_Elapsed;
            UncommittedFundTimer.Start();
        }

		/// <summary>
		/// Occurs when the cached payments timer needs to commit all the uncommitted transactions
		/// </summary>
        static void UncommittedFundTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            UncommittedFundTimer.Stop();
            ProcessQueue();
            UncommittedFundTimer.Start();
        }

		/// <summary>
		/// Adds a fund to the uncommitted cache
		/// </summary>
        public static void AddCachedTransaction(CachedTransaction Fund) {
            CachedTransactions.Enqueue(Fund);
        }

		/// <summary>
		/// Processes all elements in the queue and transfers them
		/// </summary>
        static void ProcessQueue() {
            List<CachedTransaction> aggregatedFunds = new List<CachedTransaction>();
            CachedTransaction fund;

            while (CachedTransactions.TryDequeue(out fund)) {
                //The idea of this is that the concurrent queue will aggregate everything with the same message.
                //So instead of spamming eye of ctaltlatlatututlutultu (shut up) it'll just agg them into one
                //and print something like "You gained 60 silver from 20 eyes" instead of spamming both the chat log
                //and the journal with bullshit
                CachedTransaction existingFund = aggregatedFunds.FirstOrDefault(i => i.Message == fund.Message && i.SourceBankAccountK == fund.SourceBankAccountK && i.DestinationBankAccountK == fund.DestinationBankAccountK);
                if (existingFund != null) {
                    existingFund.Amount += fund.Amount;

                    //indicate that this is an aggregate of a previous uncommitted fund
                    existingFund.Aggregations++;
                } else {
                    aggregatedFunds.Add(fund);
                }
            }

            foreach (CachedTransaction aggregatedFund in aggregatedFunds) {
                Journal.IBankAccount sourceAccount = SEconomyPlugin.RunningJournal.GetBankAccount(aggregatedFund.SourceBankAccountK);
                Journal.IBankAccount destAccount = SEconomyPlugin.RunningJournal.GetBankAccount(aggregatedFund.DestinationBankAccountK);

                if (sourceAccount != null && destAccount != null) {
                    StringBuilder messageBuilder = new StringBuilder(aggregatedFund.Message);

                    if (aggregatedFund.Aggregations > 1) {
                        messageBuilder.Insert(0, aggregatedFund.Aggregations + " ");
                        messageBuilder.Append("s");
                    }
                    //transfer the money
                    BankTransferEventArgs transfer = sourceAccount.TransferTo(destAccount, aggregatedFund.Amount, aggregatedFund.Options, messageBuilder.ToString(), messageBuilder.ToString());
                    if (!transfer.TransferSucceeded) {
                        if (transfer.Exception != null) {
                            TShockAPI.Log.ConsoleError(string.Format("seconomy cache: error source={0} dest={1}: {2}", aggregatedFund.SourceBankAccountK, aggregatedFund.DestinationBankAccountK, transfer.Exception));
                        }
                    }
                } else {
                    TShockAPI.Log.ConsoleError(string.Format("seconomy cache: transaction cache has no source or destination. source key={0} dest key={1}", aggregatedFund.SourceBankAccountK, aggregatedFund.DestinationBankAccountK));
                }
            }
        }
    }

	/// <summary>
	/// Holds information about a transaction that is to be cached, and committed some time in the future.
	/// </summary>
	public class CachedTransaction {
        public long SourceBankAccountK { get; set; }
        public long DestinationBankAccountK { get; set; }
        public Money Amount { get; set; }
        public string Message { get; set; }
        public BankAccountTransferOptions Options { get; set; }
        public int Aggregations { get; set; }

        public CachedTransaction() {
			this.Aggregations = 1;
        }

        /// <summary>
        /// Creates a simple cached transaction to the world account
        /// </summary>
        public static CachedTransaction NewTransactionToWorldAccount() {
            return new CachedTransaction() { DestinationBankAccountK = SEconomyPlugin.WorldAccount.BankAccountK };
        }

        /// <summary>
        /// Creates a simple cached transaction from the world account
        /// </summary>
        public static CachedTransaction NewTranasctionFromWorldAccount() {
            return new CachedTransaction() { SourceBankAccountK = SEconomyPlugin.WorldAccount.BankAccountK };
        }
    }

}