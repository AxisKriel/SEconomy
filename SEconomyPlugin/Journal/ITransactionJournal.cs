using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Journal {
    public interface ITransactionJournal : IDisposable  {

        #region "Transaction Journal Events"
        event EventHandler<PendingTransactionEventArgs> BankTransactionPending;
        #endregion

		SEconomy SEconomyInstance { get; set; }

        List<IBankAccount> BankAccounts { get; }
        IEnumerable<ITransaction> Transactions { get; }

        IBankAccount AddBankAccount(string UserAccountName, long WorldID, BankAccountFlags Flags, string iDonoLol);
        IBankAccount AddBankAccount(IBankAccount Account);

        IBankAccount GetBankAccountByName(string UserAccountName);
        IBankAccount GetBankAccount(long BankAccountK);
        IEnumerable<IBankAccount> GetBankAccountList(long BankAccountK);

        void DeleteBankAccount(long BankAccountK);
        
        event EventHandler<BankTransferEventArgs> BankTransferCompleted;

        bool JournalSaving { get; set; }

        bool BackupsEnabled { get; set; }

        void SaveJournal();
        Task SaveJournalAsync();

        void LoadJournal();
        Task LoadJournalAsync();

        void BackupJournal();
        Task BackupJournalAsync();

        Task SquashJournalAsync();

        BankTransferEventArgs TransferBetween(IBankAccount FromAccount, IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage);
        Task<BankTransferEventArgs> TransferBetweenAsync(IBankAccount FromAccount, IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage);

        IBankAccount GetWorldAccount();

        void DumpSummary();
    }
}
