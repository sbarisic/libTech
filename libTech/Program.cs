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

		public static Camera Camera3D;
		public static Camera Camera2D;

		public static ConVar<string> GamePath;
		public static ConVar<int> WindowWidth;
		public static ConVar<int> WindowHeight;
		public static ConVar<bool> WindowResizable;
		public static ConVar<bool> WindowBorderless;

		public static void LogFatal(string Msg) {
			Console.WriteLine(Msg);
			File.AppendAllText("exceptions.txt", Msg + "\n\n");
		}

		public static void LogFatal(string Fmt, params object[] Args) {
			LogFatal(string.Format(Fmt, Args));
		}

		public static void LogFatal(Exception E) {
			LogFatal(E?.ToString() ?? "Unknown Exception Logged");
		}

		public static bool LogFatal(Action A) {
			if (Debugger.IsAttached)
				A();
			else {
				try {
					A();
				} catch (Exception E) {
					LogFatal(E);
					return true;
				}
			}

			return false;
		}
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

			AppDomain.CurrentDomain.UnhandledException += (S, E) => {
				if (!Debugger.IsAttached)
					Engine.LogFatal((E.ExceptionObject ?? "Unknown unhandled exception object").ToString());
			};

			AppDomain.CurrentDomain.AssemblyResolve += (S, E) => TryLoadAssembly(E.Name, DllDirectory);

			if (Engine.LogFatal(RunGame)) {
				Console.WriteLine("\n\nENGINE TERMINATED UNEXPECTEDLY");

				while (true)
					Thread.Sleep(10);
			}
		}

		static void InitConsole() {
			Engine.GamePath = ConVar.Register("game", "basegame", ConVarType.Replicated | ConVarType.Init);
			//Engine.GamePath = ConVar.Register("game", "legprocessor", ConVarType.Replicated | ConVarType.Init);

			Engine.WindowWidth = ConVar.Register("width", 1366, ConVarType.Archive);
			Engine.WindowHeight = ConVar.Register("height", 768, ConVarType.Archive);

			//Engine.WindowWidth = CVar.Register("width", 800, CVarType.Archive);
			//Engine.WindowHeight = CVar.Register("height", 600, CVarType.Archive);

			Engine.WindowBorderless = ConVar.Register("borderless", false, ConVarType.Archive);
			Engine.WindowResizable = ConVar.Register("resizable", false, ConVarType.Archive);

			// Parse all arguments and set CVars
			foreach (var Arg in ArgumentParser.All) {
				switch (Arg.Key) {
					case "console":
						GConsole.Open = true;
						break;

					case "game":
						Engine.GamePath.Value = Arg.Value.Last();
						break;

					default:
						GConsole.Error("Invalid switch '{0}' with value '{1}'", Arg.Key, Arg.Value);
						break;
				}
			}

			foreach (var CVar in ConVar.GetAll())
				GConsole.WriteLine(CVar);

			ConCmd.Register("exit", (Argv) => Environment.Exit(0));
			GConsole.RegisterAlias("quit", "exit");
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
			InitConsole();

			FileWatcher.Init("content");
			Importers.RegisterAll(Reflect.GetExeAssembly());

			Engine.GUI = new libGUI();

			Engine.Window = new RenderWindow(Engine.WindowWidth, Engine.WindowHeight, "libTech", Engine.WindowResizable);
			Engine.Window.OnMouseMove += OnMouseMove;
			Engine.Window.OnKey += OnKey;
			Engine.Window.OnChar += OnChar;
			GConsole.Init();

			GConsole.Color = FishGfx.Color.Orange;
			foreach (var DllName in FailedToLoadDLLs)
				GConsole.WriteLine("Failed to load '{0}'", DllName);
			GConsole.Color = FishGfx.Color.White;

			// Graphics init
			Gfx.Line2D = DefaultShaders.Line2D;
			Gfx.Point2D = DefaultShaders.Point2D;
			Gfx.Default2D = DefaultShaders.DefaultColor2D;

			Gfx.Line3D = DefaultShaders.Line3D;
			Gfx.Point3D = DefaultShaders.Point3D;
			Gfx.Default3D = DefaultShaders.DefaultColor3D;

			// Camera init
			Engine.Camera2D = new Camera();
			Engine.Camera2D.SetOrthogonal(0, 0, Engine.Window.WindowWidth, Engine.Window.WindowHeight);

			Engine.Camera3D = new Camera();
			Engine.Camera3D.SetPerspective(Engine.Window.WindowWidth, Engine.Window.WindowHeight);

			LoadGameDll(Engine.GamePath);

			float Dt = 0;
			while (!Engine.Window.ShouldClose) {
				Update(Dt);
				Draw(Dt);
				Thread.Sleep(0);
			}
		}

		static void Update(float Dt) {
			Events.Poll();

			Game.Update(Dt);
			GConsole.Update();
		}

		static void Draw(float Dt) {
			Gfx.Clear();

			ShaderUniforms.Camera = Engine.Camera3D;
			Game.Draw(Dt);
			Game.DrawTransparent(Dt);

			ShaderUniforms.Camera = Engine.Camera2D;
			Game.DrawGUI(Dt);
			Engine.GUI.Draw();

			Engine.Window.SwapBuffers();
		}

		private static void OnChar(RenderWindow Wnd, string Char, uint Unicode) {
			Engine.GUI.SendOnChar(Char, Unicode);
		}

		private static void OnKey(RenderWindow Wnd, Key Key, int Scancode, bool Pressed, bool Repeat, KeyMods Mods) {
			if (Key == Key.F1 && Pressed) {
				GConsole.Open = !GConsole.Open;
				return;
			} else if (Key == Key.F3 && Pressed) {
				Engine.Window.ShowCursor = false;
			} else if (Key == Key.F2 && Pressed) {
				Engine.Window.ShowCursor = true;
			}

			Engine.GUI.SendOnKey(Key, Scancode, Pressed, Repeat, Mods);
		}

		private static void OnMouseMove(RenderWindow Wnd, float X, float Y) {
			Engine.GUI.SendOnMouseMove(new Vector2(X, Engine.WindowHeight - Y));
		}
	}
}
