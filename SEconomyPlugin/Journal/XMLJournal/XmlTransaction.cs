using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy.Journal {
	public class XmlTransaction : ITransaction {
		IBankAccount owningBankAccount;
		Dictionary<string, object> customValues;

		#region "custom keys"
		public const string kXmlTransactionOldTransactonK = "kXmlTransactionOldTransactonK";
		public const string kXmlTransactionOldTransactonFK = "kXmlTransactionOldTransactonK";
		#endregion

		#region "public properties"
		public long BankAccountTransactionK
		{
			get;
			set;
		}

		public long BankAccountFK
		{
			get;
			set;
		}

		public Money Amount
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		public BankAccountTransactionFlags Flags
		{
			get;
			set;
		}

		public BankAccountTransactionFlags Flags2
		{
			get;
			set;
		}

		public DateTime TransactionDateUtc
		{
			get;
			set;
		}

		public long BankAccountTransactionFK
		{
			get;
			set;
		}
		#endregion

		public XmlTransaction(IBankAccount OwningAccount)
		{
			if (OwningAccount == null) {
				throw new ArgumentNullException("OwningAccount is null");
			}
			this.owningBankAccount = OwningAccount;
			this.BankAccountFK = OwningAccount.BankAccountK;
			this.customValues = new Dictionary<string, object>();
		}

		public IBankAccount BankAccount
		{
			get { return owningBankAccount; }
		}

		public ITransaction OppositeTransaction
		{
			get { return this.owningBankAccount.OwningJournal.Transactions.FirstOrDefault(i => i.BankAccountTransactionK == this.BankAccountTransactionFK); }
		}

		public Dictionary<string, object> CustomValues
		{
			get
			{
				return customValues;
			}
			set
			{
				customValues = value;
			}
		}
	}
}
