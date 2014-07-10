using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Journal {
    public class JournalTransactionCache : IDisposable {

		/// <summary>
		/// List of uncommitted funds
		/// </summary>
        protected ConcurrentQueue<CachedTransaction> CachedTransactions { get; set; }
        protected readonly System.Timers.Timer UncommittedFundTimer = new System.Timers.Timer(5000);

        public JournalTransactionCache()
        {
            CachedTransactions = new ConcurrentQueue<CachedTransaction>();
            UncommittedFundTimer.Elapsed += UncommittedFundTimer_Elapsed;
            UncommittedFundTimer.Start();
        }

		/// <summary>
		/// Occurs when the cached payments timer needs to commit all the uncommitted transactions
		/// </summary>
        protected async void UncommittedFundTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
			await ProcessQueueAsync();
        }

		/// <summary>
		/// Adds a fund to the uncommitted cache
		/// </summary>
        public void AddCachedTransaction(CachedTransaction Fund) {
            CachedTransactions.Enqueue(Fund);
        }

		/// <summary>
		/// Processes all elements in the queue and transfers them
		/// </summary>
        protected async Task ProcessQueueAsync() {
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
                Journal.IBankAccount sourceAccount = SEconomyPlugin.Instance.RunningJournal.GetBankAccount(aggregatedFund.SourceBankAccountK);
				Journal.IBankAccount destAccount = SEconomyPlugin.Instance.RunningJournal.GetBankAccount(aggregatedFund.DestinationBankAccountK);

                if (sourceAccount != null && destAccount != null) {
                    StringBuilder messageBuilder = new StringBuilder(aggregatedFund.Message);

                    if (aggregatedFund.Aggregations > 1) {
                        messageBuilder.Insert(0, aggregatedFund.Aggregations + " ");
                        messageBuilder.Append("s");
                    }
                    //transfer the money
                    BankTransferEventArgs transfer = await sourceAccount.TransferToAsync(destAccount, aggregatedFund.Amount, aggregatedFund.Options, messageBuilder.ToString(), messageBuilder.ToString());
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

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true) {
                UncommittedFundTimer.Elapsed -= UncommittedFundTimer_Elapsed;
                UncommittedFundTimer.Stop();
                UncommittedFundTimer.Dispose();
                /*
                 * Flush remaining items in the queue before releasing resources.
                 */
                ProcessQueueAsync().Wait();
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
    }

}