using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using Wolfje.Plugins.SEconomy.CmdAliasModule;

namespace Wolfje.Plugins.SEconomy.JistAliasModule {
	[ApiVersion(1, 20)]
	public class JistAliasPlugin : TerrariaPlugin {
		public override string Author { get { return "Wolfje"; } }

		public override string Description { get { return "Provides AliasCmd scripting support for Jist"; } }

		public override string Name { get { return "JistAlias"; } }

		public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

		public JistAlias Instance { get; protected set; }

		public JistAliasPlugin(Main game)
			: base(game)
		{
			base.Order = Int32.MaxValue - 2;
		}


		public override void Initialize()
		{
			this.Instance = new JistAlias(this);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing) {
				this.Instance.Dispose();
			}
		}
	}
}
