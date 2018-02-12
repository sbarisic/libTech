using libTech.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Importer {
	public static class Importers {
		static List<Importer> AllImporters;

		static Importers() {
			AllImporters = new List<Importer>();
		}

		public static void Register(Type T) {
			foreach (var Imp in AllImporters)
				if (Imp.GetType() == T)
					throw new Exception("Importer already registered: " + T.ToString());

			AllImporters.Add((Importer)Activator.CreateInstance(T));
		}

		public static void RegisterAll(Assembly A) {
			foreach (var Type in Reflect.GetAllImplementationsOf(A, typeof(Importer)))
				Register(Type);
		}

		public static IEnumerable<Importer<T>> GetAll<T>() {
			foreach (var Imp in AllImporters) {
				Importer<T> TImp = Imp as Importer<T>;

				if (TImp != null)
					yield return TImp;
			}
		}

		public static Importer<T> Get<T>(string FilePath) {
			IEnumerable<Importer<T>> Importers = GetAll<T>();

			foreach (var Imp in Importers)
				if (Imp.CanLoad(FilePath))
					return Imp;

			throw new Exception("Could not find importer for \"" + FilePath + "\"");
		}

		public static T Load<T>(string FilePath) {
			return Get<T>(FilePath).Load(FilePath);
		}
	}

	public abstract class Importer {
		public virtual bool CanLoad(string FilePath) {
			return CanLoadExt(Path.GetExtension(FilePath));
		}

		public abstract bool CanLoadExt(string Extension);
	}

	public abstract class Importer<T> : Importer {
		public abstract T Load(string FilePath);
	}
}
