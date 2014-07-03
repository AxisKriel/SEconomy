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
			TShockAPI.Commands.ChatCommands.Add(new TShockAPI.Command(TShock_CommandExecuted, "seconomy", "sec"));
			_seconomyInstance = new SEconomy(this);
			Instance.LoadSEconomy();
		}

		protected async void TShock_CommandExecuted(TShockAPI.CommandArgs args)
		{
			if (args.Parameters.Count == 0) {
				args.Player.SendInfoMessage("SEconomy v" + this.Version + " by Wolfje");
				args.Player.SendInfoMessage(" * http://plugins.tw.id.au/");
				args.Player.SendInfoMessage(" * /sec[onomy] reload|rl - Reloads SEconomy");
				args.Player.SendInfoMessage(" * /sec[onomy] stop - Stops and unloads SEconomy");
				args.Player.SendInfoMessage(" * /sec[onomy] start - Reloads SEconomy");
				return;
			}

			if ((args.Parameters[0].Equals("reload", StringComparison.CurrentCultureIgnoreCase)
				|| args.Parameters[0].Equals("rl", StringComparison.CurrentCultureIgnoreCase))
				&& args.Player.Group.HasPermission("seconomy.command.reload") == true) {
				
					await Task.Run(async () => {
						if (_seconomyInstance != null) {
							_seconomyInstance.Dispose();
							_seconomyInstance = null;
						}

						_seconomyInstance = new SEconomy(this);
						_seconomyInstance.LoadSEconomy();
						await _seconomyInstance.BindToWorldAsync();
					});
			}

			if (args.Parameters[0].Equals("stop", StringComparison.CurrentCultureIgnoreCase)
				&& args.Player.Group.HasPermission("seconomy.command.stop") == true) {
				if (_seconomyInstance == null) {
					args.Player.SendErrorMessage("seconomy stop: SEconomy is already stopped. Use /sec start to start");
					return;
				}

				await Task.Run(() => {
					_seconomyInstance.Dispose();
					_seconomyInstance = null;
				});
			}

			if (args.Parameters[0].Equals("start", StringComparison.CurrentCultureIgnoreCase)
				&& args.Player.Group.HasPermission("seconomy.command.start") == true) {
				if (_seconomyInstance != null) {
					args.Player.SendErrorMessage("seconomy stop: SEconomy is already started. Use /sec stop to stop.");
					return;
				}
				await Task.Run(async() => {
					_seconomyInstance = new SEconomy(this);
					_seconomyInstance.LoadSEconomy();
					await _seconomyInstance.BindToWorldAsync();
				});
			}
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
