using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wolfje.Plugins.SEconomy.CmdAliasModule;

namespace Wolfje.Plugins.SEconomy.JistAliasPlugin {
    public class JScriptAliasCommand : AliasCommand {
        
        public Jint.Native.JsFunction func;

        public static JScriptAliasCommand Create(string AliasName, string Cost, int CooldownSeconds, string PermissionNeeded, Jint.Native.JsFunction func) {
            return new JScriptAliasCommand() { CommandAlias = AliasName, CommandsToExecute = null, CooldownSeconds = CooldownSeconds, Permissions = PermissionNeeded, func = func };
        }


    }
}
