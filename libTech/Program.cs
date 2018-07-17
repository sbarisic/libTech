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
using System.ComponentModel;
using System.Diagnostics;
using CARP;

using libTech.GUI;
using libTech.Reflection;
using libTech.Importer;

using FishGfx.Graphics;
using FishGfx.System;
using libTech.Graphics;

namespace libTech {
	static partial class Engine {
		public static RenderWindow Window;
	}

	unsafe static class Program {
		static LibTechGame Game;

		static void Main(string[] args) {
			string DllDirectory = "native/x86";

			if (IntPtr.Size == 8)
				DllDirectory = "native/x64";

			if (!Kernel32.SetDllDirectory(DllDirectory))
				throw new Win32Exception();

			AppDomain.CurrentDomain.AssemblyResolve += (S, E) => TryLoadAssembly(E.Name, DllDirectory);

			RunGame();
		}

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

		static Assembly TryLoadAssembly(string AssemblyName, string BasePath) {
			AssemblyName Name = new AssemblyName(AssemblyName);
			string ExpectedPath = Path.Combine(Path.GetFullPath(BasePath), Name.Name + ".dll");

			if (File.Exists(ExpectedPath))
				return Assembly.LoadFile(ExpectedPath);

			GConsole.WriteLine("Could not find " + Path.GetFileName(ExpectedPath));
			return null;
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

			AppDomain.CurrentDomain.AssemblyResolve += (S, E) => TryLoadAssembly(E.Name, BasePath);
			Importers.RegisterAll(GameAssembly);

			Game = (LibTechGame)Activator.CreateInstance(GameImplementations[0]);
			Game.Load();
		}

		static void RunGame() {
			ParseVariables();

			FileWatcher.Init("content");
			Importers.RegisterAll(Reflect.GetExeAssembly());

			Engine.Window = new RenderWindow(CVar.GetInt("width", 800), CVar.GetInt("height", 600), "libTech", CVar.GetBool("resizable"));
			Engine.Window.GetWindowSize(out int W, out int H);
			Engine.Window.OnMouseMove += (Wnd, X, Y) => Engine.OnMouseMove(new Vector2(X, H - Y));
			Engine.Window.OnKey += (Wnd, Key, Scancode, Pressed, Repeat, Mods) => Engine.OnKey(Key, Scancode, Pressed, Repeat, Mods);
			LoadGameDll(CVar.GetString("game"));

			ShaderUniforms.Camera.SetOrthogonal(0, 0, W, H);

			float Dt = 0;

			while (!Engine.Window.ShouldClose) {
				Gfx.Clear();

				Game.DrawGUI(Dt);
				Engine.DrawAllGUI();

				Engine.Window.SwapBuffers();
				Events.Poll();
			}
		}
	}
}
