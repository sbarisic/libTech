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

using NuklearDotNet;
using System.Runtime.InteropServices;
using libTech.UI;

using Glfw3;
using OpenGL;
using System.Threading;
using System.Diagnostics;
using Khronos;

using libTech.Graphics;

using Matrix4 = System.Numerics.Matrix4x4;

namespace libTech {
	public unsafe static partial class Engine {
		internal static void FatalError(string Msg) {
			GConsole.WriteLine(Msg);

			while (true)
				Thread.Sleep(10);
		}

		internal static void FatalError(string Fmt, params object[] Args) {
			FatalError(string.Format(Fmt, Args));
		}

		static void CreateContext(/*params string[] RequiredExtensions*/) {
			GConsole.WriteLine("Initializing OpenGL");

#if DEBUG
			Khronos.KhronosApi.Log += (S, E) => {
				if (E.Name == "glGetError")
					return;
				
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("OpenGL> {0}", string.Format("{0}({1}) = {2}", E.Name, string.Join(", ", E.Args), E.ReturnValue ?? "null"));
				Console.ResetColor();
			};
			Khronos.KhronosApi.LogEnabled = false;//*/
#endif

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

			Gl.Initialize();
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

				if ((/*Severity == Gl.DebugSeverity.Medium ||*/ Severity == Gl.DebugSeverity.High) && Debugger.IsAttached)
					Debugger.Break();
			}, IntPtr.Zero);

			Gl.Enable((EnableCap)Gl.DEBUG_OUTPUT);
			Gl.Enable((EnableCap)Gl.DEBUG_OUTPUT_SYNCHRONOUS);
#endif

			GConsole.WriteLine("{0}, {1}", Gl.GetString(StringName.Vendor), Gl.GetString(StringName.Renderer));
			GConsole.WriteLine("OpenGL {0}", Gl.GetString(StringName.Version));
			GConsole.WriteLine("GLSL {0}", Gl.GetString(StringName.ShadingLanguageVersion));

			Gl.ClearColor(69 / 255.0f, 112 / 255.0f, 56 / 255.0f, 1.0f);

			// Fuck the police
			Gl.Enable((EnableCap)Gl.DEPTH_CLAMP);
			Gl.Enable(EnableCap.Blend);
			//Gl.VERTEX_PROGRAM_POINT_SIZE;
			Gl.Enable((EnableCap)Gl.VERTEX_PROGRAM_POINT_SIZE);
			//Gl.Enable((EnableCap)Gl.SAMPLE_SHADING);
			//Gl.Enable(EnableCap.FramebufferSrgb);

			Gl.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
			Gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			//Gl.BlendEquation(BlendEquationMode.FuncAdd);
			//Gl.BlendFunc(BlendingFactor.One, BlendingFactor.One);

			ShaderProgram.LoadDefaultShaders();
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

			RenderDevice = new RenderDevice(ShaderProgram.GUI, Width, Height);
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

				RenderDevice.OnMouseButton(NkButton, (int)MousePos.X, (int)MousePos.Y, IsDown);
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
	}
}