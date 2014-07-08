using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy.Journal {
    [Flags]
    public enum BankAccountTransactionFlags {
        FundsAvailable = 1,
        Squashed = 1 << 1
    }
}
