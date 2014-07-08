using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI.DB;
using TShockAPI.Extensions;

namespace Wolfje.Plugins.SEconomy.Extensions {
	public static class QueryResultExtensions {
		public static IEnumerable<IDataReader> AsEnumerable(this QueryResult res)
		{
			return res.Reader.AsEnumerable();
		}

	}
}
