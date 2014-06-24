namespace Wolfje.Plugins.SEconomy.Forms {
    partial class CAccountManagementWnd {
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CAccountManagementWnd));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.lblSearch = new System.Windows.Forms.ToolStripLabel();
            this.txtSearch = new System.Windows.Forms.ToolStripTextBox();
            this.gvAccounts = new System.Windows.Forms.DataGridView();
            this.clmName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmBalance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.lblName = new System.Windows.Forms.ToolStripLabel();
            this.lblBalance = new System.Windows.Forms.ToolStripLabel();
            this.lblOnline = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRefreshTrans = new System.Windows.Forms.ToolStripButton();
            this.btnResetTrans = new System.Windows.Forms.ToolStripButton();
            this.btnShowFrom = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnModifyBalance = new System.Windows.Forms.ToolStripButton();
            this.gvTransactions = new System.Windows.Forms.DataGridView();
            this.clmId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmFrom = new System.Windows.Forms.DataGridViewLinkColumn();
            this.clmAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmFlags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ctxAccount = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxConfirmDeleteAccount = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ctxTransaction = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxDeleteTransactionItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxDeleteTransactionConfirm = new System.Windows.Forms.ToolStripMenuItem();
            this.cAccountManagementWndBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvAccounts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvTransactions)).BeginInit();
            this.ctxAccount.SuspendLayout();
            this.ctxTransaction.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cAccountManagementWndBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.lblStatus,
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 625);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1103, 24);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 18);
            this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(39, 19);
            this.lblStatus.Text = "Ready";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(831, 19);
            this.toolStripStatusLabel1.Spring = true;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.ForeColor = System.Drawing.Color.Gray;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(116, 19);
            this.toolStripStatusLabel2.Text = "SEconomy by Wolfje";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1103, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1103, 601);
            this.splitContainer1.SplitterDistance = 313;
            this.splitContainer1.TabIndex = 2;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.toolStrip2);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.gvAccounts);
            this.splitContainer3.Size = new System.Drawing.Size(313, 601);
            this.splitContainer3.SplitterDistance = 25;
            this.splitContainer3.TabIndex = 0;
            // 
            // toolStrip2
            // 
            this.toolStrip2.AutoSize = false;
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblSearch,
            this.txtSearch});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(313, 25);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // lblSearch
            // 
            this.lblSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblSearch.Image = ((System.Drawing.Image)(resources.GetObject("lblSearch.Image")));
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblSearch.Size = new System.Drawing.Size(68, 22);
            this.lblSearch.Text = "Search";
            // 
            // txtSearch
            // 
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(150, 25);
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // gvAccounts
            // 
            this.gvAccounts.AllowUserToAddRows = false;
            this.gvAccounts.AllowUserToDeleteRows = false;
            this.gvAccounts.AllowUserToResizeRows = false;
            this.gvAccounts.BackgroundColor = System.Drawing.Color.White;
            this.gvAccounts.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gvAccounts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvAccounts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clmName,
            this.clmBalance});
            this.gvAccounts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvAccounts.Location = new System.Drawing.Point(0, 0);
            this.gvAccounts.MultiSelect = false;
            this.gvAccounts.Name = "gvAccounts";
            this.gvAccounts.ReadOnly = true;
            this.gvAccounts.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.gvAccounts.RowHeadersVisible = false;
            this.gvAccounts.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gvAccounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gvAccounts.Size = new System.Drawing.Size(313, 572);
            this.gvAccounts.TabIndex = 0;
            this.gvAccounts.SelectionChanged += new System.EventHandler(this.gvAccounts_SelectionChanged);
            // 
            // clmName
            // 
            this.clmName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.clmName.DataPropertyName = "Name";
            this.clmName.HeaderText = "Name";
            this.clmName.Name = "clmName";
            this.clmName.ReadOnly = true;
            // 
            // clmBalance
            // 
            this.clmBalance.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.clmBalance.DataPropertyName = "Balance";
            this.clmBalance.HeaderText = "Balance";
            this.clmBalance.Name = "clmBalance";
            this.clmBalance.ReadOnly = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.toolStrip1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.gvTransactions);
            this.splitContainer2.Size = new System.Drawing.Size(786, 601);
            this.splitContainer2.SplitterDistance = 25;
            this.splitContainer2.SplitterWidth = 2;
            this.splitContainer2.TabIndex = 2;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblName,
            this.lblBalance,
            this.lblOnline,
            this.toolStripSeparator1,
            this.btnRefreshTrans,
            this.btnResetTrans,
            this.btnShowFrom,
            this.toolStripSeparator2,
            this.btnModifyBalance});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(786, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // lblName
            // 
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(45, 22);
            this.lblName.Text = "(name)";
            // 
            // lblBalance
            // 
            this.lblBalance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBalance.Margin = new System.Windows.Forms.Padding(15, 1, 0, 2);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(57, 22);
            this.lblBalance.Text = "(balance)";
            // 
            // lblOnline
            // 
            this.lblOnline.ForeColor = System.Drawing.Color.Red;
            this.lblOnline.Margin = new System.Windows.Forms.Padding(15, 1, 0, 2);
            this.lblOnline.Name = "lblOnline";
            this.lblOnline.Size = new System.Drawing.Size(43, 22);
            this.lblOnline.Text = "Offline";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnRefreshTrans
            // 
            this.btnRefreshTrans.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRefreshTrans.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshTrans.Image")));
            this.btnRefreshTrans.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefreshTrans.Name = "btnRefreshTrans";
            this.btnRefreshTrans.Size = new System.Drawing.Size(23, 22);
            this.btnRefreshTrans.ToolTipText = "Refresh Transactions";
            this.btnRefreshTrans.Click += new System.EventHandler(this.btnRefreshTrans_Click);
            // 
            // btnResetTrans
            // 
            this.btnResetTrans.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnResetTrans.Image = ((System.Drawing.Image)(resources.GetObject("btnResetTrans.Image")));
            this.btnResetTrans.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnResetTrans.Name = "btnResetTrans";
            this.btnResetTrans.Size = new System.Drawing.Size(23, 22);
            this.btnResetTrans.Text = "toolStripButton1";
            this.btnResetTrans.ToolTipText = "Delete all transactions for this user.  Resets balance to 0.";
            this.btnResetTrans.Click += new System.EventHandler(this.btnResetTrans_Click);
            // 
            // btnShowFrom
            // 
            this.btnShowFrom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowFrom.Image = ((System.Drawing.Image)(resources.GetObject("btnShowFrom.Image")));
            this.btnShowFrom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowFrom.Name = "btnShowFrom";
            this.btnShowFrom.Size = new System.Drawing.Size(23, 22);
            this.btnShowFrom.Text = "Show From";
            this.btnShowFrom.ToolTipText = "Show who the transactions in the currently visible area were from";
            this.btnShowFrom.Click += new System.EventHandler(this.btnShowFrom_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnModifyBalance
            // 
            this.btnModifyBalance.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnModifyBalance.Image = ((System.Drawing.Image)(resources.GetObject("btnModifyBalance.Image")));
            this.btnModifyBalance.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnModifyBalance.Name = "btnModifyBalance";
            this.btnModifyBalance.Size = new System.Drawing.Size(23, 22);
            this.btnModifyBalance.Text = "toolStripButton1";
            this.btnModifyBalance.ToolTipText = "Modifies someone\'s account balance.";
            this.btnModifyBalance.Click += new System.EventHandler(this.btnModifyBalance_Click);
            // 
            // gvTransactions
            // 
            this.gvTransactions.AllowUserToAddRows = false;
            this.gvTransactions.AllowUserToOrderColumns = true;
            this.gvTransactions.AllowUserToResizeRows = false;
            this.gvTransactions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvTransactions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clmId,
            this.clmFrom,
            this.clmAmount,
            this.clmDate,
            this.clmMessage,
            this.clmFlags});
            this.gvTransactions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvTransactions.Location = new System.Drawing.Point(0, 0);
            this.gvTransactions.Name = "gvTransactions";
            this.gvTransactions.RowHeadersVisible = false;
            this.gvTransactions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gvTransactions.Size = new System.Drawing.Size(786, 574);
            this.gvTransactions.TabIndex = 0;
            this.gvTransactions.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.gvTransactions_CellEnter);
            this.gvTransactions.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.gvTransactions_DataError);
            this.gvTransactions.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gvTransactions_MouseUp);
            // 
            // clmId
            // 
            this.clmId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.clmId.DataPropertyName = "BankAccountTransactionK";
            this.clmId.Frozen = true;
            this.clmId.HeaderText = "Transaction ID";
            this.clmId.Name = "clmId";
            this.clmId.ReadOnly = true;
            this.clmId.Width = 120;
            // 
            // clmFrom
            // 
            this.clmFrom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.clmFrom.HeaderText = "From";
            this.clmFrom.Name = "clmFrom";
            this.clmFrom.ReadOnly = true;
            this.clmFrom.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // clmAmount
            // 
            this.clmAmount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.clmAmount.DataPropertyName = "Amount";
            this.clmAmount.HeaderText = "Amount";
            this.clmAmount.Name = "clmAmount";
            this.clmAmount.ReadOnly = true;
            // 
            // clmDate
            // 
            this.clmDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.clmDate.DataPropertyName = "TransactionDateUtc";
            this.clmDate.HeaderText = "Date";
            this.clmDate.Name = "clmDate";
            this.clmDate.ReadOnly = true;
            this.clmDate.Width = 130;
            // 
            // clmMessage
            // 
            this.clmMessage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.clmMessage.DataPropertyName = "Message";
            this.clmMessage.HeaderText = "Message";
            this.clmMessage.Name = "clmMessage";
            // 
            // clmFlags
            // 
            this.clmFlags.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.clmFlags.DataPropertyName = "Flags";
            this.clmFlags.HeaderText = "Flags";
            this.clmFlags.Name = "clmFlags";
            this.clmFlags.Width = 110;
            // 
            // ctxAccount
            // 
            this.ctxAccount.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteAccountToolStripMenuItem});
            this.ctxAccount.Name = "ctxAccount";
            this.ctxAccount.Size = new System.Drawing.Size(156, 26);
            // 
            // deleteAccountToolStripMenuItem
            // 
            this.deleteAccountToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxConfirmDeleteAccount});
            this.deleteAccountToolStripMenuItem.Name = "deleteAccountToolStripMenuItem";
            this.deleteAccountToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.deleteAccountToolStripMenuItem.Text = "Delete &Account";
            // 
            // ctxConfirmDeleteAccount
            // 
            this.ctxConfirmDeleteAccount.Name = "ctxConfirmDeleteAccount";
            this.ctxConfirmDeleteAccount.Size = new System.Drawing.Size(118, 22);
            this.ctxConfirmDeleteAccount.Text = "Confirm";
            // 
            // ctxTransaction
            // 
            this.ctxTransaction.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxDeleteTransactionItem});
            this.ctxTransaction.Name = "ctxTransaction";
            this.ctxTransaction.Size = new System.Drawing.Size(173, 26);
            // 
            // ctxDeleteTransactionItem
            // 
            this.ctxDeleteTransactionItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxDeleteTransactionConfirm});
            this.ctxDeleteTransactionItem.Image = ((System.Drawing.Image)(resources.GetObject("ctxDeleteTransactionItem.Image")));
            this.ctxDeleteTransactionItem.Name = "ctxDeleteTransactionItem";
            this.ctxDeleteTransactionItem.Size = new System.Drawing.Size(172, 22);
            this.ctxDeleteTransactionItem.Text = "&Delete Transaction";
            // 
            // ctxDeleteTransactionConfirm
            // 
            this.ctxDeleteTransactionConfirm.Name = "ctxDeleteTransactionConfirm";
            this.ctxDeleteTransactionConfirm.Size = new System.Drawing.Size(118, 22);
            this.ctxDeleteTransactionConfirm.Text = "C&onfirm";
            this.ctxDeleteTransactionConfirm.Click += new System.EventHandler(this.ctxDeleteTransactionConfirm_Click);
            // 
            // cAccountManagementWndBindingSource
            // 
            this.cAccountManagementWndBindingSource.DataSource = typeof(Wolfje.Plugins.SEconomy.Forms.CAccountManagementWnd);
            // 
            // CAccountManagementWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1103, 649);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "CAccountManagementWnd";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SEconomy Account Manager";
            this.Load += new System.EventHandler(this.CAccountManagementWnd_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvAccounts)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvTransactions)).EndInit();
            this.ctxAccount.ResumeLayout(false);
            this.ctxTransaction.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cAccountManagementWndBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView gvTransactions;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip ctxTransaction;
        private System.Windows.Forms.ToolStripMenuItem ctxDeleteTransactionItem;
        private System.Windows.Forms.ToolStripMenuItem ctxDeleteTransactionConfirm;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel lblName;
        private System.Windows.Forms.ToolStripLabel lblBalance;
        private System.Windows.Forms.ContextMenuStrip ctxAccount;
        private System.Windows.Forms.ToolStripMenuItem deleteAccountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ctxConfirmDeleteAccount;
        private System.Windows.Forms.ToolStripButton btnRefreshTrans;
        private System.Windows.Forms.ToolStripButton btnResetTrans;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel lblOnline;
        private System.Windows.Forms.BindingSource cAccountManagementWndBindingSource;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DataGridView gvAccounts;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmName;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmBalance;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripLabel lblSearch;
        private System.Windows.Forms.ToolStripTextBox txtSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmId;
        private System.Windows.Forms.DataGridViewLinkColumn clmFrom;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmAmount;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmMessage;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmFlags;
        private System.Windows.Forms.ToolStripButton btnShowFrom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnModifyBalance;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
    }
}