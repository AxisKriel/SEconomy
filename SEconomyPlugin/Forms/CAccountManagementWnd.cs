using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Xml.Linq;
using System.Xml.XPath;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Forms {
    public partial class CAccountManagementWnd : Form {

        List<AccountSummary> sourceAccList = new List<AccountSummary>();
        BindingList<AccountSummary> accList = new BindingList<AccountSummary>();
        IEnumerable<Journal.ITransaction> tranList;


        AccountSummary currentlySelectedAccount = null;

        public CAccountManagementWnd() {
            InitializeComponent();

        }

        void LoadTransactionsForUser(long BankAccountK) {
            Journal.IBankAccount selectedAccount = SEconomyPlugin.RunningJournal.GetBankAccount(BankAccountK);

            var qTransactions = selectedAccount.Transactions;

            gvTransactions.DataSource = null;
            gvTransactions.DataSource = qTransactions;

            tranList = SEconomyPlugin.RunningJournal.Transactions;

            lblStatus.Text = string.Format("Loaded {0} transactions for {1}.", qTransactions.Count(), selectedAccount.UserAccountName);


            if (selectedAccount.Owner != null) {
                lblOnline.Text = "Online";
                lblOnline.ForeColor = System.Drawing.Color.DarkGreen;
            } else {
                lblOnline.Text = "Offline";
                lblOnline.ForeColor = System.Drawing.Color.Red;
            }

            selectedAccount.SyncBalanceAsync().ContinueWith((task) => {
                AccountSummary summary = accList.FirstOrDefault(i => i.Value == BankAccountK);

                if (summary != null) {
                    summary.Balance = selectedAccount.Balance;
                }

                lblBalance.Text = selectedAccount.Balance.ToLongString(true);
                lblName.Text = string.Format("{0}, acccount ID {1}", selectedAccount.UserAccountName, selectedAccount.BankAccountK);
            });

           
        }

        Task LoadAccountsAsync() {
            return Task.Factory.StartNew(() => {

                this.MainThreadInvoke(() => {
                    gvTransactions.Enabled = false;
                    gvAccounts.Enabled = false;
                    btnRefreshTrans.Enabled = false;
                    btnResetTrans.Enabled = false;

                    lblStatus.Text = "Syncing accounts";
                });

                int i = 0;
                foreach (Journal.IBankAccount account in SEconomyPlugin.RunningJournal.BankAccounts) {
                    account.SyncBalance();
                    sourceAccList.Add(new AccountSummary() { Name = account.UserAccountName, Balance = account.Balance, Value = account.BankAccountK });

                    int p = Convert.ToInt32(((double)i / (double)SEconomyPlugin.RunningJournal.BankAccounts.Count) * 100);

                    this.MainThreadInvoke(() => {
                        toolStripProgressBar1.Value = p;
                    });
                    
                    i++;
                }

                this.MainThreadInvoke(() => {
                    accList = new BindingList<AccountSummary>(sourceAccList.OrderByDescending(x => (long)x.Balance).ToList());
                    gvAccounts.DataSource = accList;

                    gvTransactions.Enabled = true;
                    gvAccounts.Enabled = true;
                    btnRefreshTrans.Enabled = true;
                    btnResetTrans.Enabled = true;

                    lblStatus.Text = "Done.";
                });
            });

        }

        private void CAccountManagementWnd_Load(object sender, EventArgs e) {
            gvTransactions.AutoGenerateColumns = false;
            gvAccounts.AutoGenerateColumns = false;

            LoadAccountsAsync().ContinueWith((task) => {
                this.MainThreadInvoke(() => {
                    toolStripProgressBar1.Value = 100;
                    lblStatus.Text = "Done.";
                });
            });   
        }


        /// <summary>
        /// Occurs when the user selects an item on the left hand menu
        /// </summary>
        private void lbAccounts_SelectedIndexChanged(object sender, EventArgs e) {
            AccountSummary selectedSummary = gvAccounts.SelectedRows[0].DataBoundItem as AccountSummary;
            if (selectedSummary != null) {
                LoadTransactionsForUser(selectedSummary.Value);
            } else {
                MessageBox.Show("Could not load bank account " + selectedSummary.DisplayValue, "Error");
            }
        }

        private void gvTransactions_CellEnter(object sender, DataGridViewCellEventArgs e) {
            var cell = gvTransactions.CurrentCell;
            var cellRect = gvTransactions.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);

            if (cell.OwningColumn.HeaderText.Equals("amount", StringComparison.CurrentCultureIgnoreCase)) {
                Money amount = 0;
                if ( Money.TryParse((string)cell.Value.ToString(), out amount)) {
                    toolTip1.Show(amount.ToLongString(true), gvTransactions, cellRect.X + cell.Size.Width / 2, cellRect.Y + cell.Size.Height / 2, 10000);
                }
            }

            gvTransactions.ShowCellToolTips = false;
        }


        /// <summary>
        /// Shows the transaction context menu under the mouse cursor
        /// </summary>
        private void gvTransactions_MouseUp(object sender, MouseEventArgs e) {
            // Load context menu on right mouse click
            DataGridView.HitTestInfo hitTestInfo;

            if (e.Button == MouseButtons.Right) {
                hitTestInfo = gvTransactions.HitTest(e.X, e.Y);
                // If clicked on a cell
                if (hitTestInfo.Type == DataGridViewHitTestType.Cell) {
                    ctxDeleteTransactionItem.Text = string.Format("Delete {0} transactions", gvTransactions.SelectedRows.Count);
                    ctxTransaction.Show(gvTransactions, e.X, e.Y);
                }
               
            }
        }

        /// <summary>
        /// Fires when a user wants to delete some transactions
        /// </summary>
        private void ctxDeleteTransactionConfirm_Click(object sender, EventArgs e) {
            gvTransactions.Enabled = false;
            gvAccounts.Enabled = false;
            btnRefreshTrans.Enabled = false;
            btnResetTrans.Enabled = false;

            Task.Factory.StartNew(() => {
                int count = gvTransactions.SelectedRows.Count;

                for (int i = 0; i < gvTransactions.SelectedRows.Count; i++) {
                    DataGridViewRow selectedItem = gvTransactions.SelectedRows[i];

                    if (selectedItem.DataBoundItem != null) {
                        Journal.IBankAccount bankAccount;
                        Journal.ITransaction selectedTrans = selectedItem.DataBoundItem as Journal.ITransaction;


                        if (selectedTrans != null) {
                            bankAccount = selectedTrans.BankAccount;

                            //invoke GUI updates on gui thread
                            this.MainThreadInvoke(() => {
                                lblStatus.Text = string.Format("Deleted transaction {0} for {1}", selectedTrans.BankAccountTransactionK, ((Money)selectedTrans.Amount).ToLongString(true));
                                toolStripProgressBar1.Value = Convert.ToInt32(((double)i / (double)count) * 100);
                            });

                            bankAccount.Transactions.Remove(selectedTrans);
                        }

                    }

                }
            }).ContinueWith((task) => {
                this.MainThreadInvoke(() => {
                    AccountSummary summary = gvAccounts.SelectedRows[0].DataBoundItem as AccountSummary;
                    Journal.IBankAccount selectedUserAccount = SEconomyPlugin.RunningJournal.GetBankAccount(summary.Value);

                    if (selectedUserAccount != null) {

                        selectedUserAccount.SyncBalanceAsync().ContinueWith((t) => {
                            AccountSummary selectedSummary = accList.FirstOrDefault(i => i.Value == selectedUserAccount.BankAccountK);

                            if (selectedSummary != null) {
                                selectedSummary.Balance = selectedUserAccount.Balance;
                            }
                        });

                    }

                    toolStripProgressBar1.Value = 100;

                    gvTransactions.Enabled = true;
                    gvAccounts.Enabled = true;
                    btnRefreshTrans.Enabled = true;
                    btnResetTrans.Enabled = true;
                    btnShowFrom.Enabled = true;

                    LoadTransactionsForUser(summary.Value);

                });
            });
        }

        /// <summary>
        /// Occurs when a user clicks on the refresh transactions button on the right hand top toolbar
        /// </summary>
        private void btnRefreshTrans_Click(object sender, EventArgs e) {
            if (gvAccounts.SelectedRows.Count > 0) {
                AccountSummary summary = gvAccounts.SelectedRows[0].DataBoundItem as AccountSummary;
                LoadTransactionsForUser(summary.Value);
            }
           
        }

        private void btnResetTrans_Click(object sender, EventArgs e) {

            if (MessageBox.Show("This will delete all transactions for this user.  This could take a long time and SEconomy performance could be impacted.  The user's balance will be reset to 0 copper.", "Delete all Transactions", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes) {
                Journal.IBankAccount account = SEconomyPlugin.RunningJournal.GetBankAccount(currentlySelectedAccount.Value);

                gvTransactions.Enabled = false;
                gvAccounts.Enabled = false;
                btnRefreshTrans.Enabled = false;
                btnResetTrans.Enabled = false;
                btnShowFrom.Enabled = false;

                lblStatus.Text = "Resetting all transactions for " + currentlySelectedAccount.Name;
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Style = ProgressBarStyle.Marquee;


                Task.Factory.StartNew(() => {

                    account.Transactions.Clear();

                }).ContinueWith((task) => {
                    Journal.IBankAccount selectedUserAccount = SEconomyPlugin.RunningJournal.GetBankAccount(currentlySelectedAccount.Value);

                    if (selectedUserAccount != null) {

                        selectedUserAccount.SyncBalanceAsync().ContinueWith((t) => {
                            AccountSummary summary = accList.FirstOrDefault(i => i.Value == selectedUserAccount.BankAccountK);

                            if (summary != null) {
                                summary.Balance = selectedUserAccount.Balance;
                            }

                            this.MainThreadInvoke(() => {
                                toolStripProgressBar1.Value = 100;
                                toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
                                gvTransactions.DataSource = null;
                                gvTransactions.Enabled = true;
                                gvAccounts.Enabled = true;
                                btnRefreshTrans.Enabled = true;
                                btnResetTrans.Enabled = true;

                                //force the bindinglist to raise the reset event updating the right hand side
                                accList.ResetBindings();

                                LoadTransactionsForUser(currentlySelectedAccount.Value);
                            });
                        });
                    }
                });
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Hide();
        }

        private void gvAccounts_SelectionChanged(object sender, EventArgs e) {
            if (gvAccounts.SelectedRows.Count > 0) {
                this.currentlySelectedAccount = gvAccounts.SelectedRows[0].DataBoundItem as AccountSummary;

                LoadTransactionsForUser(this.currentlySelectedAccount.Value);
            } else {
                this.currentlySelectedAccount = null;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) {
            if (txtSearch.Text.Length > 0) {
                accList = new BindingList<AccountSummary>(sourceAccList.Where(i => i.Name.ContainsInsensitive(txtSearch.Text)).OrderByDescending(x => (long)x.Balance).ToList());
            } else {
                accList = new BindingList<AccountSummary>(sourceAccList.OrderByDescending(x => (long)x.Balance).ToList());
            }
            
            gvAccounts.DataSource = accList;
        }

        private void btnShowFrom_Click(object sender, EventArgs e) {
            var vivibleRowsCount = gvTransactions.DisplayedRowCount(true);
            var firstDisplayedRowIndex = gvTransactions.FirstDisplayedCell.RowIndex;
            var lastvibileRowIndex = (firstDisplayedRowIndex + vivibleRowsCount) - 1;

            for (int rowIndex = firstDisplayedRowIndex; rowIndex <= lastvibileRowIndex; rowIndex++) {
                var row = gvTransactions.Rows[rowIndex];
                var cell = row.Cells.OfType<DataGridViewLinkCell>().FirstOrDefault();

                if (string.IsNullOrEmpty(cell.Value as string)) {

                    Task.Factory.StartNew(() => {
                        Journal.ITransaction trans = row.DataBoundItem as Journal.ITransaction;
                        Journal.ITransaction oppositeTrans = tranList.FirstOrDefault(i => i.BankAccountTransactionK == trans.BankAccountTransactionFK);

                        if (oppositeTrans != null) {
                            AccountSummary oppositeAccount = accList.FirstOrDefault(i => i.Value == oppositeTrans.BankAccountFK);
                            if (oppositeAccount != null) {
                                this.MainThreadInvoke(() => {
                                    cell.Value = oppositeAccount.Name;
                                });
                            }
                        } else {
                            this.MainThreadInvoke(() => {
                                cell.LinkBehavior = LinkBehavior.NeverUnderline;
                                cell.LinkColor = System.Drawing.Color.Gray;

                                cell.Value = "{Unknown}";
                            });
                        }
                    });


                }
            }
        }

        private void btnModifyBalance_Click(object sender, EventArgs e) {
            Journal.IBankAccount account = SEconomyPlugin.RunningJournal.GetBankAccount(currentlySelectedAccount.Value);
            CModifyBalanceWnd modifyBalanceWindow = new CModifyBalanceWnd(account);

            modifyBalanceWindow.ShowDialog(this);

            //when the dialog has finished, reload the account 
            LoadTransactionsForUser(account.BankAccountK);
        }

        private void gvTransactions_DataError(object sender, DataGridViewDataErrorEventArgs e) {
            e.ThrowException = false;
        }


    }

    class AccountSummary {
        public string Name { get; set; }
        public long Value { get; set; }
        public Money Balance { get; set; }
        public string DisplayValue {
            get {
                return this.ToString();
            }
        }
        public override string ToString() {
            return string.Format("{0} - {1} [{2}]", this.Name, this.Value, this.Balance);
        }
    }
}
