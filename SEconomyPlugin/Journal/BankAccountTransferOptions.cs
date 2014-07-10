using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy.Journal {

    /// <summary>
    /// A list of options to consider when making a bank transfer.
    /// </summary>
    [Flags]
    public enum BankAccountTransferOptions {
        /// <summary>
        /// None, indicates a silent, normal payment.
        /// </summary>
        None = 0,

        /// <summary>
        /// Announces the payment to the reciever that they recieved, or gained money
        /// </summary>
        AnnounceToReceiver = 1,

        /// <summary>
        /// Announces the payment to the sender that they sent, or paid money
        /// </summary>
        AnnounceToSender = 1 << 1,

        /// <summary>
        /// Overrides the normal deficit logic, and will allow a normal player account to go into 
        /// </summary>
        AllowDeficitOnNormalAccount = 1 << 2,

        /// <summary>
        /// Indicates that the transfer happened because of PvP.
        /// </summary>
        PvP = 1 << 3,

        /// <summary>
        /// Indicates that the money was taken from the player because they died.
        /// </summary>
        MoneyTakenOnDeath = 1 << 4,

        /// <summary>
        /// Indicates that this transfer is a player-to-player transfer.
        /// 
        /// Note that PVP penalties ARE a player to player transfer but are forcefully taken; this is NOT set for these type of transfers, set MoneyFromPvP instead.
        /// </summary>
        IsPlayerToPlayerTransfer = 1 << 5,

        /// <summary>
        /// Indicates that this transaction was a payment for something tangible.
        /// </summary>
        IsPayment = 1 << 6,

        /// <summary>
        /// Suppresses the default announce messages.  Used for modules that have their own announcements for their own transfers.
        /// 
        /// Handle BankAccount.BankTransferSucceeded to hook your own.
        /// </summary>
        SuppressDefaultAnnounceMessages = 1 << 7
    }
}
