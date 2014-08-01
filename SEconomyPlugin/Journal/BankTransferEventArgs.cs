using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy.Journal {
	/// <summary>
	/// Describes a bank account transaction
	/// </summary>
	public class BankTransferEventArgs : EventArgs {
		public bool TransferSucceeded { get; set; }

		public IBankAccount ReceiverAccount { get; set; }

		public IBankAccount SenderAccount { get; set; }

		public long TransactionID { get; set; }

		public Money Amount { get; set; }

		public Exception Exception { get; set; }

		public BankAccountTransferOptions TransferOptions { get; set; }

		public string TransactionMessage { get; set; }
	}
}
