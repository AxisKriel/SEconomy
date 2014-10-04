/*
 * This file is part of SEconomy - A server-sided currency implementation
 * Copyright (C) 2013-2014, Tyler Watson <tyler@tw.id.au>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
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

			public T Current {
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
