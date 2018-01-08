using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libTech.Importer;

namespace libTech {
	public static class Assets {
		public static T Get<T>(string Path) {
			return Importers.Get<T>(Path).Load(Path);
		}
	}
}
