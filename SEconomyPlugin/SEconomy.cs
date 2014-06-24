using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy {
	public class SEconomy : IDisposable {
		public Journal.ITransactionJournal RunningJournal { get; internal set; }
		public Config Configuration { get; private set; }
		public SEconomyPlugin PluginInstance { get; set; }
		public Journal.IBankAccount WorldAccount { get; protected set; }
		public WorldEconomy.WorldEconomy WorldEconomy { get; protected set; }
		
		protected List<Economy.EconomyPlayer> EconomyPlayers { get; set; }
		protected System.Timers.Timer PayRunTimer { get; set; }
		protected System.Timers.Timer JournalBackupTimer { get; set; }

		internal Performance.Profiler Profiler = new Performance.Profiler();

		public SEconomy(SEconomyPlugin PluginInstance)
		{
			this.PluginInstance = PluginInstance;
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
		}

		#region "Event Handlers"
		protected virtual void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (e.Observed == true) {
				return;
			}
			
			TShockAPI.Log.ConsoleError("seconomy async: error occurred on a task thread: " + e.Exception.Flatten().ToString());
			e.SetObserved();
		}
		#endregion

		#region "Loading and setup"
		public int LoadSEconomy()
		{
			if (LoadConfiguration() < 0 || LoadJournal() < 0
				|| LoadJournalBackups() < 0 || LoadWorldEconomy() < 0) {
				TShockAPI.Log.ConsoleError("Initialization of SEconomy failed.");
				return -1;
			}


			return 0;
		}

		/// <summary>
		/// Loads SEconomy configuration from disk.
		/// </summary>
		/// <returns>0 if the operation was successful, less otherwise.</returns>
		protected int LoadConfiguration()
		{
			try {
				this.Configuration = Config.LoadConfigurationFromFile(Config.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "SEconomy.config.json");
			} catch (Exception ex) {
				TShockAPI.Log.ConsoleError("LoadConfiguration: Exception loading configuration: {0}", ex.ToString());
				return -1;
			}
			return 0;
		}

		/// <summary>
		/// Loads the journal into memory from XML.
		/// </summary>
		/// <returns>0 if the operation was successful, less otherwise.</returns>
		protected int LoadJournal()
		{
			try {
				this.RunningJournal = new Journal.XmlTransactionJournal(new Dictionary<string, object> {
                    { Journal.XmlTransactionJournal.kXMLTransactionJournalSavePath, Config.JournalPath }
                });

				RunningJournal.LoadJournal();

				//RunningJournal.BankTransferCompleted += BankAccount_BankTransferCompleted;
			} catch {
				TShockAPI.Log.ConsoleError("SEconomy: xml initialization failed.");
				return -1;
			}

			return 0;
		}

		protected int LoadJournalBackups()
		{
			JournalBackupTimer = new System.Timers.Timer(Configuration.JournalBackupMinutes * 60000);
			//JournalBackupTimer.Elapsed += JournalBackupTimer_Elapsed;
			JournalBackupTimer.Start();

			return 0;
		}

		protected int LoadHooks()
		{
			return 0;
		}

		protected int LoadWorldEconomy()
		{
			try {
				this.WorldEconomy = new WorldEconomy.WorldEconomy(this);
			} catch (Exception) {
				TShockAPI.Log.ConsoleError("seconomy init: WorldEconomy failed to initialize.");
				return -1;
			}
			return 0;
		}
		#endregion

		#region "Unloading"
		protected int UnloadHooks()
		{
			//TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= PlayerHooks_PlayerPostLogin;

			//ServerApi.Hooks.GamePostInitialize.Deregister(this, GameHooks_PostInitialize);
			//ServerApi.Hooks.ServerJoin.Deregister(this, ServerHooks_Join);
			//ServerApi.Hooks.ServerLeave.Deregister(this, ServerHooks_Leave);
			//ServerApi.Hooks.NetGetData.Deregister(this, NetHooks_GetData);
			//SEconomyPlugin.RunningJournal.BankTransferCompleted -= BankAccount_BankTransferCompleted;

			TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
			return 0;
		}
		#endregion

		#region "IDisposable"
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing == true) {
				UnloadHooks();
			}

			//HaltIfBackup();
		}
		#endregion
	}
}
