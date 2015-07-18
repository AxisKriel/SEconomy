using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;

namespace Wolfje.Plugins.SEconomy.CmdAliasModule {
	public class Configuration {

		public List<AliasCommand> CommandAliases = new List<AliasCommand>();

		/// <summary>
		/// Loads a configuration file and deserializes it from JSON
		/// </summary>
		public static Configuration LoadConfigurationFromFile(string Path)
		{
			Configuration config = null;
			string confDirectory = System.IO.Path.Combine(Path, "aliascmd.conf.d");

			try {
				string fileText = System.IO.File.ReadAllText(Path);
				config = JsonConvert.DeserializeObject<Configuration>(fileText);
			} catch (Exception ex) {
				if (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException) {
					TShock.Log.ConsoleError("cmdalias configuration: Cannot find file or directory. Creating new one.");

					config = Configuration.NewSampleConfiguration();
					config.SaveConfiguration(Path);
				} else if (ex is System.Security.SecurityException) {
					TShock.Log.ConsoleError("cmdalias configuration: Access denied reading file " + Path);
				} else {
					TShock.Log.ConsoleError("cmdalias configuration: error " + ex.ToString());
				}
			}

			if (Directory.Exists(confDirectory) == false) {
				try {
					Directory.CreateDirectory(confDirectory);
				} catch {
				}
			}	

			if (Directory.Exists(confDirectory) == false) {
				return config;
			}

			/*
			 * Command aliases in .json files in the subdirectory "aliascmd.conf.d"
			 * will be loaded into the main configuration.
			 */
			foreach (string aliasFile in Directory.EnumerateFiles(confDirectory, "*.json")) {
				string fileText = null;
				Configuration extraConfig = null;

				if (string.IsNullOrEmpty(aliasFile)
				    || File.Exists(aliasFile) == false) {
					continue;
				}

				try {
					fileText = File.ReadAllText(aliasFile);
				} catch {
				}

				if (string.IsNullOrEmpty(fileText) == true) {
					continue;
				}

				try {
					extraConfig = JsonConvert.DeserializeObject<Configuration>(fileText);
				} catch {
				}
			
				if (extraConfig == null) {
					continue;
				}

				foreach (AliasCommand alias in extraConfig.CommandAliases) {
					if (config.CommandAliases.FirstOrDefault(i => i.CommandAlias == alias.CommandAlias) != null) {
						TShock.Log.ConsoleError("aliascmd warning: Duplicate alias {0} in file {1} ignored",
							alias.CommandAlias, System.IO.Path.GetFileName(aliasFile));
						continue;
					}

					config.CommandAliases.Add(alias);
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
					TShock.Log.ConsoleError("cmdalias config: save directory not found: " + Path);

				} else if (ex is UnauthorizedAccessException || ex is System.Security.SecurityException) {
					TShock.Log.ConsoleError("cmdalias config: Access is denied to Vault config: " + Path);
				} else {
					TShock.Log.ConsoleError("cmdalias config: Error reading file: " + Path);
					throw;
				}
			}

		}

	}

 
}
