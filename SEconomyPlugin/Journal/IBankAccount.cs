using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Journal {
    public interface IBankAccount {

        ITransactionJournal OwningJournal {  get; }

        long BankAccountK { get; set; }
        string OldBankAccountK { get; set; }
        string UserAccountName { get; set; }
        long WorldID { get; set; }
        BankAccountFlags Flags { get; set; }
        string Description { get; set; }

        Money Balance { get; set; }

        bool IsAccountEnabled { get; }
        bool IsSystemAccount { get; }
        bool IsLockedToWorld { get; }
        bool IsPluginAccount { get; }

        List<ITransaction> Transactions { get; }

        ITransaction AddTransaction(ITransaction Transaction);
        void ResetAccountTransactions(long BankAccountK);
        Task ResetAccountTransactionsAsync(long BankAccountK);

        void SyncBalance();
        Task SyncBalanceAsync();

        BankTransferEventArgs TransferTo(IBankAccount Account, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage);

        Task<BankTransferEventArgs> TransferToAsync(int Index, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage);
        Task<BankTransferEventArgs> TransferToAsync(IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage);

    }
}
