using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libTech.Reflection;
using libTech.Importer;
using System.Reflection;
using System.Numerics;
using CARP;
using System.IO;
using System.Drawing;

using System.Runtime.InteropServices;
using libTech.UI;

using System.Threading;
using System.Diagnostics;

namespace libTech {
	public unsafe static class Program {
		static void Main(string[] args) {
			CVar.InitMode = true;
			CVar.Register("game", "basegame", CVarType.Replicated | CVarType.Init, (This, Old, New) => This.Value = Path.GetFullPath((string)New));

			CVar.Register("width", 1366, CVarType.Archive);
			CVar.Register("height", 768, CVarType.Archive);
			CVar.Register("borderless", false, CVarType.Archive);
			CVar.Register("resizable", false, CVarType.Archive);
		
			// Parse all arguments and set CVars
			foreach (var Arg in ArgumentParser.All) {
				switch (Arg.Key) {
					case "console":
						GConsole.Open = true;
						break;

					default: {
							CVar CVar = CVar.Find(Arg.Key);

							if (CVar != null)
								CVar.Value = Arg.Value.LastOrDefault();
							else
								CVar.Register(Arg.Key, Arg.Value.LastOrDefault());

							break;
						}
				}
			}

			CVar.InitMode = false;
			foreach (var CVar in CVar.GetAll())
				GConsole.WriteLine(CVar);

			FileWatcher.Init("content");
			Importers.RegisterAll(Reflect.GetExeAssembly());

			
		}
	}
}
