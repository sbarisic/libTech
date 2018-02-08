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

using NuklearDotNet;
using System.Runtime.InteropServices;
using libTech.Renderer;
using libTech.UI;

using Glfw3;
using OpenGL;
using System.Threading;
using System.Diagnostics;
using Khronos;

using libTech.Graphics;

namespace libTech {
	public unsafe static class Engine {
		internal static Glfw.Window Window;

		static int FPS;

		public static LibTechGame Game;

		static void Main(string[] args) {
			CVar.InitMode = true;
			CVar.Register("game", "basegame", CVarType.Replicated | CVarType.Init, (This, Old, New) => This.Value = Path.GetFullPath((string)New));

			CVar.Register("width", 800, CVarType.Archive);
			CVar.Register("height", 600, CVarType.Archive);
			CVar.Register("borderless", false, CVarType.Archive);
			CVar.Register("resizable", false, CVarType.Archive);
			CVar.Register("gl_doublebuffer", true, CVarType.Archive);
			CVar.Register("gl_samples", 0, CVarType.Archive);

			CVar.Register("gl_forwardcompat", true, CVarType.Archive | CVarType.Init | CVarType.Unsafe);
			CVar.Register("gl_major", 4, CVarType.Archive | CVarType.Init | CVarType.Unsafe);
			CVar.Register("gl_minor", 5, CVarType.Archive | CVarType.Init | CVarType.Unsafe);

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

			Importers.RegisterAll(Reflect.GetExeAssembly());

			CreateContext();
			LoadContent();

			Stopwatch SWatch = Stopwatch.StartNew();
			float Target = 1.0f / 120;
			float Dt = Target;

			while (!Glfw.WindowShouldClose(Window)) {
				Glfw.PollEvents();
				Game.Update(Dt);

				Game.Draw(Dt);
				Glfw.SwapBuffers(Window);
				
				// Cap at Target framerate
				while ((float)SWatch.ElapsedMilliseconds / 1000 < Target)
					;

				Dt = (float)SWatch.ElapsedMilliseconds / 1000;
				FPS = (int)(1.0f / Dt);
				SWatch.Restart();
			}

			Environment.Exit(0);
		}

		static void FatalError(string Msg) {
			GConsole.WriteLine(Msg);

			while (true)
				Thread.Sleep(10);
		}

		static void FatalError(string Fmt, params object[] Args) {
			FatalError(string.Format(Fmt, Args));
		}

		static void CreateContext(/*params string[] RequiredExtensions*/) {
			GConsole.WriteLine("Initializing OpenGL");
			Gl.Initialize();

			GConsole.WriteLine("Initializing GLFW");
			Glfw.ConfigureNativesDirectory("native/glfw3_64");
			if (!Glfw.Init())
				FatalError("Could not initialize GLFW");

			int W = CVar.GetInt("width", 800);
			int H = CVar.GetInt("height", 600);

			Glfw.WindowHint(Glfw.Hint.Resizable, CVar.GetBool("resizable"));

			//Glfw.WindowHint(Glfw.Hint.ClientApi, Glfw.ClientApi.None);
			Glfw.WindowHint(Glfw.Hint.ClientApi, Glfw.ClientApi.OpenGL);
			Glfw.WindowHint(Glfw.Hint.ContextCreationApi, Glfw.ContextApi.Native);
			Glfw.WindowHint(Glfw.Hint.Doublebuffer, CVar.GetBool("gl_doublebuffer"));
			Glfw.WindowHint(Glfw.Hint.ContextVersionMajor, CVar.GetInt("gl_major"));
			Glfw.WindowHint(Glfw.Hint.ContextVersionMinor, CVar.GetInt("gl_minor"));

			Glfw.WindowHint(Glfw.Hint.Samples, CVar.GetInt("gl_samples"));
			Glfw.WindowHint(Glfw.Hint.OpenglForwardCompat, CVar.GetBool("gl_forwardcompat"));
			Glfw.WindowHint(Glfw.Hint.OpenglProfile, Glfw.OpenGLProfile.Core);
#if DEBUG
			Glfw.WindowHint(Glfw.Hint.OpenglDebugContext, true);
#endif

			GConsole.WriteLine("Creating window");
			Window = Glfw.CreateWindow(W, H, "libTech");
			if (!Window)
				FatalError("Could not create window({0}x{1})", W, H);

			Glfw.MakeContextCurrent(Window);

			/*bool AllSupported = true;

			for (int i = 0; i < RequiredExtensions.Length; i++) {
				if (!Glfw.ExtensionSupported(RequiredExtensions[i])) {
					GConsole.WriteLine("{0} not supported", RequiredExtensions[i]);
					AllSupported = false;
				}
			}

			if (!AllSupported)
				while (true)
					Thread.Sleep(10);*/

#if DEBUG
			Gl.DebugMessageCallback((Src, DbgType, ID, Severity, Len, Buffer, UserPtr) => {
				if (Severity == Gl.DebugSeverity.Notification)
					return;

				GConsole.WriteLine("OpenGL {0} {1} {2}, {3}", Src, DbgType, ID, Severity);
				GConsole.WriteLine(Encoding.ASCII.GetString((byte*)Buffer, Len));

				if ((Severity == Gl.DebugSeverity.Medium || Severity == Gl.DebugSeverity.High) && Debugger.IsAttached)
					Debugger.Break();
			}, IntPtr.Zero);
#endif

			GConsole.WriteLine("{0}, {1}", Gl.GetString(StringName.Vendor), Gl.GetString(StringName.Renderer));
			GConsole.WriteLine("OpenGL {0}", Gl.GetString(StringName.Version));
			GConsole.WriteLine("GLSL {0}", Gl.GetString(StringName.ShadingLanguageVersion));

			Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
		}

