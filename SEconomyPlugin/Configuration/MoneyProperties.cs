using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy {
	public class MoneyProperties {
		public bool UseQuadrantNotation = true;
		public string MoneyName = "coin";
		public string MoneyNamePlural = "coins";
		public string SingularDisplayFormat = "N0";
		public string SingularDisplayCulture = "en-US";

		public string Quadrant1FullName = "Copper";
		public string Quadrant1ShortName = "copper";
		public string Quadrant1Abbreviation = "c";

		public string Quadrant2FullName = "Silver";
		public string Quadrant2ShortName = "silver";
		public string Quadrant2Abbreviation = "s";

		public string Quadrant3FullName = "Gold";
		public string Quadrant3ShortName = "gold";
		public string Quadrant3Abbreviation = "g";

		public string Quadrant4FullName = "Platinum";
		public string Quadrant4ShortName = "plat";
		public string Quadrant4Abbreviation = "p";
	}
}
