using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;

namespace Wolfje.Plugins.SEconomy {
	/// <summary>
	/// Disposable wrapper for all data handlers that SEconomy depends on to run
	/// </summary>
	public class EventHandlers : IDisposable {
		public SEconomy Parent { get; protected set; }

		public EventHandlers(SEconomy Parent)
		{
			this.Parent = Parent;
            if (Parent.Configuration.PayIntervalMinutes > 0) {
                Parent.PayRunTimer = new System.Timers.Timer(Parent.Configuration.PayIntervalMinutes * 60000);
                Parent.PayRunTimer.Elapsed += PayRunTimer_Elapsed;
            }

			TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += PlayerHooks_PlayerPostLogin;
			Parent.RunningJournal.BankTransferCompleted += BankAccount_BankTransferCompleted;
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

			ServerApi.Hooks.GamePostInitialize.Register(this.Parent.PluginInstance, GameHooks_PostInitialize);
			ServerApi.Hooks.ServerJoin.Register(this.Parent.PluginInstance, ServerHooks_Join);
			ServerApi.Hooks.ServerLeave.Register(this.Parent.PluginInstance, ServerHooks_Leave);
			ServerApi.Hooks.NetGetData.Register(this.Parent.PluginInstance, NetHooks_GetData);
		}

		/// <summary>
		/// Default messages sent to players when a transaction happens within SEconomy.
		/// </summary>
		protected void BankAccount_BankTransferCompleted(object sender, Journal.BankTransferEventArgs e)
		{
			if (e.ReceiverAccount == null
				|| (e.TransferOptions & Journal.BankAccountTransferOptions.SuppressDefaultAnnounceMessages) 
				== Journal.BankAccountTransferOptions.SuppressDefaultAnnounceMessages) {
				return;
			}

			//Player died from PvP
			if ((e.TransferOptions & Journal.BankAccountTransferOptions.PvP) == Journal.BankAccountTransferOptions.PvP) {
				if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToReceiver) == Journal.BankAccountTransferOptions.AnnounceToReceiver) {
					e.ReceiverAccount.Owner.TSPlayer.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(14, "You killed {0} and gained {1}."), 
						e.SenderAccount.Owner.TSPlayer.Name, e.Amount.ToLongString()), Color.Orange);
				}
				if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToSender) == Journal.BankAccountTransferOptions.AnnounceToSender) {
					e.SenderAccount.Owner.TSPlayer.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(15, "{0} killed you and you lost {1}."), 
						e.ReceiverAccount.Owner.TSPlayer.Name, e.Amount.ToLongString()), Color.Orange);
				}

				//P2P transfers, both the sender and the reciever get notified.
			} else if ((e.TransferOptions & Journal.BankAccountTransferOptions.IsPlayerToPlayerTransfer) == Journal.BankAccountTransferOptions.IsPlayerToPlayerTransfer) {
				if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToReceiver) == Journal.BankAccountTransferOptions.AnnounceToReceiver && e.ReceiverAccount != null && e.ReceiverAccount.Owner != null) {
					e.ReceiverAccount.Owner.TSPlayer.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(16, "You {3} {0} from {1}. Transaction # {2}"), e.Amount.ToLongString(), 
						e.SenderAccount.Owner != null ? e.SenderAccount.Owner.TSPlayer.Name : SEconomyPlugin.Locale.StringOrDefault(17, "The server"), e.TransactionID, 
						e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(18, "received") : SEconomyPlugin.Locale.StringOrDefault(19, "sent")), Color.Orange);
				}
				if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToSender) == Journal.BankAccountTransferOptions.AnnounceToSender && e.SenderAccount.Owner != null) {
					e.SenderAccount.Owner.TSPlayer.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(16, "You {3} {0} from {1}. Transaction # {2}"), e.Amount.ToLongString(), e.ReceiverAccount.Owner.TSPlayer.Name, e.TransactionID,
						e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(19, "sent") : SEconomyPlugin.Locale.StringOrDefault(18, "received")), Color.Orange);
				}

				//Everything else, including world to player, and player to world.
			} else {
				if ((e.TransferOptions & Journal.BankAccountTransferOptions.IsPayment) == Journal.BankAccountTransferOptions.IsPayment) {
					if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToSender) == Journal.BankAccountTransferOptions.AnnounceToSender && e.SenderAccount.Owner != null) {
						e.SenderAccount.Owner.TSPlayer.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(20, "You {0} {1}{2}"), 
							e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(21, "paid") : SEconomyPlugin.Locale.StringOrDefault(22, "got paid"), e.Amount.ToLongString(),
							!string.IsNullOrEmpty(e.TransactionMessage) ? SEconomyPlugin.Locale.StringOrDefault(25, " for ") + e.TransactionMessage : ""), Color.Orange);
					}

					if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToReceiver) == Journal.BankAccountTransferOptions.AnnounceToReceiver && e.ReceiverAccount.Owner != null) {
						e.ReceiverAccount.Owner.TSPlayer.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(20, "You {0} {1}{2}"), e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(22, "got paid") 
							: SEconomyPlugin.Locale.StringOrDefault(22, "paid"), e.Amount.ToLongString(),
							!string.IsNullOrEmpty(e.TransactionMessage) ? SEconomyPlugin.Locale.StringOrDefault(25, " for ") + e.TransactionMessage : ""), Color.Orange);
					}
				} else {
					if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToSender) == Journal.BankAccountTransferOptions.AnnounceToSender && e.SenderAccount.Owner != null) {
						e.SenderAccount.Owner.TSPlayer.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(20, "You {0} {1}{2}"), e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(23, "lost") 
							: SEconomyPlugin.Locale.StringOrDefault(24, "gained"), e.Amount.ToLongString(), 
							!string.IsNullOrEmpty(e.TransactionMessage) ? SEconomyPlugin.Locale.StringOrDefault(25, " for ") + e.TransactionMessage : ""), Color.Orange);
					}

					if ((e.TransferOptions & Journal.BankAccountTransferOptions.AnnounceToReceiver) == Journal.BankAccountTransferOptions.AnnounceToReceiver && e.ReceiverAccount.Owner != null) {
						e.ReceiverAccount.Owner.TSPlayer.SendMessage(string.Format(SEconomyPlugin.Locale.StringOrDefault(20, "You {0} {1}{2}"), e.Amount > 0 ? SEconomyPlugin.Locale.StringOrDefault(24, "gained")
							: SEconomyPlugin.Locale.StringOrDefault(23, "lost"), e.Amount.ToLongString(), 
							!string.IsNullOrEmpty(e.TransactionMessage) ? SEconomyPlugin.Locale.StringOrDefault(25, " for ") + e.TransactionMessage : ""), Color.Orange);
					}
				}
			}
		}

		/// <summary>
		/// Occurs when the engine receives a packet from the client
		/// </summary>
		protected void NetHooks_GetData(GetDataEventArgs args)
		{
			byte index = default(byte);
			PlayerControlFlags playerState = default(PlayerControlFlags);
			Economy.EconomyPlayer player = null;

			if (args.MsgID != PacketTypes.PlayerUpdate 
				|| (index = args.Msg.readBuffer[args.Index]) < 0
				|| (player = Parent.GetEconomyPlayerSafe(index)) == null) {
				return;
			}

			if ((playerState = (PlayerControlFlags)args.Msg.readBuffer[args.Index + 1]) != PlayerControlFlags.Idle) {
				player.IdleSince = DateTime.Now;
			}

			player.LastKnownState = playerState;
		}

		/// <summary>
		/// Occurs when someone leaves the game.
		/// </summary>
		protected void ServerHooks_Leave(LeaveEventArgs args)
		{
			Economy.EconomyPlayer ePlayer = Parent.GetEconomyPlayerSafe(args.Who);
			Parent.EconomyPlayers.Remove(ePlayer);
		}

		/// <summary>
		/// Occurs when someone joins the game.
		/// </summary>
		protected void ServerHooks_Join(JoinEventArgs args)
		{
			Parent.EconomyPlayers.Add(new Economy.EconomyPlayer(args.Who));
		}

		protected async void GameHooks_PostInitialize(EventArgs e)
		{
			await Parent.BindToWorldAsync();
		}

		protected async void PlayerHooks_PlayerPostLogin(TShockAPI.Hooks.PlayerPostLoginEventArgs e)
		{
			Economy.EconomyPlayer ePlayer = null;
			if ((ePlayer = Parent.GetEconomyPlayerSafe(e.Player.Index)) == null) {
				return;
			}

			await ePlayer.EnsureBankAccountExistsAsync();
			await Task.Delay(2500);
			if (e.Player.IsLoggedIn == false || ePlayer.BankAccount == null) {
				return;
			}

			e.Player.SendInfoMessage(SEconomyPlugin.Locale.StringOrDefault(26, "You have {0}."), ePlayer.BankAccount.Balance.ToLongString());
		}

		/// <summary>
		/// Occurs when the pay run timer is elapsed, and gives the IntervalPayAmount to each player
		/// that has not been idle since the configured idle threshold.  Idle players are not paid.
		/// </summary>
		protected async void PayRunTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Money payAmount = 0;

			if (Parent.Configuration.PayIntervalMinutes <= 0 || string.IsNullOrEmpty(Parent.Configuration.IntervalPayAmount) == true
				|| Money.TryParse(Parent.Configuration.IntervalPayAmount, out payAmount) == false || payAmount <= 0) {
					return;
			}

			foreach (Economy.EconomyPlayer ep in Parent.EconomyPlayers) {
				if (ep.TimeSinceIdle.TotalMinutes > Parent.Configuration.IdleThresholdMinutes
					|| ep.BankAccount == null) {
						continue;
				}

				await Parent.WorldAccount.TransferToAsync(ep.BankAccount, payAmount, Journal.BankAccountTransferOptions.AnnounceToReceiver, "Auto pay", "Auto Pay");
			}
		}

		/// <summary>
		/// Occurs when an exception happens on a task thread.
		/// </summary>
		protected void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (e.Observed == true) {
				return;
			}

			TShockAPI.Log.ConsoleError(SEconomyPlugin.Locale.StringOrDefault(27, "seconomy async: error occurred on a task thread: ") + e.Exception.Flatten().ToString());
			e.SetObserved();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing == true) {
				TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= PlayerHooks_PlayerPostLogin;
				Parent.RunningJournal.BankTransferCompleted -= BankAccount_BankTransferCompleted;
				TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

				ServerApi.Hooks.GamePostInitialize.Deregister(this.Parent.PluginInstance, GameHooks_PostInitialize);
				ServerApi.Hooks.ServerJoin.Deregister(this.Parent.PluginInstance, ServerHooks_Join);
				ServerApi.Hooks.ServerLeave.Deregister(this.Parent.PluginInstance, ServerHooks_Leave);
				ServerApi.Hooks.NetGetData.Deregister(this.Parent.PluginInstance, NetHooks_GetData);
			}
		}
	}
}
