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
