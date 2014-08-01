using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using Wolfje.Plugins.SEconomy.Journal;

namespace Wolfje.Plugins.SEconomy {
	public class SEconomy : IDisposable {
		public Journal.ITransactionJournal RunningJournal { get; set; }

		public Config Configuration { get; set; }

		public SEconomyPlugin PluginInstance { get; set; }

		public Journal.JournalTransactionCache TransactionCache { get; set; }

		public Journal.IBankAccount WorldAccount { get; internal set; }

		public WorldEconomy WorldEc { get; internal set; }

		internal EventHandlers EventHandlers { get; set; }

		internal ChatCommands ChatCommands { get; set; }

		public Dictionary<Player, DateTime> IdleCache { get; protected set; }

		public SEconomy(SEconomyPlugin PluginInstance)
		{
			this.IdleCache = new Dictionary<Player, DateTime>();
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
			} catch (Exception) {
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
			} else if (Configuration.JournalType.Equals("mysql", StringComparison.InvariantCultureIgnoreCase) == true
			           || Configuration.JournalType.Equals("sql", StringComparison.InvariantCultureIgnoreCase) == true) {
				Wolfje.Plugins.SEconomy.Journal.MySQLJournal.MySQLTransactionJournal sqlJournal = new Journal.MySQLJournal.MySQLTransactionJournal(this, Configuration.SQLConnectionProperties);
				sqlJournal.JournalLoadingPercentChanged += (sender, args) => {
					ConsoleEx.WriteBar(args);
				};

				journal = sqlJournal;
			}

			this.RunningJournal = journal;
			this.RunningJournal.LoadJournal();
		}

		internal async Task<Journal.IBankAccount> CreatePlayerAccountAsync(TSPlayer player)
		{
			Money startingMoney;
			Journal.IBankAccount newAccount = SEconomyPlugin.Instance.RunningJournal.AddBankAccount(player.UserAccountName, 
				                                  Terraria.Main.worldID, 
				                                  Journal.BankAccountFlags.Enabled, 
				                                  "");

			TShockAPI.Log.ConsoleInfo(string.Format("seconomy: bank account for {0} created.", player.UserAccountName));

			if (Money.TryParse(SEconomyPlugin.Instance.Configuration.StartingMoney, out startingMoney)
			    && startingMoney > 0) {

				await SEconomyPlugin.Instance.WorldAccount.TransferToAsync(newAccount, startingMoney, 
					Journal.BankAccountTransferOptions.AnnounceToReceiver, 
					"starting out.", "starting out.");
			}

			return newAccount;
		}

		/// <summary>
		/// Called after LoadSEconomy, or on PostInitialize, this binds the current SEconomy instance
		/// to the currently running terraria world.
		/// </summary>
		public async Task BindToWorldAsync()
		{
			IBankAccount account = null;

			if ((WorldAccount = RunningJournal.GetWorldAccount()) == null) {
				TShockAPI.Log.ConsoleError("seconomy bind:  The journal system did not return a world account.  This is an internal error.");
				return;
			}

			await WorldAccount.SyncBalanceAsync();
			TShockAPI.Log.ConsoleInfo(string.Format(SEconomyPlugin.Locale.StringOrDefault(1, "SEconomy: world account: paid {0} to players."), WorldAccount.Balance.ToLongString()));

			await Task.Delay(5000);
			foreach (var player in TShockAPI.TShock.Players) {
				if (player == null
				    || string.IsNullOrWhiteSpace(player.Name) == true
				    || string.IsNullOrWhiteSpace(player.UserAccountName) == true
				    || (account = GetBankAccount(player)) == null) {
					continue;
				}
                
				await account.SyncBalanceAsync();
			}
		}

		#endregion

		#region "Player idle caching"

		public void RemovePlayerIdleCache(Player player)
		{
			if (IdleCache == null
			    || player == null
			    || IdleCache.ContainsKey(player) == false) {
				return;
			}

			IdleCache.Remove(player);
		}

		public TimeSpan? PlayerIdleSince(Player player)
		{
			if (player == null
			    || IdleCache == null
			    || IdleCache.ContainsKey(player) == false) {
				return null;
			}

			return DateTime.UtcNow.Subtract(IdleCache[player]);
		}

		public void UpdatePlayerIdle(Player player)
		{
			if (player == null || IdleCache == null) {
				return;
			}

			if (IdleCache.ContainsKey(player) == false) {
				IdleCache.Add(player, DateTime.UtcNow);
			}

			IdleCache[player] = DateTime.UtcNow;
		}

		#endregion

		public int PurgeAccounts()
		{
			
			return 0;
		}

		public IBankAccount GetBankAccount(TShockAPI.TSPlayer tsPlayer)
		{
			if (tsPlayer == null || RunningJournal == null) {
				return null;
			}

			if (tsPlayer == TSPlayer.Server) {
				return WorldAccount;
			}

			try {
				return RunningJournal.GetBankAccountByName(tsPlayer.UserAccountName);
			} catch (Exception ex) {
				TShockAPI.Log.ConsoleError("seconomy error: Error getting bank account for {0}: {1}", 
					tsPlayer.Name, ex.Message);
				return null;
			}
		}

		public IBankAccount GetBankAccount(Terraria.Player player)
		{
			return GetBankAccount(player.whoAmi);
		}

		public IBankAccount GetBankAccount(string userAccountName)
		{
			return GetBankAccount(TShockAPI.TShock.Players.FirstOrDefault(i => i != null && i.UserAccountName == userAccountName));
		}

		public IBankAccount GetPlayerBankAccount(string playerName)
		{
			return GetBankAccount(TShockAPI.TShock.Players.FirstOrDefault(i => i != null && i.Name == playerName));
		}

		public IBankAccount GetBankAccount(int who)
		{
			if (who < 0) {
				return GetBankAccount(TSPlayer.Server);
			}
			return GetBankAccount(TShockAPI.TShock.Players.FirstOrDefault(i => i != null && i.Index == who));
		}

		/// <summary>
		/// Gets an economy-enabled player by their player name. 
		/// </summary>
		[Obsolete("Use GetBankAccount() instead.", true)]
		public object GetEconomyPlayerByBankAccountNameSafe(string Name)
		{
			throw new NotSupportedException("Use GetBankAccount() instead.");
		}

		/// <summary>
		/// Gets an economy-enabled player by their index.  This method is thread-safe.
		/// </summary>
		[Obsolete("Use GetBankAccount() instead.", true)]
		public object GetEconomyPlayerSafe(int Id)
		{
			throw new NotSupportedException("Use GetBankAccount() instead.");
		}

		/// <summary>
		/// Gets an economy-enabled player by their player name. 
		/// </summary>
		[Obsolete("Use GetBankAccount() instead.", true)]
		public object GetEconomyPlayerSafe(string Name)
		{
			throw new NotSupportedException("Use GetBankAccount() instead.");
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