		static void LoadContent() {
			string GameDllPath = Path.Combine(CVar.GetString("game"), "Game.dll");

			if (!File.Exists(GameDllPath))
				FatalError("File not found: {0}", GameDllPath);

			Assembly GameAssembly = Reflect.LoadAssembly(GameDllPath);
			Importers.RegisterAll(GameAssembly);

			Type[] GameImplementations = Reflect.GetAllImplementationsOf(GameAssembly, typeof(LibTechGame)).ToArray();

			if (GameImplementations.Length == 0)
				FatalError("Could not find game implementation in {0}", GameDllPath);
			if (GameImplementations.Length > 1)
				FatalError("Found too many game implementations in {0}", GameDllPath);

			Game = (LibTechGame)Activator.CreateInstance(GameImplementations[0]);
			Game.Load();

			/*base.OnLoadingContent();

			IUltravioletPlatform Platform = Ultraviolet.GetPlatform();
			IUltravioletWindow Window = Platform.Windows.GetPrimary();

			ScreenService.Init(Ultraviolet.GetUI());

			if (Window != null) {
				Window.Caption = "libTech";
				Window.ClientSize = new Size2(CVar.GetInt("width", 800), CVar.GetInt("height", 600));
				Window.SetWindowedClientSizeCentered(Window.ClientSize);
			}

			IUltravioletInput Input = Ultraviolet.GetInput();
			MouseDevice Mouse = Input.GetMouse();
			KeyboardDevice Keyboard = Input.GetKeyboard();

			Mouse.Moved += (IUltravioletWindow W, MouseDevice Dev, int X, int Y, int DX, int DY) => {
				RenderDevice.OnMouseMove(X, Y);
			};

			Mouse.WheelScrolled += (W, Dev, X, Y) => {
				RenderDevice.OnScroll(X, Y);
			};

			Mouse.ButtonPressed += (W, Dev, Button) => {
				Point2 Pos = Dev.Position;
				if (Button == MouseButton.Left || Button == MouseButton.Middle || Button == MouseButton.Right)
					RenderDevice.OnMouseButton((NuklearEvent.MouseButton)(Button - 1), Pos.X, Pos.Y, true);
			};

			Mouse.ButtonReleased += (W, Dev, Button) => {
				Point2 Pos = Dev.Position;
				if (Button == MouseButton.Left || Button == MouseButton.Middle || Button == MouseButton.Right)
					RenderDevice.OnMouseButton((NuklearEvent.MouseButton)(Button - 1), Pos.X, Pos.Y, false);
			};

			Keyboard.KeyPressed += (W, Dev, Key, Ctrl, Alt, Shift, Repeat) => {
				if (Key == Key.F1) {
					GConsole.Open = true;
					NuklearAPI.QueueForceUpdate();
					return;
				}

				NkKeys K = Key.ToNkKeys();
				if (K != (NkKeys)(-1))
					RenderDevice.OnKey(K, true);
			};

			Keyboard.KeyReleased += (W, Dev, Key) => {
				NkKeys K = Key.ToNkKeys();
				if (K != (NkKeys)(-1))
					RenderDevice.OnKey(K, false);
			};

			Keyboard.TextEditing += (W, Dev) => {
				Console.WriteLine("Text editing");
			};

			StringBuilder TextInputBuffer = new StringBuilder();

			Keyboard.TextInput += (W, Dev) => {
				Dev.GetTextInput(TextInputBuffer);
				RenderDevice.OnText(TextInputBuffer.ToString());
			};

			NuklearAPI.Init(RenderDevice);*/
		}
	}
}
