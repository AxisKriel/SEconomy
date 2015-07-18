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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Extensions;
using Wolfje.Plugins.SEconomy.Extensions;
using Wolfje.Plugins.SEconomy.Journal.MySQLHistory;


namespace Wolfje.Plugins.SEconomy.Journal.MySQLJournal {
	public class BankAccount : IBankAccount {
		protected MySQLHistoryJournal journal;

		public BankAccount(MySQLHistoryJournal journal)
		{
			this.journal = journal;
		}

		#region IBankAccount Members

		public ITransactionJournal OwningJournal {
			get { return journal; }
		}

		public long BankAccountK { get; set; }

		public string OldBankAccountK { get; set; }

		public string UserAccountName { get; set; }

		public long WorldID { get; set; }

		public BankAccountFlags Flags { get; set; }

		public string Description { get; set; }

		public Money Balance { get; set; }

		public bool IsAccountEnabled {
			get { return false; }
		}

		public bool IsSystemAccount {
			get { return false; }
		}

		public bool IsLockedToWorld {
			get { return false; }
		}

		public bool IsPluginAccount {
			get { return false; }
		}

		public List<ITransaction> Transactions {
			get {
				List<ITransaction> tranList = new List<ITransaction>();

				using (QueryResult reader = journal.Connection.QueryReader("SELECT * FROM `bank_account_transaction` WHERE `bank_account_fk` = " + this.BankAccountK + ";")) {
					foreach (IDataReader item in reader.AsEnumerable()) {
						MySQLTransaction bankTrans = new MySQLTransaction((IBankAccount)this) {
							BankAccountFK = item.Get<long>("bank_account_fk"),
							BankAccountTransactionK = item.Get<long>("bank_account_transaction_id"),
							BankAccountTransactionFK = item.Get<long?>("bank_account_transaction_fk").GetValueOrDefault(-1L),
							Flags = (BankAccountTransactionFlags)item.Get<int>("flags"),
							TransactionDateUtc = item.GetDateTime(reader.Reader.GetOrdinal("transaction_date_utc")),
							Amount = item.Get<long>("amount"),
							Message = item.Get<string>("message")
						};

						tranList.Add(bankTrans);
					}
				}

				return tranList;
			}
		}

		public ITransaction AddTransaction(ITransaction Transaction)
		{
			throw new NotSupportedException("AddTransaction via interface is not supported for SQL journals.  To transfer money between accounts, use the TransferTo methods instead.");
		}

		public void ResetAccountTransactions(long BankAccountK)
		{
			try {
				journal.Connection.Query("DELETE FROM `bank_account_transaction` WHERE `bank_account_fk` = " + this.BankAccountK + ";");
                this.Balance = 0;
			} catch {
				TShock.Log.ConsoleError(" seconomy mysql: MySQL command error in ResetAccountTransactions");
			}
		}

		public async Task ResetAccountTransactionsAsync(long BankAccountK)
		{
			await Task.Run(() => ResetAccountTransactions(BankAccountK));
		}

		public async Task SyncBalanceAsync()
		{
			await Task.Run(() => SyncBalance());
		}

		public void SyncBalance(IDbConnection conn)
		{
			try {
				this.Balance = Convert.ToInt64(journal.Connection.QueryScalarExisting<decimal>("SELECT IFNULL(SUM(Amount), 0) FROM `bank_account_transaction` WHERE `bank_account_transaction`.`bank_account_fk` = " + this.BankAccountK + ";"));
			} catch (Exception ex) {
				TShock.Log.ConsoleError(" seconomy mysql: SQL error in SyncBalance: " + ex.Message);
			}
		}

		public async Task SyncBalanceAsync(IDbConnection conn)
		{
			await Task.Run(() => SyncBalance(conn));
		}

		public void SyncBalance()
		{
			try {
				this.Balance = Convert.ToInt64(journal.Connection.QueryScalar<decimal>("SELECT IFNULL(SUM(Amount), 0) FROM `bank_account_transaction` WHERE `bank_account_transaction`.`bank_account_fk` = " + this.BankAccountK + ";"));
			} catch (Exception ex) {
				TShock.Log.ConsoleError(" seconomy mysql: SQL error in SyncBalance: " + ex.Message);
			}
		}

		public BankTransferEventArgs TransferTo(IBankAccount Account, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			return journal.TransferBetween(this, Account, Amount, Options, TransactionMessage, JournalMessage);
		}

		public async Task<BankTransferEventArgs> TransferToAsync(int Index, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			IBankAccount account;

			if (SEconomyPlugin.Instance == null
			    || (account = SEconomyPlugin.Instance.GetBankAccount(Index)) == null) {
				return null;
			}

			return await Task.Factory.StartNew(() => TransferTo(account, Amount, Options, TransactionMessage, JournalMessage));
		}

		public async Task<BankTransferEventArgs> TransferToAsync(IBankAccount ToAccount, Money Amount, BankAccountTransferOptions Options, string TransactionMessage, string JournalMessage)
		{
			return await Task.Factory.StartNew(() => TransferTo(ToAccount, Amount, Options, TransactionMessage, JournalMessage));
		}

		#endregion

		public override string ToString()
		{
			return string.Format("MySQLBankAccount {0} UserAccountName={1} Balance={2}", this.BankAccountK, this.UserAccountName, this.BankAccountK);
		}
	}
}
