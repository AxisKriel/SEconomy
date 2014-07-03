 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace Wolfje.Plugins.SEconomy.Lang {
	public class Localization {
		public string Locale { get; protected set; }
		public string[] StringTable { get; protected set; }

		public Localization(string Locale)
		{
			this.Locale = Locale;

			if (Load() < 0) {
				TShockAPI.Log.ConsoleError("SEconomy language loading failed.");
			}
		}

		public string StringOrDefault(int index, string defaultText = "")
		{
			string target = null;
			if (this.StringTable == null 
				|| (target = this.StringTable.ElementAtOrDefault(index)) == null) {
				target = defaultText;
			}

			return target;
		}

		public int Load()
		{
			XDocument localeDoc = null;
			int tableCount = 0;
			string localePath = string.Format("{1}{0}Lang{0}{2}.xml", System.IO.Path.DirectorySeparatorChar, Config.BaseDirectory, this.Locale);

			if (string.IsNullOrEmpty(Locale) == true) {
				return -1;
			}

			if (File.Exists(localePath) == false) {
				TShockAPI.Log.ConsoleError("seconomy locale: Language for locale {0} doesn't exist.", Locale);
				return -1;
			}

			try {
				localeDoc = XDocument.Load(localePath);
			} catch {
				TShockAPI.Log.ConsoleError("Could not load language {0}.", Locale);
				return -1;
			}

			tableCount = localeDoc.Root.Elements().Count();
			this.StringTable = new string[tableCount];

			for (int i = 0; i < tableCount; i++) {
				XElement stringElement = null;
				if ((stringElement = localeDoc.Root.Elements().ElementAtOrDefault(i)) == null) {
					return -1;
				}

				if ((this.StringTable[i] = stringElement.Descendants().SingleOrDefault().Value) == null) {
					return -1;
				}
			}

			return 0;
		}
	}
}
