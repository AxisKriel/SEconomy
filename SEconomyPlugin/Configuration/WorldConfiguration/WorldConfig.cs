using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Wolfje.Plugins.SEconomy.Configuration.WorldConfiguration {
	public class WorldConfig {
		public bool MoneyFromNPCEnabled = true;
		public bool MoneyFromBossEnabled = true;
		public bool MoneyFromPVPEnabled = true;

		public bool AnnounceNPCKillGains = true;
		public bool AnnounceBossKillGains = true;

		public bool StaticDeathPenalty = false;
		public long StaticPenaltyAmount = 0;
		public List<StaticPenaltyOverride> StaticPenaltyOverrides = new List<StaticPenaltyOverride>();

		public bool KillerTakesDeathPenalty = true;
		public decimal DeathPenaltyPercentValue = 10.0M;

		public decimal MoneyPerDamagePoint = 1.0M;

		public List<NPCRewardOverride> Overrides = new List<NPCRewardOverride>();

		public WorldConfig()
		{
			StaticPenaltyOverrides.Add(new StaticPenaltyOverride() { StaticRewardOverride = 0, TShockGroup = "" });
		}

		/// <summary>
		/// Loads a configuration file and deserializes it from JSON
		/// </summary>
		public static WorldConfig LoadConfigurationFromFile(string Path)
		{
			WorldConfig config = null;

			try {
				string fileText = System.IO.File.ReadAllText(Path);
				config = JsonConvert.DeserializeObject<WorldConfig>(fileText);
			} catch (Exception ex) {
				if (ex is System.IO.FileNotFoundException || ex is System.IO.DirectoryNotFoundException) {
					TShockAPI.Log.ConsoleError("seconomy worldconfig: Cannot find file or directory. Creating new one.");
					config = WorldConfig.NewSampleConfiguration();
					config.SaveConfiguration(Path);
				} else if (ex is System.Security.SecurityException) {
					TShockAPI.Log.ConsoleError("seconomy worldconfig: Access denied reading file " + Path);
				} else {
					TShockAPI.Log.ConsoleError("seconomy worldconfig: error " + ex.ToString());
				}
			}

			return config;
		}

		public static WorldConfig NewSampleConfiguration()
		{
			WorldConfig newConfig = new WorldConfig();
			int[] bannedMobs = new int[] { 1, 49, 74, 46, 85, 67, 55, 63, 58, 21 };

			foreach (int bannedMobID in bannedMobs) {
				newConfig.Overrides.Add(new NPCRewardOverride() {
					NPCID = bannedMobID,
					OverridenMoneyPerDamagePoint = 0.0M
				});
			}

			newConfig.StaticPenaltyOverrides.Add(new StaticPenaltyOverride() { StaticRewardOverride = 0, TShockGroup = "" });
			return newConfig;
		}

		public void SaveConfiguration(string Path)
		{
			try {
				string config = JsonConvert.SerializeObject(this, Formatting.Indented);
				System.IO.File.WriteAllText(Path, config);
			} catch (Exception ex) {

				if (ex is System.IO.DirectoryNotFoundException) {
					TShockAPI.Log.ConsoleError("seconomy worldconfig: save directory not found: " + Path);

				} else if (ex is UnauthorizedAccessException || ex is System.Security.SecurityException) {
					TShockAPI.Log.ConsoleError("seconomy worldconfig: Access is denied to Vault config: " + Path);
				} else {
					TShockAPI.Log.ConsoleError("seconomy worldconfig: Error reading file: " + Path);
					throw;
				}
			}
		}
	}

	public class NPCRewardOverride {
		public int NPCID = 0;
		public decimal OverridenMoneyPerDamagePoint = 1.0M;
	}

	public class StaticPenaltyOverride {
		public string TShockGroup = "group";
		public long StaticRewardOverride = 0;
	}
}
