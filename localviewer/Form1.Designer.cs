namespace localviewer {
	partial class Form1 {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
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
		private void InitializeComponent()
		{
			this.lvStringTable = new System.Windows.Forms.ListView();
			this.clm2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.clm1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label1 = new System.Windows.Forms.Label();
			this.txtLocale = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// lvStringTable
			// 
			this.lvStringTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lvStringTable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clm2,
            this.clm1});
			this.lvStringTable.Location = new System.Drawing.Point(13, 39);
			this.lvStringTable.Name = "lvStringTable";
			this.lvStringTable.Size = new System.Drawing.Size(628, 501);
			this.lvStringTable.TabIndex = 0;
			this.lvStringTable.UseCompatibleStateImageBehavior = false;
			this.lvStringTable.View = System.Windows.Forms.View.Details;
			this.lvStringTable.SelectedIndexChanged += new System.EventHandler(this.lvStringTable_SelectedIndexChanged);
			// 
			// clm2
			// 
			this.clm2.DisplayIndex = 1;
			this.clm2.Text = "ID";
			this.clm2.Width = 561;
			// 
			// clm1
			// 
			this.clm1.DisplayIndex = 0;
			this.clm1.Text = "Entry";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(82, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Load Language";
			// 
			// txtLocale
			// 
			this.txtLocale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLocale.Location = new System.Drawing.Point(100, 13);
			this.txtLocale.Name = "txtLocale";
			this.txtLocale.Size = new System.Drawing.Size(541, 20);
			this.txtLocale.TabIndex = 2;
			this.txtLocale.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtLocale_KeyUp);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(653, 552);
			this.Controls.Add(this.txtLocale);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lvStringTable);
			this.Name = "Form1";
			this.Text = "SEconomy locale viewer";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView lvStringTable;
		private System.Windows.Forms.ColumnHeader clm1;
		private System.Windows.Forms.ColumnHeader clm2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtLocale;
	}
}

