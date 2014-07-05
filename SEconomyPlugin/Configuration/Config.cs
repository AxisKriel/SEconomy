using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace Wolfje.Plugins.SEconomy {
    public class Config {
		public static string BaseDirectory = @"tshock" + System.IO.Path.DirectorySeparatorChar + "SEconomy";
        public static string JournalPath = Config.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "SEconomy.journal.xml.gz";
		protected string path;

        public bool BankAccountsEnabled = true;
        public string StartingMoney = "0";

        public int PayIntervalMinutes = 30;
        public int IdleThresholdMinutes = 10;
        public string IntervalPayAmount = "0";
		public string JournalType = "xml";
        public int JournalBackupMinutes = 1;
        public MoneyProperties MoneyConfiguration = new MoneyProperties();
		public Configuration.SQLConnectionProperties SQLConnectionProperties = new Configuration.SQLConnectionProperties();
        public bool EnableProfiler = false;

		public Config(string path)
		{
			this.path = path;
		}

        /// <summary>
        /// Loads a configuration file and deserializes it from JSON
        /// </summary>
        public static Config FromFile(string Path) {
            Config config = null;

			if (!System.IO.Directory.Exists(Config.BaseDirectory)) {
				try {
					System.IO.Directory.CreateDirectory(Config.BaseDirectory);
				} catch {
					TShockAPI.Log.ConsoleError("seconomy configuration: Cannot create base directory: {0}", Config.BaseDirectory);
					return null;
				}
			}

			try {
				string fileText = System.IO.File.ReadAllText(Path);
				config = JsonConvert.DeserializeObject<Config>(fileText);
			} catch (Exception ex) {
				if (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException) {
					TShockAPI.Log.ConsoleError("seconomy configuration: Cannot find file or directory. Creating new one.");
					config = new Config(Path);
					config.SaveConfiguration();
				} else if (ex is System.Security.SecurityException) {
					TShockAPI.Log.ConsoleError("seconomy configuration: Access denied reading file " + Path);
				} else {
					TShockAPI.Log.ConsoleError("seconomy configuration: error " + ex.ToString());
				}
			}
            return config;
        }


        public void SaveConfiguration() {
            try {
                string config = JsonConvert.SerializeObject(this, Formatting.Indented);
                System.IO.File.WriteAllText(path, config);
            } catch (Exception ex) {

                if (ex is System.IO.DirectoryNotFoundException) {
					TShockAPI.Log.ConsoleError("seconomy config: save directory not found: " + path);

                } else if (ex is UnauthorizedAccessException || ex is System.Security.SecurityException) {
					TShockAPI.Log.ConsoleError("seconomy config: Access is denied to config: " + path);
                } else {
					TShockAPI.Log.ConsoleError("seconomy config: Error reading file: " + path);
                    throw;
                }
            }
        }
    }
}
