using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace Wolfje.Plugins.SEconomy.CmdAliasModule {
	public class Configuration {

		public List<AliasCommand> CommandAliases = new List<AliasCommand>();

		/// <summary>
		/// Loads a configuration file and deserializes it from JSON
		/// </summary>
		public static Configuration LoadConfigurationFromFile(string Path)
		{
			Configuration config = null;

			try {
				string fileText = System.IO.File.ReadAllText(Path);

				config = JsonConvert.DeserializeObject<Configuration>(fileText);

			} catch (Exception ex) {
				if (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException) {
					TShockAPI.Log.ConsoleError("cmdalias configuration: Cannot find file or directory. Creating new one.");

					config = Configuration.NewSampleConfiguration();

					config.SaveConfiguration(Path);

				} else if (ex is System.Security.SecurityException) {
					TShockAPI.Log.ConsoleError("cmdalias configuration: Access denied reading file " + Path);
				} else {
					TShockAPI.Log.ConsoleError("cmdalias configuration: error " + ex.ToString());
				}
			}

			return config;
		}

		public static Configuration NewSampleConfiguration()
		{
			Configuration newConfig = new Configuration();

			newConfig.CommandAliases.Add(AliasCommand.Create("testparms", "", "0c", "", 0, "/bc Input param 1 2 3: $1 $2 $3", "/bc Input param 1-3: $1-3", "/bc Input param 2 to end of line: $2-"));
			newConfig.CommandAliases.Add(AliasCommand.Create("testrandom", "", "0c", "", 0, "/bc Random Number: $random(1,100)"));
			newConfig.CommandAliases.Add(AliasCommand.Create("impersonate", "", "0c", "", 0, "$runas($1,/me can fit $random(1,100) cocks in their mouth at once.)"));

			return newConfig;
		}

		public void SaveConfiguration(string Path)
		{

			try {
				string config = JsonConvert.SerializeObject(this, Formatting.Indented);

				System.IO.File.WriteAllText(Path, config);

			} catch (Exception ex) {

				if (ex is System.IO.DirectoryNotFoundException) {
					TShockAPI.Log.ConsoleError("cmdalias config: save directory not found: " + Path);

				} else if (ex is UnauthorizedAccessException || ex is System.Security.SecurityException) {
					TShockAPI.Log.ConsoleError("cmdalias config: Access is denied to Vault config: " + Path);
				} else {
					TShockAPI.Log.ConsoleError("cmdalias config: Error reading file: " + Path);
					throw;
				}
			}

		}

	}

 
}
