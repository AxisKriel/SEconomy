using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Journal.MySQLJournal {
	public class MySQLTransaction : ITransaction {
		protected IBankAccount bankAccount;

		public MySQLTransaction(IBankAccount bankAccount)
		{
			this.bankAccount = bankAccount;
		}

		#region ITransaction Members

		public long BankAccountTransactionK { get; set; }

		public long BankAccountFK { get; set; }
		public Money Amount { get; set; }
		public string Message { get; set; }

		public BankAccountTransactionFlags Flags { get; set; }

		public BankAccountTransactionFlags Flags2 { get; set; }

		public DateTime TransactionDateUtc { get; set; }

		public long BankAccountTransactionFK { get; set; }

		public IBankAccount BankAccount
		{
			get { return bankAccount; }
		}

		public ITransaction OppositeTransaction
		{
			get { return null; }
		}

		public Dictionary<string, object> CustomValues
		{
			get { return null; }
		}

		#endregion

		public object TransactionObject { get; set; }
	}
}
