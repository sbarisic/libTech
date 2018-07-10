using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Numerics;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using CARP;

using libTech.UI;
using libTech.Reflection;
using libTech.Importer;

using FishGfx.Graphics;
using FishGfx.System;

namespace libTech {
	public unsafe static class Program {
		static RenderWindow Window;
		static LibTechGame Game;

		static void ParseVariables() {
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
		}

		static void LoadGameDll(string BasePath) {
			string GameFolder = CVar.GetString("game");
			BasePath = Path.Combine(GameFolder, "Game.dll");

			Assembly GameAssembly = Reflect.LoadAssembly(BasePath);
			Type[] GameImplementations = Reflect.GetAllImplementationsOf(GameAssembly, typeof(LibTechGame)).ToArray();

			if (GameImplementations.Length == 0) {
				GConsole.WriteLine("No game implementations found in " + BasePath);
				Environment.Exit(0);
			} else if (GameImplementations.Length > 1) {
				GConsole.WriteLine("Too many game implementations in " + BasePath);
				Environment.Exit(0);
			}

			Importers.RegisterAll(GameAssembly);

			Game = (LibTechGame)Activator.CreateInstance(GameImplementations[0]);
			Game.Load();
		}

		static void Main(string[] args) {
			ParseVariables();

			FileWatcher.Init("content");
			Importers.RegisterAll(Reflect.GetExeAssembly());

			Window = new RenderWindow(CVar.GetInt("width", 800), CVar.GetInt("height", 600), "libTech", CVar.GetBool("resizable"));
			LoadGameDll(CVar.GetString("game"));

			while (!Window.ShouldClose) {
				Gfx.Clear();

				Window.SwapBuffers();
				Events.Poll();
			}
		}
	}
}
