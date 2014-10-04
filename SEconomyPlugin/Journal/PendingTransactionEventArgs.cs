/*
 * This file is part of SEconomy - A server-sided currency implementation
 * Copyright (C) 2013-2014, Tyler Watson <tyler@tw.id.au>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy.Journal {
	public class PendingTransactionEventArgs : EventArgs {
		IBankAccount fromAccount;
		IBankAccount toAccount;

		public PendingTransactionEventArgs(IBankAccount From, IBankAccount To, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string LogMessage) : base()
		{
			this.fromAccount = From;
			this.toAccount = To;
			this.Amount = Amount;
			this.Options = Options;
			this.TransactionMessage = TransactionMessage;
			this.JournalLogMessage = LogMessage;
			this.IsCancelled = false;
		}

		/// <summary>
		/// Specifies the source bank account the amount is to be transferred from
		/// </summary>
		public IBankAccount FromAccount {
			get {
				return fromAccount;
			}
			set {
				if (value == null) {
					throw new ArgumentNullException("FromAaccount cannot be set to null.");
				}

				fromAccount = value;
			}
		}

		/// <summary>
		/// Specifies the destination account the amount is to be transferred to
		/// </summary>
		public IBankAccount ToAccount {
			get {
				return toAccount;
			}
			set {
				if (value == null) {
					throw new ArgumentNullException("ToAccount cannot be set to null.");
				}

				toAccount = value;
			}
		}

		public Money Amount { get; set; }

		public BankAccountTransferOptions Options { get; private set; }

		/// <summary>
		/// Specifies the transaction message 
		/// </summary>
		public string TransactionMessage { get; set; }

		/// <summary>
		/// Specifies the journal message
		/// </summary>
		public string JournalLogMessage { get; set; }

		/// <summary>
		/// Cancels the transaction
		/// </summary>
		public bool IsCancelled { get; set; }

	}
}
