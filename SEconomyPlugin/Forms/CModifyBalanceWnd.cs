using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wolfje.Plugins.SEconomy;
using Wolfje.Plugins.SEconomy.Journal;

namespace Wolfje.Plugins.SEconomy.Forms {
	public partial class CModifyBalanceWnd : Form {
		IBankAccount account;
		SEconomy parent;

		enum MoneyParseState {
			Indeterminate,
			Invalid,
			Valid
		}

		public CModifyBalanceWnd(SEconomy parent, IBankAccount BankAccount)
		{
			this.account = BankAccount;
			this.parent = parent;

			InitializeComponent();

			rbTradeWith.CheckedChanged += (sender, args) => SwitchRegions();
			rbSetTo.CheckedChanged += (sender, args) => SwitchRegions();

			txtSetCurrency.TextChanged += (sender, args) => ParseMoneyFromControl(sender as TextBox);
			txtTradeCurrency.TextChanged += (sender, args) => ParseMoneyFromControl(sender as TextBox);

			txtSetCurrency.LostFocus += (sender, args) => ConvertMoneyToNumber(sender as TextBox);
			txtTradeCurrency.LostFocus += (sender, args) => ConvertMoneyToNumber(sender as TextBox);
			txtSetCurrency.Focus();

			ddlTradeAccount.DataSource = parent.RunningJournal.BankAccounts.OrderBy(i => i.UserAccountName).ToList();
			ddlTradeAccount.DisplayMember = "UserAccountName";
			ddlTradeAccount.ValueMember = "BankAccountK";

			this.Text = "Modify Balance - " + account.UserAccountName;

			lblCurrencyName.Text = Money.CurrencyName;
			lblCurrencyName2.Text = Money.CurrencyName;
		}

		void ConvertMoneyToNumber(TextBox Control)
		{
			if (ParseMoneyFromControl(Control) == MoneyParseState.Valid) {
				Money money;
				if (Money.TryParse(Control.Text, out money)) {
					Control.Text = money.Value.ToString();
				}
			}
		}

		void SwitchRegions()
		{
			txtSetCurrency.Enabled = !rbTradeWith.Checked;
			ddlTradeAccount.Enabled = rbTradeWith.Checked;
			txtTradeCurrency.Enabled = rbTradeWith.Checked;

			if (rbTradeWith.Checked) {
				ddlTradeAccount.Focus();
			} else if (rbSetTo.Checked) {
				txtSetCurrency.Focus();
			}
		}

		void SetMoneyInputState(MoneyParseState state, TextBox Control)
		{
			switch (state) {
			case MoneyParseState.Indeterminate:
				{
					Control.ForeColor = System.Drawing.Color.Black;
					// Control.BackColor = System.Drawing.Color.White;
				}
				break;
			case MoneyParseState.Invalid:
				{
					Control.ForeColor = System.Drawing.Color.Red;
					// Control.BackColor = System.Drawing.Color.FromArgb(255, 240, 240);
				}
				break;
			case MoneyParseState.Valid:
				{
					Control.ForeColor = System.Drawing.Color.Green;
					//    Control.BackColor = System.Drawing.Color.FromArgb(223, 255, 240);
				}
				break;
			}
		}

		MoneyParseState ParseMoneyFromControl(TextBox Control)
		{
			MoneyParseState state = MoneyValid(Control);
			SetMoneyInputState(state, Control);
			return state;
		}

		MoneyParseState MoneyValid(TextBox input)
		{
			Money money;

			if (string.IsNullOrWhiteSpace(input.Text)) {
				return MoneyParseState.Indeterminate;
			} else if (Money.TryParse(input.Text, out money) && money.Value >= 0) {
				return MoneyParseState.Valid;
			} else {
				return MoneyParseState.Invalid;
			}
		}

		async Task SetBalanceToAsync(Money money)
		{
			await account.SyncBalanceAsync();
			BankTransferEventArgs args = await account.OwningJournal.TransferBetweenAsync(parent.WorldAccount, account, money - account.Balance, BankAccountTransferOptions.AnnounceToReceiver | BankAccountTransferOptions.AnnounceToSender, "Balance Adjustment", "Balance Adjustment from bank manager.");

			if (args.TransferSucceeded == true) {
				this.MainThreadInvoke(() => this.Hide());
				return;
			}

			MessageBox.Show("Transfer failed.", "Transfer failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private async void CModifyBalanceWnd_Load(object sender, EventArgs e)
		{
			if (account == null) {
				return;
			}

			await account.SyncBalanceAsync();
			this.MainThreadInvoke(() => {
				lblBalance.Text = account.Balance.ToLongString(true);
				lblAccountName.Text = account.UserAccountName;
			});

			txtSetCurrency.Select();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Hide();
		}

		private async void btnModify_Click(object sender, EventArgs e)
		{
			Money money;
			if (rbSetTo.Checked == false || Money.TryParse(txtSetCurrency.Text, out money) == false) {
				return;
			}

			if (MessageBox.Show(string.Format("Really set {0}'s balance to {1}?", account.UserAccountName, money.ToLongString()),
				    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes) {
				await SetBalanceToAsync(money);
			} else {
				this.Hide();
			}
		}
	}
}
