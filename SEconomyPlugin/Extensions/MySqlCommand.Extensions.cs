using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Extensions {
	public static class MySqlCommandExtensions {
		public static MySql.Data.MySqlClient.MySqlParameter AddParameter(this MySql.Data.MySqlClient.MySqlCommand command, string name, object data)
		{
			var parm = command.CreateParameter();
			parm.ParameterName = name;
			parm.Value = data;
			command.Parameters.Add(parm);
			return parm;
		}
	}
}
