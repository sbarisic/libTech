using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
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

		public static Type[] GetAllTypes(this Assembly Asm) {
			return Asm.GetTypes();
		}

		public static IEnumerable<Type> GetAllImplementationsOf(this Assembly Asm, Type Base) {
			Type[] AllTypes = GetAllTypes(Asm);

			foreach (var T in AllTypes)
				if (!T.IsAbstract && !T.IsInterface && Inherits(T, Base))
					yield return T;
		}

		public static bool IsClass(this Type T) {
			return T.GetTypeInfo().IsClass;
		}
	}
}
