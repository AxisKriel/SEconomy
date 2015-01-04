using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.CmdAliasModule {
	public class AliasExecutedEventArgs : EventArgs {
		public string CommandIdentifier { get; set; }

		public TShockAPI.CommandArgs CommandArgs { get; set; }
	}
}
