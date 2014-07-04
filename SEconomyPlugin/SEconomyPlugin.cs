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
		public static Lang.Localization Locale { get; private set; }

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
			Lang.Localization.PrepareLanguages();
			Locale = new Lang.Localization("en-AU");

			PrintIntro();

			TShockAPI.Commands.ChatCommands.Add(new TShockAPI.Command(TShock_CommandExecuted, "seconomy", "sec"));
			_seconomyInstance = new SEconomy(this);
			Instance.LoadSEconomy();
		}

		protected void PrintIntro()
		{
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write(" SEconomy Update ");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(this.Version.Build);
			
#if __PREVIEW
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write(" Preview");
#endif

			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write(" Copyright (C) Wolfje, 2014 - ");

			Console.ForegroundColor = ConsoleColor.Blue;
			Console.Write("http://plugins.tw.id.au");

			Console.WriteLine("\r\n");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(" Please wait...");
			Console.WriteLine();
			Console.ResetColor();
		}

		protected async void TShock_CommandExecuted(TShockAPI.CommandArgs args)
		{
			if (args.Parameters.Count == 0) {
				args.Player.SendInfoMessage(string.Format(Locale.StringOrDefault(3, "SEconomy v{0} by Wolfje"), this.Version));
				args.Player.SendInfoMessage(" * http://plugins.tw.id.au");
				args.Player.SendInfoMessage(Locale.StringOrDefault(5, " * /sec[onomy] reload|rl - Reloads SEconomy"));
				args.Player.SendInfoMessage(Locale.StringOrDefault(6, " * /sec[onomy] stop - Stops and unloads SEconomy"));
				args.Player.SendInfoMessage(Locale.StringOrDefault(7, " * /sec[onomy] start - Reloads SEconomy"));
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

					args.Player.SendSuccessMessage(Locale.StringOrDefault(8, "SEconomy is reloaded."));
			}

			if (args.Parameters[0].Equals("stop", StringComparison.CurrentCultureIgnoreCase)
				&& args.Player.Group.HasPermission("seconomy.command.stop") == true) {
				if (_seconomyInstance == null) {
					args.Player.SendErrorMessage(Locale.StringOrDefault(9, "seconomy stop: SEconomy is already stopped. Use /sec start to start"));
					return;
				}

				await Task.Run(() => {
					_seconomyInstance.Dispose();
					_seconomyInstance = null;
				});

				args.Player.SendSuccessMessage(Locale.StringOrDefault(10, "SEconomy is stopped."));
			}

			if (args.Parameters[0].Equals("start", StringComparison.CurrentCultureIgnoreCase)
				&& args.Player.Group.HasPermission("seconomy.command.start") == true) {
				if (_seconomyInstance != null) {
					args.Player.SendErrorMessage(Locale.StringOrDefault(11, "seconomy stop: SEconomy is already started. Use /sec stop to stop."));
					return;
				}
				try {
					await Task.Run(async () => {
						_seconomyInstance = new SEconomy(this);
						_seconomyInstance.LoadSEconomy();
						await _seconomyInstance.BindToWorldAsync();
					});
				} catch {
					args.Player.SendErrorMessage(Locale.StringOrDefault(12, "SEconomy failed to initialize, and will be unavailable for this session."));
				}

				args.Player.SendSuccessMessage(Locale.StringOrDefault(13, "SEconomy has started."));
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
