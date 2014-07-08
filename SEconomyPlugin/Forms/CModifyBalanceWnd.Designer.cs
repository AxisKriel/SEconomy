namespace Wolfje.Plugins.SEconomy.Forms {
    partial class CModifyBalanceWnd {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblCurrencyName2 = new System.Windows.Forms.Label();
            this.txtTradeCurrency = new System.Windows.Forms.TextBox();
            this.lblCurrencyName = new System.Windows.Forms.Label();
            this.ddlTradeAccount = new System.Windows.Forms.ComboBox();
            this.rbTradeWith = new System.Windows.Forms.RadioButton();
            this.txtSetCurrency = new System.Windows.Forms.TextBox();
            this.rbSetTo = new System.Windows.Forms.RadioButton();
            this.btnModify = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblAccountName = new System.Windows.Forms.Label();
            this.lblBalance = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblCurrencyName2);
            this.groupBox1.Controls.Add(this.txtTradeCurrency);
            this.groupBox1.Controls.Add(this.lblCurrencyName);
            this.groupBox1.Controls.Add(this.ddlTradeAccount);
            this.groupBox1.Controls.Add(this.rbTradeWith);
            this.groupBox1.Controls.Add(this.txtSetCurrency);
            this.groupBox1.Controls.Add(this.rbSetTo);
            this.groupBox1.Location = new System.Drawing.Point(15, 39);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(578, 110);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Modify Balance";
            // 
            // lblCurrencyName2
            // 
            this.lblCurrencyName2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrencyName2.AutoSize = true;
            this.lblCurrencyName2.Location = new System.Drawing.Point(472, 67);
            this.lblCurrencyName2.Name = "lblCurrencyName2";
            this.lblCurrencyName2.Size = new System.Drawing.Size(82, 13);
            this.lblCurrencyName2.TabIndex = 7;
            this.lblCurrencyName2.Text = "currency names";
            // 
            // txtTradeCurrency
            // 
            this.txtTradeCurrency.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTradeCurrency.Enabled = false;
            this.txtTradeCurrency.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTradeCurrency.Location = new System.Drawing.Point(331, 63);
            this.txtTradeCurrency.Name = "txtTradeCurrency";
            this.txtTradeCurrency.Size = new System.Drawing.Size(135, 20);
            this.txtTradeCurrency.TabIndex = 6;
            // 
            // lblCurrencyName
            // 
            this.lblCurrencyName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrencyName.AutoSize = true;
            this.lblCurrencyName.Location = new System.Drawing.Point(472, 35);
            this.lblCurrencyName.Name = "lblCurrencyName";
            this.lblCurrencyName.Size = new System.Drawing.Size(82, 13);
            this.lblCurrencyName.TabIndex = 3;
            this.lblCurrencyName.Text = "currency names";
            // 
            // ddlTradeAccount
            // 
            this.ddlTradeAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddlTradeAccount.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.ddlTradeAccount.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.ddlTradeAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlTradeAccount.Enabled = false;
            this.ddlTradeAccount.FormattingEnabled = true;
            this.ddlTradeAccount.Location = new System.Drawing.Point(147, 62);
            this.ddlTradeAccount.Name = "ddlTradeAccount";
            this.ddlTradeAccount.Size = new System.Drawing.Size(178, 21);
            this.ddlTradeAccount.TabIndex = 5;
            // 
            // rbTradeWith
            // 
            this.rbTradeWith.AutoSize = true;
            this.rbTradeWith.Enabled = false;
            this.rbTradeWith.Location = new System.Drawing.Point(21, 63);
            this.rbTradeWith.Name = "rbTradeWith";
            this.rbTradeWith.Size = new System.Drawing.Size(101, 17);
            this.rbTradeWith.TabIndex = 4;
            this.rbTradeWith.TabStop = true;
            this.rbTradeWith.Text = "Force Pay Acct.";
            this.rbTradeWith.UseVisualStyleBackColor = true;
            // 
            // txtSetCurrency
            // 
            this.txtSetCurrency.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSetCurrency.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSetCurrency.Location = new System.Drawing.Point(147, 30);
            this.txtSetCurrency.Name = "txtSetCurrency";
            this.txtSetCurrency.Size = new System.Drawing.Size(319, 20);
            this.txtSetCurrency.TabIndex = 1;
            // 
            // rbSetTo
            // 
            this.rbSetTo.AutoSize = true;
            this.rbSetTo.Checked = true;
            this.rbSetTo.Location = new System.Drawing.Point(21, 31);
            this.rbSetTo.Name = "rbSetTo";
            this.rbSetTo.Size = new System.Drawing.Size(99, 17);
            this.rbSetTo.TabIndex = 0;
            this.rbSetTo.TabStop = true;
            this.rbSetTo.Text = "Set Balance To";
            this.rbSetTo.UseVisualStyleBackColor = true;
            // 
            // btnModify
            // 
            this.btnModify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnModify.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModify.Location = new System.Drawing.Point(518, 170);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(75, 23);
            this.btnModify.TabIndex = 1;
            this.btnModify.Text = "Modify";
            this.btnModify.UseVisualStyleBackColor = true;
            this.btnModify.Click += new System.EventHandler(this.btnModify_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(15, 170);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblAccountName
            // 
            this.lblAccountName.AutoSize = true;
            this.lblAccountName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAccountName.Location = new System.Drawing.Point(12, 14);
            this.lblAccountName.Name = "lblAccountName";
            this.lblAccountName.Size = new System.Drawing.Size(90, 13);
            this.lblAccountName.TabIndex = 0;
            this.lblAccountName.Text = "Account Name";
            // 
            // lblBalance
            // 
            this.lblBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBalance.Location = new System.Drawing.Point(253, 14);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(340, 13);
            this.lblBalance.TabIndex = 0;
            this.lblBalance.Text = "Balance";
            this.lblBalance.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // CModifyBalanceWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 205);
            this.Controls.Add(this.lblBalance);
            this.Controls.Add(this.lblAccountName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnModify);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CModifyBalanceWnd";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CModifyBalanceWnd";
            this.Load += new System.EventHandler(this.CModifyBalanceWnd_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnModify;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblAccountName;
        private System.Windows.Forms.Label lblBalance;
        private System.Windows.Forms.Label lblCurrencyName;
        private System.Windows.Forms.ComboBox ddlTradeAccount;
        private System.Windows.Forms.RadioButton rbTradeWith;
        private System.Windows.Forms.TextBox txtSetCurrency;
        private System.Windows.Forms.RadioButton rbSetTo;
        private System.Windows.Forms.Label lblCurrencyName2;
        private System.Windows.Forms.TextBox txtTradeCurrency;
    }
}