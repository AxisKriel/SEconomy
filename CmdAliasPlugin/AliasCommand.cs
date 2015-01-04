using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wolfje.Plugins.SEconomy.CmdAliasModule {
	/// <summary>
	/// Class that represents a command alias,
	/// </summary>
	public class AliasCommand {

		/// <summary>
		/// The command alias.
		/// </summary>
		public string CommandAlias = "";
        
		/// <summary>
		/// The in-order list of commands to execute in place of this
		/// </summary>
		public List<string> CommandsToExecute = new List<string>();
		/// <summary>
		/// How much the command costs the player to execute.  If zero, the command is free.
		/// </summary>
		public string Cost = "0c";

		/// <summary>
		/// List of permissions the user needs to access this command alias.
		/// </summary>
		public string Permissions = "";

		/// <summary>
		/// This is the text to display when a user inputs your alias wrong and it doesn't parse, put some useful information about it here
		/// </summary>
		public string UsageHelpText = "";

		/// <summary>
		/// Time in seconds between when the user is allowed to run the command again
		/// </summary>
		public int CooldownSeconds = 0;

		public static AliasCommand Create(string CommandAlias, string Permissions, string Cost, string HelpText, int CooldownSeconds, params string[] CommandsToRun)
		{
			AliasCommand alias = new AliasCommand();

			alias.CommandAlias = CommandAlias;
			alias.Permissions = Permissions;
			alias.UsageHelpText = HelpText;
			alias.Cost = Cost;
			alias.CooldownSeconds = CooldownSeconds;

			alias.CommandsToExecute.AddRange(CommandsToRun);

			return alias;
		}

	}
}
