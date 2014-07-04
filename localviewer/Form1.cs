using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wolfje.Plugins.SEconomy.Lang;
using Wolfje.Plugins.SEconomy.Extensions;
using System.IO;
using System.Reflection;

namespace localviewer {
	public partial class Form1 : Form {
		public Form1()
		{
			InitializeComponent();

			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}

		System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\ServerPlugins";
			string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
			if (File.Exists(assemblyPath) == false) return null;
			Assembly assembly = Assembly.LoadFrom(assemblyPath);
			return assembly;
		}

		private void txtLocale_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter) {
				LoadLocale();
			}
		}

		void LoadLocale()
		{
			string loc = null;
			Localization locProvider = null;
			if (string.IsNullOrEmpty((loc = this.txtLocale.Text)) == true) {
				return;
			}

			try {
				locProvider = new Localization(loc);

				for (int i = 0; i < locProvider.StringTable.Length; i++) {
					ListViewItem item = lvStringTable.Items.Add(locProvider.StringOrDefault(i, "<Unknown>"));
					item.SubItems.Add(i.ToString());
					item.SubItems.Add(locProvider.StringOrDefault(i, "<Unknown>"));
				}
			} catch (Exception ex) {
				return;
			}

		}

		private void lvStringTable_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}
	}
}
