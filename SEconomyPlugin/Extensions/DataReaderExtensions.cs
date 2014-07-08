using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Extensions {
	public static class DataReaderExtension {
		public class EnumeratorWrapper<T> {
			private readonly Func<bool> moveNext;
			private readonly Func<T> current;

			public EnumeratorWrapper(Func<bool> moveNext, Func<T> current)
			{
				this.moveNext = moveNext;
				this.current = current;
			}

			public EnumeratorWrapper<T> GetEnumerator()
			{
				return this;
			}

			public bool MoveNext()
			{
				return moveNext();
			}

			public T Current
			{
				get { return current(); }
			}
		}

		private static IEnumerable<T> BuildEnumerable<T>(
				Func<bool> moveNext, Func<T> current)
		{
			var po = new EnumeratorWrapper<T>(moveNext, current);
			foreach (var s in po)
				yield return s;
		}

		public static IEnumerable<T> AsEnumerable<T>(this T source) where T : IDataReader
		{
			return BuildEnumerable(source.Read, () => source);
		}
	}
}
