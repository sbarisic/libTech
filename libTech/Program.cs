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
		public static libGUI GUI;

		public static CVar<string> GamePath;
		public static CVar<int> WindowWidth;
		public static CVar<int> WindowHeight;
		public static CVar<bool> WindowResizable;
		public static CVar<bool> WindowBorderless;
	}

	unsafe static class Program {
		static LibTechGame Game;

		static List<string> FailedToLoadDLLs;

		static void Main(string[] args) {
			string DllDirectory = "native/x86";

			if (IntPtr.Size == 8)
				DllDirectory = "native/x64";

			if (!Kernel32.SetDllDirectory(DllDirectory))
				throw new Win32Exception();

			FailedToLoadDLLs = new List<string>();
			AppDomain.CurrentDomain.AssemblyResolve += (S, E) => TryLoadAssembly(E.Name, DllDirectory);
			RunGame();
		}

		static void ParseVariables() {
			Engine.GamePath = CVar.Register("game", "basegame", CVarType.Replicated | CVarType.Init);

			Engine.WindowWidth = CVar.Register("width", 1366, CVarType.Archive);
			Engine.WindowHeight = CVar.Register("height", 768, CVarType.Archive);
			Engine.WindowBorderless = CVar.Register("borderless", false, CVarType.Archive);
			Engine.WindowResizable = CVar.Register("resizable", false, CVarType.Archive);

			// Parse all arguments and set CVars
			foreach (var Arg in ArgumentParser.All) {
				switch (Arg.Key) {
					case "console":
						GConsole.Open = true;
						break;
				}
			}

			foreach (var CVar in CVar.GetAll())
				GConsole.WriteLine(CVar);
		}

		static Assembly TryLoadAssembly(string AssemblyName, string BasePath) {
			AssemblyName Name = new AssemblyName(AssemblyName);
			string ExpectedPath = Path.Combine(Path.GetFullPath(BasePath), Name.Name + ".dll");

			if (File.Exists(ExpectedPath))
				return Assembly.LoadFile(ExpectedPath);

			FailedToLoadDLLs.Add(Path.GetFileName(ExpectedPath));
			return null;
		}

		static void LoadGameDll(string BasePath) {
			BasePath = Path.Combine(BasePath, "Game.dll");

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

			Engine.GUI = new libGUI();

			Engine.Window = new RenderWindow(Engine.WindowWidth, Engine.WindowHeight, "libTech", Engine.WindowResizable);
			Engine.Window.GetWindowSize(out int W, out int H);
			Engine.Window.OnMouseMove += OnMouseMove;
			Engine.Window.OnKey += OnKey;
			Engine.Window.OnChar += OnChar;
			GConsole.Init();
			LoadGameDll(Engine.GamePath);

			GConsole.Color = FishGfx.Color.Orange;
			foreach (var DllName in FailedToLoadDLLs)
				GConsole.WriteLine("Failed to load '{0}'", DllName);
			GConsole.Color = FishGfx.Color.White;

			ShaderUniforms.Camera.SetOrthogonal(0, 0, W, H);

			float Dt = 0;


			while (!Engine.Window.ShouldClose) {
				Update(Dt);
				Draw(Dt);
			}
		}

		static void Update(float Dt) {
			Events.Poll();

			Game.Update(Dt);
			GConsole.Update();
		}

		static void Draw(float Dt) {
			Gfx.Clear();

			Game.Draw(Dt);
			Game.DrawTransparent(Dt);

			Game.DrawGUI(Dt);
			Engine.GUI.Draw();

			Engine.Window.SwapBuffers();
		}

		private static void OnChar(RenderWindow Wnd, string Char, uint Unicode) {
			Engine.GUI.SendOnChar(Char, Unicode);
		}

		private static void OnKey(RenderWindow Wnd, Key Key, int Scancode, bool Pressed, bool Repeat, KeyMods Mods) {
			Engine.GUI.SendOnKey(Key, Scancode, Pressed, Repeat, Mods);
		}

		private static void OnMouseMove(RenderWindow Wnd, float X, float Y) {
			Engine.GUI.SendOnMouseMove(new Vector2(X, Engine.WindowHeight - Y));
		}
	}
}
