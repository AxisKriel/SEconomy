using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy.Journal {
    public class PendingTransactionEventArgs : EventArgs {
        IBankAccount fromAccount;
        IBankAccount toAccount;

        public PendingTransactionEventArgs(IBankAccount From, IBankAccount To, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string LogMessage) : base() {
            this.fromAccount = From;
            this.toAccount = To;
            this.Amount = Amount;
            this.Options = Options;
            this.TransactionMessage = TransactionMessage;
            this.JournalLogMessage = LogMessage;
            this.IsCancelled = false;
        }

        /// <summary>
        /// Specifies the source bank account the amount is to be transferred from
        /// </summary>
        public IBankAccount FromAccount {
            get {
                return fromAccount;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("FromAaccount cannot be set to null.");
                }

                fromAccount = value;
            }
        }

        /// <summary>
        /// Specifies the destination account the amount is to be transferred to
        /// </summary>
        public IBankAccount ToAccount {
            get {
                return toAccount;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("ToAccount cannot be set to null.");
                }

                toAccount = value;
            }
        }

        public Money Amount { get; set; }

        public BankAccountTransferOptions Options { get; private set; }

        /// <summary>
        /// Specifies the transaction message 
        /// </summary>
        public string TransactionMessage { get; set; }

        /// <summary>
        /// Specifies the journal message
        /// </summary>
        public string JournalLogMessage { get; set; }

        /// <summary>
        /// Cancels the transaction
        /// </summary>
        public bool IsCancelled { get; set; }

    }
}
