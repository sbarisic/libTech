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
using libTech.UI;

using Glfw3;
using OpenGL;
using System.Threading;
using System.Diagnostics;
using Khronos;

using libTech.Graphics;
using glTFLoader;

using Matrix4 = System.Numerics.Matrix4x4;

namespace libTech {
	public unsafe static class Engine {
		internal static Glfw.Window Window;
		internal static RenderDevice RenderDevice;

		static int FPS;

		public static LibTechGame Game;
		public static int Width { get; private set; }
		public static int Height { get; private set; }
		public static int MouseX { get; private set; }
		public static int MouseY { get; private set; }

		static string[] DragDropPaths;
		public static event Action<string[]> OnDragDrop;

		static Stopwatch SWatch = Stopwatch.StartNew();
		public static float TimeSinceStart {
			get {
				return SWatch.ElapsedMilliseconds / 1000.0f;
			}
		}

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
				Update(Dt);
				Draw(Dt);

				// Cap at Target framerate
				while ((float)SWatch.ElapsedMilliseconds / 1000 < Target)
					;

				Dt = (float)SWatch.ElapsedMilliseconds / 1000;
				FPS = (int)(1.0f / Dt);
				SWatch.Restart();
			}

			Environment.Exit(0);
		}

		static void Update(float Dt) {
			Glfw.PollEvents();
			if (DragDropPaths != null) {
				OnDragDrop?.Invoke(DragDropPaths);
				DragDropPaths = null;
			}

			Game.Update(Dt);
		}

		static void Draw(float Dt) {
			Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			Gl.Enable(EnableCap.DepthTest);
			Gl.Disable(EnableCap.CullFace);

			Game.Draw(Dt);

			Gl.Disable(EnableCap.DepthTest);
			NuklearAPI.Frame(() => {
				GConsole.NuklearDraw(10, 10);
			});

			Glfw.SwapBuffers(Window);
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

			Glfw.SetErrorCallback((Err, Msg) => {
				FatalError("GLFW({0}) {1}", Err, Msg);
			});

			Width = CVar.GetInt("width", 800);
			Height = CVar.GetInt("height", 600);

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
			Window = Glfw.CreateWindow(Width, Height, "libTech");
			if (!Window)
				FatalError("Could not create window({0}x{1})", Width, Height);

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

			Gl.ClearColor(69 / 255.0f, 112 / 255.0f, 56 / 255.0f, 1.0f);

			Gl.Enable(EnableCap.Blend);

			Gl.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
			Gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			//Gl.BlendEquation(BlendEquationMode.FuncAdd);
			//Gl.BlendFunc(BlendingFactor.One, BlendingFactor.One);
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

			ShaderProgram GUIShader = new ShaderProgram();
			GUIShader.AttachShader(new ShaderStage(ShaderType.VertexShader).SetSourceFile("content/shaders/gui.vert").Compile());
			GUIShader.AttachShader(new ShaderStage(ShaderType.FragmentShader).SetSourceFile("content/shaders/gui.frag").Compile());
			GUIShader.Link();

			RenderDevice = new RenderDevice(GUIShader, Width, Height);
			NuklearAPI.Init(RenderDevice);
			NuklearAPI.SetClipboardCallback((Txt) => {
				if (string.IsNullOrEmpty(Txt))
					return;

				Glfw.SetClipboardString(Window, Txt);
			}, () => {
				string Str = Glfw.GetClipboardString(Window);
				if (Str == null)
					Str = "";

				return Str;
			});

			Glfw.SetCursorPosCallback(Window, (Wnd, X, Y) => {
				MouseX = (int)X;
				MouseY = (int)Y;
				RenderDevice.OnMouseMove((int)X, (int)Y);
			});

			Glfw.SetMouseButtonCallback(Window, (Wnd, Button, State, Mods) => {
				NuklearEvent.MouseButton NkButton;
				bool IsDown = State == Glfw.InputState.Press ? true : false;

				if (!(State == Glfw.InputState.Press || State == Glfw.InputState.Release))
					return;

				if (Button == Glfw.MouseButton.ButtonLeft)
					NkButton = NuklearEvent.MouseButton.Left;
				else if (Button == Glfw.MouseButton.ButtonMiddle)
					NkButton = NuklearEvent.MouseButton.Middle;
				else if (Button == Glfw.MouseButton.ButtonRight)
					NkButton = NuklearEvent.MouseButton.Right;
				else
					return;

				RenderDevice.OnMouseButton(NkButton, MouseX, MouseY, IsDown);
			});

			Glfw.SetScrollCallback(Window, (Wnd, X, Y) => {
				RenderDevice.OnScroll((float)X, (float)Y);
			});

			Glfw.SetCharCallback(Window, (Wnd, Chr) => {
				RenderDevice.OnText(((char)Chr).ToString());
			});

			Glfw.SetKeyCallback(Window, (Wnd, KCode, SCode, State, Mods) => {
				if (KCode == Glfw.KeyCode.F1 && State == Glfw.InputState.Press)
					GConsole.Open = true;

				NkKeys K = ConvertToNkKey(KCode, Mods);

				if (K != NkKeys.None) {
					RenderDevice.OnKey(K, State == Glfw.InputState.Press);
					if (State == Glfw.InputState.Repeat)
						RenderDevice.OnKey(K, true);
				}
			});

			Glfw.SetDropCallback(Window, (Wnd, Cnt, Paths) => {
				DragDropPaths = Paths;
			});
		}

		static NkKeys ConvertToNkKey(Glfw.KeyCode KCode, Glfw.KeyMods Mods) {
			switch (KCode) {
				case Glfw.KeyCode.RightShift:
				case Glfw.KeyCode.LeftShift:
					return NkKeys.Shift;

				case Glfw.KeyCode.LeftControl:
				case Glfw.KeyCode.RightControl:
					return NkKeys.Ctrl;

				case Glfw.KeyCode.Delete:
					return NkKeys.Del;

				case Glfw.KeyCode.Enter:
				case Glfw.KeyCode.NumpadEnter:
					return NkKeys.Enter;

				case Glfw.KeyCode.Tab:
					return NkKeys.Tab;

				case Glfw.KeyCode.Backspace:
					return NkKeys.Backspace;

				case Glfw.KeyCode.Up:
					return NkKeys.Up;

				case Glfw.KeyCode.Down:
					return NkKeys.Down;

				case Glfw.KeyCode.Left:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextWordLeft;
					return NkKeys.Left;

				case Glfw.KeyCode.Right:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextWordRight;
					return NkKeys.Right;

				case Glfw.KeyCode.Home:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextStart;
					return NkKeys.LineStart;

				case Glfw.KeyCode.End:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextEnd;
					return NkKeys.LineEnd;

				case Glfw.KeyCode.PageUp:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.ScrollStart;
					return NkKeys.ScrollUp;

				case Glfw.KeyCode.PageDown:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.ScrollEnd;
					return NkKeys.ScrollDown;

				case Glfw.KeyCode.A:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextSelectAll;
					return NkKeys.None;

				case Glfw.KeyCode.Z:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextUndo;
					return NkKeys.None;

				case Glfw.KeyCode.Y:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.TextRedo;
					return NkKeys.None;

				case Glfw.KeyCode.C:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.Copy;
					return NkKeys.None;

				case Glfw.KeyCode.V:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.Paste;
					return NkKeys.None;

				case Glfw.KeyCode.X:
					if (Mods == Glfw.KeyMods.Control)
						return NkKeys.Cut;
					return NkKeys.None;

				default:
					return NkKeys.None;
			}
		}
	}
}
