using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Reflection {
	public static class Reflect {
		public static Assembly GetExeAssembly() {
			return Assembly.GetExecutingAssembly();
		}

		public static Assembly LoadAssembly(string FilePath) {
			return Assembly.Load(File.ReadAllBytes(FilePath));
		}

		public static bool Inherits(this Type T, Type Base) {
			if (T == Base)
				return false;

			return Base.IsAssignableFrom(T);
		}

		public static bool Inherits<T>(this Type Typ) {
			return Typ.Inherits(typeof(T));
		}

		public static IEnumerable<Type> GetAllTypes(Assembly Asm = null) {
			Assembly[] Assemblies = null;

			if (Asm != null)
				Assemblies = new[] { Asm };
			else
				Assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var A in Assemblies)
				foreach (var T in A.GetTypes())
					yield return T;
		}

		public static IEnumerable<Type> GetAllImplementationsOf(this Assembly Asm, Type Base) {
			foreach (var T in Asm.GetTypes())
				if (!T.IsAbstract && !T.IsInterface && Inherits(T, Base))
					yield return T;
		}

		public static bool IsClass(this Type T) {
			return T.GetTypeInfo().IsClass;
		}

		public static IEnumerable<Type> GetTypesWithAttributes(Type AttributeType, Assembly Asm = null) {
			return GetAllTypes(Asm).Where(T => T.GetCustomAttributes(AttributeType).Count() != 0);
		}

		public static IEnumerable<Type> GetTypesWithAttributes<T>(Assembly Asm = null) {
			return GetTypesWithAttributes(typeof(T), Asm);
		}
	}
}
