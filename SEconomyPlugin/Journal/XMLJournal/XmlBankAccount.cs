using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Journal.XMLJournal {
    public class XmlBankAccount : IBankAccount {
        ITransactionJournal owningJournal;
        List<ITransaction> transactions;

        public XmlBankAccount(ITransactionJournal OwningJournal) {
            this.owningJournal = OwningJournal;
            this.transactions = new List<ITransaction>();
        }

        #region "Public Properties"
        public long BankAccountK { get; set; }

        public string OldBankAccountK {
            get;
            set;
        }

        public string UserAccountName {
            get;
            set;
        }

        public long WorldID {
            get;
            set;
        }

        public BankAccountFlags Flags {
            get;
            set;
        }

        public string Description {
            get;
            set;
        }

        public Money Balance {
            get;
            set;
        }

        public Economy.EconomyPlayer Owner {
            get {
                return OwningJournal.SEconomyInstance.GetEconomyPlayerByBankAccountNameSafe(this.UserAccountName);
            }
        }

        public bool IsAccountEnabled {
            get { return (this.Flags & Journal.BankAccountFlags.Enabled) == Journal.BankAccountFlags.Enabled; }
        }

        public bool IsSystemAccount {
            get { return (this.Flags & Journal.BankAccountFlags.SystemAccount) == Journal.BankAccountFlags.SystemAccount; }
        }

        public bool IsLockedToWorld {
            get { return (this.Flags & Journal.BankAccountFlags.LockedToWorld) == Journal.BankAccountFlags.LockedToWorld; }
        }

        public bool IsPluginAccount {
            get { return (this.Flags & Journal.BankAccountFlags.PluginAccount) == Journal.BankAccountFlags.PluginAccount; }
        }

        public List<ITransaction> Transactions { get { return transactions; } }
        
        public ITransactionJournal OwningJournal { get { return owningJournal; } }
        #endregion

        public void SyncBalance() {
            this.Balance = this.Transactions.Sum(i => i.Amount);
        }

        public System.Threading.Tasks.Task SyncBalanceAsync() {
            return System.Threading.Tasks.Task.Factory.StartNew(() => SyncBalance());
        }

        public BankTransferEventArgs TransferTo(IBankAccount Account, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage) {
            return owningJournal.TransferBetween(this, Account, Amount, Options, TransactionMessage, JournalMessage);
        }

        public async Task<BankTransferEventArgs> TransferToAsync(int Index, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage) {
            Economy.EconomyPlayer ePlayer = OwningJournal.SEconomyInstance.GetEconomyPlayerSafe(Index);

            return await Task.Factory.StartNew(() => TransferTo(ePlayer.BankAccount, Amount, Options, TransactionMessage, JournalMessage));
        }

        public async Task<BankTransferEventArgs> TransferToAsync(IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage) {
            return await Task.Factory.StartNew(() => TransferTo(ToAccount, Amount, Options, TransactionMessage, JournalMessage));
        }

        public ITransaction AddTransaction(ITransaction Transaction) {
            if (Transaction.BankAccountTransactionK == 0) {
                Transaction.BankAccountTransactionK = Interlocked.Increment(ref XmlTransactionJournal._transactionSeed);
            } else {
                if (Transaction.BankAccountTransactionK > XmlTransactionJournal._transactionSeed) {
                    Interlocked.Exchange(ref XmlTransactionJournal._transactionSeed, Transaction.BankAccountTransactionK);
                }
            }

			lock (Transactions) {
				this.transactions.Add(Transaction);
			}

            return Transaction;
        }

        public void ResetAccountTransactions(long BankAccountK) {
			lock (Transactions) {
				this.Transactions.Clear();
			}

            this.SyncBalance();
        }

        public Task ResetAccountTransactionsAsync(long BankAccountK) {
            return Task.Factory.StartNew(() => ResetAccountTransactions(BankAccountK));
        }

    }
}
