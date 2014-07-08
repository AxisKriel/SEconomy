using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy.Journal {
    /// <summary>
    /// Bank Account flags, sets things like enabled, system bank account, etc
    /// </summary>
    [Flags]
    public enum BankAccountFlags {
        Enabled = 1,
        SystemAccount = 1 << 1,
        LockedToWorld = 1 << 2,
        PluginAccount = 1 << 3,
    }
}
