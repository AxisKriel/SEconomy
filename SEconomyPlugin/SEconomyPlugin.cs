using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;

using System.Xml.Linq;

using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Extensions;
using System.Threading.Tasks;
using System.Threading;

namespace Wolfje.Plugins.SEconomy {

    /// <summary>
    /// Seconomy for Terraria and TShock.  
	/// 
	/// Copyright (C) Tyler Watson, 2013-2014.
    /// 
    /// API Version 1.16
    /// </summary>
    [ApiVersion(1, 16)]
    public class SEconomyPlugin : TerrariaPlugin {

		protected static SEconomy _seconomyInstance = null;

        #region "API Plugin Stub"
        public override string Author {
            get {
                return "Wolfje";
            }
        }

        public override string Description {
            get {
                return "Provides server-sided currency tools for servers running TShock";
            }
        }

        public override string Name {
            get {
                string s = "SEconomy (Milestone 1) Update " + this.Version.Build;
#if __PREVIEW
                s += " Preview";
#endif

                return s;
            }
        }

        public override Version Version {
            get {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        #endregion

		/// <summary>
		/// The loaded instance of SEconomy.  This field can be null.
		/// </summary>
		public static SEconomy Instance
		{
			get
			{
				return _seconomyInstance;
			}
		}

        public SEconomyPlugin(Main Game) : base(Game) {
            Order = 20000;
        }

		public override void Initialize()
		{
			_seconomyInstance = new SEconomy(this);
			Instance.LoadSEconomy();
		}

        protected override void Dispose(bool disposing) {

            if (disposing) {
				_seconomyInstance.Dispose();
				_seconomyInstance = null;
            }

            base.Dispose(disposing);
        }

    }
}
