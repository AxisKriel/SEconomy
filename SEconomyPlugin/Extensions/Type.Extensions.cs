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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wolfje.Plugins.SEconomy.Extensions {
	public static class TypeExtensions {

		/// <summary>
		/// Reflects on a private method.  Can remove this if TShock opens up a bit more of their API publicly
		/// </summary>
		public static T CallPrivateMethod<T>(this Type _type, bool StaticMember, string Name, params object[] Params)
		{
			BindingFlags flags = BindingFlags.NonPublic;
			if (StaticMember) {
				flags |= BindingFlags.Static;
			} else {
				flags |= BindingFlags.Instance;
			}
			MethodInfo method = _type.GetMethod(Name, flags);
			return (T)method.Invoke(StaticMember ? null : _type, Params);
		}

		/// <summary>
		/// Reflects on a private instance member of a class.  Can remove this if TShock opens up a bit more of their API publicly
		/// </summary>
		public static T GetPrivateField<T>(this Type type, object Instance, string Name, params object[] Param)
		{
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
			FieldInfo field = type.GetField(Name, flags) as FieldInfo;
			if (field == null) {
				return default(T);
			}

			return (T)field.GetValue(Instance);
		}
	}
}
