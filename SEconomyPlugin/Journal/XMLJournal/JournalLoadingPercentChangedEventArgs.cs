using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy.Journal.XMLJournal {
	public class JournalLoadingPercentChangedEventArgs : EventArgs {
		public string Label { get; set; }
		public int JournalLength { get; set; }
		public int Percent { get; set; }
	}
}
