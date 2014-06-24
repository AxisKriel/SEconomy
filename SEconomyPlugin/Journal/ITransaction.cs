using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy.Journal {
    public interface ITransaction {

        long BankAccountTransactionK { get; set; }
        long BankAccountFK { get; set; }
        Money Amount { get; set; }

        string Message { get; set; }

        BankAccountTransactionFlags Flags { get; set; }
        BankAccountTransactionFlags Flags2 { get; set; }

        DateTime TransactionDateUtc { get; set; }

        long BankAccountTransactionFK { get; set; }

        IBankAccount BankAccount { get; }
        ITransaction OppositeTransaction { get; }

        Dictionary<string, object> CustomValues { get; }
    }
}
