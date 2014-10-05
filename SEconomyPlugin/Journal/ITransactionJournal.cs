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
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Journal {
	public interface ITransactionJournal : IDisposable {

		#region "Transaction Journal Events"

		event EventHandler<PendingTransactionEventArgs> BankTransactionPending;

		#endregion

		SEconomy SEconomyInstance { get; set; }

		List<IBankAccount> BankAccounts { get; }

		IEnumerable<ITransaction> Transactions { get; }

		IBankAccount AddBankAccount(string UserAccountName, long WorldID, BankAccountFlags Flags, string iDonoLol);

		IBankAccount AddBankAccount(IBankAccount Account);

		IBankAccount GetBankAccountByName(string UserAccountName);

		IBankAccount GetBankAccount(long BankAccountK);

		IEnumerable<IBankAccount> GetBankAccountList(long BankAccountK);

		Task DeleteBankAccountAsync(long BankAccountK);

		event EventHandler<BankTransferEventArgs> BankTransferCompleted;

		bool JournalSaving { get; set; }

		bool BackupsEnabled { get; set; }

		void SaveJournal();

		Task SaveJournalAsync();

		bool LoadJournal();

		Task<bool> LoadJournalAsync();

		void BackupJournal();

		Task BackupJournalAsync();

		Task SquashJournalAsync();

		BankTransferEventArgs TransferBetween(IBankAccount FromAccount, IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage);

		Task<BankTransferEventArgs> TransferBetweenAsync(IBankAccount FromAccount, IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage);

		IBankAccount GetWorldAccount();

		void DumpSummary();
		
		void CleanJournal(PurgeOptions options);
	}
}
