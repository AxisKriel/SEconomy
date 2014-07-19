using Jint.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using Wolfje.Plugins;
using Wolfje.Plugins.Jist;
using Wolfje.Plugins.Jist.Framework;
using Wolfje.Plugins.SEconomy.Journal;

namespace Wolfje.Plugins.SEconomy.SEconomyScriptPlugin {
    [ApiVersion(1, 16)]
    public class SEconomyScriptPlugin : TerrariaPlugin {
        public override string Author { get { return "Wolfje"; } }
        public override string Description { get { return "Provides SEconomy scripting support to Jist"; } }
        public override string Name { get { return "SEconomy Jist Support"; } }
        public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        public SEconomyScriptPlugin(Terraria.Main game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            JistPlugin.JavascriptFunctionsNeeded += JistPlugin_JavascriptFunctionsNeeded;
        }

        /// <summary>
        /// Tells JIST to submit this class's functions 
        /// to the javascript engine.
        /// </summary>
        private void JistPlugin_JavascriptFunctionsNeeded(object sender, JavascriptFunctionsNeededEventArgs e)
        {
            e.Engine.CreateScriptFunctions(this.GetType(), this);
        }

        /// <summary>
        /// Asynchronously transfers money using SEconomy, then calls back to the JS function specified by completedCallback
        /// </summary>
        [JavascriptFunction("seconomy_transfer_async")]
        public async void SEconomyTransferAsync(Journal.IBankAccount From, Journal.IBankAccount To, Money Amount, string TxMessage, JsValue completedCallback)
        {
            BankTransferEventArgs result = null;
            if (JistPlugin.Instance == null
                || SEconomyPlugin.Instance == null) {
                return;
            }
            
            result = await From.TransferToAsync(To, Amount, 
                Journal.BankAccountTransferOptions.AnnounceToSender, 
                TxMessage, TxMessage);
           
            Jist.JistPlugin.Instance.CallFunction(completedCallback, result);
        }

        [JavascriptFunction("seconomy_pay_async")]
        public async void SEconomyPayAsync(Journal.IBankAccount From, Journal.IBankAccount To, Money Amount, string TxMessage, JsValue completedCallback)
        {
            BankTransferEventArgs result = null;
            if (JistPlugin.Instance == null
                || SEconomyPlugin.Instance == null) {
                return;
            }
            
            result = await From.TransferToAsync(To, Amount,
                Journal.BankAccountTransferOptions.AnnounceToReceiver 
                    | Journal.BankAccountTransferOptions.AnnounceToSender 
                    | Journal.BankAccountTransferOptions.IsPayment,
                TxMessage, TxMessage);

            Jist.JistPlugin.Instance.CallFunction(completedCallback, result);
        }

        /// <summary>
        /// Javascript function:
        ///             seconomy_parse_money(money) : Money
        ///             
        /// Parses money from a string or object and 
        /// returns a money value.
        /// </summary>
        [JavascriptFunction("seconomy_parse_money")]
        public Money SEconomyParseMoney(object MoneyRep)
        {
            try {
                return Money.Parse(MoneyRep.ToString());
            } catch {
                return 0;
            }
        }

        /// <summary>
        /// Javascript function:
        ///             seconomy_valid_money(money) : boolean
        /// Returns true if a supplied money value is valid 
        /// and is parsable or not.
        /// </summary>
        [JavascriptFunction("seconomy_valid_money")]
        public bool SEconomyMoneyValid(object MoneyRep)
        {
            Money _money;
            return Money.TryParse(MoneyRep.ToString(), out _money);
        }

        /// <summary>
        /// Returns a bank account from a player based on an input object.
        /// </summary>
        [JavascriptFunction("seconomy_get_account")]
        public Journal.IBankAccount GetBankAccount(object PlayerRep)
        {
            Journal.IBankAccount bankAccount = null;
            Economy.EconomyPlayer ePlayer = null;
            TShockAPI.TSPlayer player = null;

            if (JistPlugin.Instance == null
                || SEconomyPlugin.Instance == null
                || (player = JistPlugin.Instance.stdTshock.GetPlayer(PlayerRep)) == null
                || (ePlayer = SEconomyPlugin.Instance.GetEconomyPlayerSafe(player.Name)) == null
                || (bankAccount = ePlayer.BankAccount) == null) {
                return null;
            }

            return bankAccount;
        }

        /// <summary>
        /// Returns a reference to the currently running SEconomy world account
        /// </summary>
        [JavascriptFunction("seconomy_world_account")]
        public Journal.IBankAccount WorldAccount()
        {
            if (SEconomyPlugin.Instance == null) {
                return null;
            }
            return SEconomyPlugin.Instance.WorldAccount;
        }
    }
}
