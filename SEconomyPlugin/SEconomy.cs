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

		public Journal.JournalTransactionCache TransactionCache { get; set; }
		public Journal.IBankAccount WorldAccount { get; internal set; }
		public WorldEconomy WorldEc { get; internal set; }
		public List<Economy.EconomyPlayer> EconomyPlayers { get; internal set; }

		internal System.Timers.Timer PayRunTimer { get; set; }
		internal EventHandlers EventHandlers { get; set; }
		internal ChatCommands ChatCommands { get; set; }

		public SEconomy(SEconomyPlugin PluginInstance)
		{
			this.PluginInstance = PluginInstance;
		}

		
		#region "Loading and setup"
		
		public int LoadSEconomy()
		{
            try {
				this.Configuration = Config.FromFile(Config.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "SEconomy.config.json");
				LoadJournal();
                this.WorldEc = new WorldEconomy(this);
                this.EventHandlers = new EventHandlers(this);
                this.ChatCommands = new ChatCommands(this);
                this.TransactionCache = new Journal.JournalTransactionCache();
				this.EconomyPlayers = new List<Economy.EconomyPlayer>();
            }
            catch (Exception ex) {
                TShockAPI.Log.ConsoleError(SEconomyPlugin.Locale.StringOrDefault(0, "Initialization of SEconomy failed: ") + ex.ToString());
                return -1;
            }

			return 0;
		}

		protected void LoadJournal()
		{
			Journal.ITransactionJournal journal = null;
			if (Configuration == null) {
				return;
			}

			if (Configuration.JournalType.Equals("xml", StringComparison.InvariantCultureIgnoreCase) == true) {
				Wolfje.Plugins.SEconomy.Journal.XMLJournal.XmlTransactionJournal xmlJournal = new Journal.XMLJournal.XmlTransactionJournal(this, Config.JournalPath);
				xmlJournal.JournalLoadingPercentChanged += (sender, args) => {
					ConsoleEx.WriteBar(args);
				};

				journal = xmlJournal;
			} else if (Configuration.JournalType.Equals("mysql", StringComparison.InvariantCultureIgnoreCase) == true) {
				Wolfje.Plugins.SEconomy.Journal.MySQLJournal.MySQLTransactionJournal sqlJournal = new Journal.MySQLJournal.MySQLTransactionJournal(this, Configuration.SQLConnectionProperties);
				sqlJournal.JournalLoadingPercentChanged += (sender, args) => {
					ConsoleEx.WriteBar(args);
				};

				journal = sqlJournal;
			}

			this.RunningJournal = journal;
			this.RunningJournal.LoadJournal();
		}

		/// <summary>
		/// Called after LoadSEconomy, or on PostInitialize, this binds the current SEconomy instance
		/// to the currently running terraria world.
		/// </summary>
		public async Task BindToWorldAsync()
		{
			WorldAccount = RunningJournal.GetWorldAccount();
			await WorldAccount.SyncBalanceAsync();
			TShockAPI.Log.ConsoleInfo(string.Format(SEconomyPlugin.Locale.StringOrDefault(1, "SEconomy: world account: paid {0} to players."), WorldAccount.Balance.ToLongString()));

			PayRunTimer.Start();
		}

		#endregion

		/// <summary>
		/// Gets an economy-enabled player by their player name. 
		/// </summary>
		public Economy.EconomyPlayer GetEconomyPlayerByBankAccountNameSafe(string Name)
		{
			if (EconomyPlayers == null) {
				return null;
			}

			return EconomyPlayers.FirstOrDefault(i => (i.BankAccount != null) && i.BankAccount.UserAccountName == Name);
		}

		/// <summary>
		/// Gets an economy-enabled player by their index.  This method is thread-safe.
		/// </summary>
		public Economy.EconomyPlayer GetEconomyPlayerSafe(int Id)
		{
			if (Id < 0) {
				return new Economy.EconomyPlayer(-1) {
					BankAccount = WorldAccount
				};
			}

			return this.EconomyPlayers.FirstOrDefault(i => i.Index == Id);
		}

		/// <summary>
		/// Gets an economy-enabled player by their player name. 
		/// </summary>
		public Economy.EconomyPlayer GetEconomyPlayerSafe(string Name)
		{
			if (EconomyPlayers == null) {
				return null;
			}
			return EconomyPlayers.FirstOrDefault(i => i.TSPlayer.Name == Name);
		}

		#region "IDisposable"
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing == true) {
				this.EventHandlers.Dispose();
				this.ChatCommands.Dispose();
				this.WorldEc.Dispose();
                this.TransactionCache.Dispose();
				this.RunningJournal.Dispose();
				this.Configuration = null;
			}

		}
		#endregion
	}
}
