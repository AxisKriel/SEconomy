using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Extensions {
	public static class MySqlCommandExtensions {
		public static IDbDataParameter AddParameter(this IDbCommand command, string name, object data)
		{
			var parm = command.CreateParameter();
			parm.ParameterName = name;
			parm.Value = data;
			command.Parameters.Add(parm);
			return parm;
		}

	}
}
