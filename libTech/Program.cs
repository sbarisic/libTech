using CARP;
using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using libTech.FileSystem;
using libTech.Graphics;
using libTech.GUI;
using libTech.Importer;
using libTech.Reflection;
using libTech.Scripting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Color = FishGfx.Color;

namespace libTech {
	static partial class Engine {
		public static RenderWindow Window;
		public static NuklearGUI GUI;

		public static libGUI UI;

		public static VirtualFileSystem VFS;

		public static Camera Camera3D;
		public static Camera Camera2D;

		public static RenderTexture ScreenRT;
		public static RenderTexture GBuffer;
		internal static Mesh2D ScreenQuad;

		public static ConVar<int> MaxFPS;
		public static ConVar<string> GamePath;
		public static ConVar<int> WindowWidth;
		public static ConVar<int> WindowHeight;
		public static ConVar<bool> WindowResizable;
		public static ConVar<bool> WindowBorderless;
		public static ConVar<int> MSAA;
		public static ConVar<bool> ShowFPS;
		public static ConVar<string> SourceGameDirs;

		public static float Time;
		public static RunningAverage FrameTime = new RunningAverage(30);

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
		static Stopwatch TimeStopwatch;

		static List<string> FailedToLoadDLLs;

		internal static void Main(string[] args) {
			if (IntPtr.Size != 8)
				throw new Exception("x86 not supported");

			if (!Kernel32.SetDllDirectory("native"))
				throw new Win32Exception();

			/*
			{
				Console.WriteLine("Waiting for RenderDoc ...");
				Console.ReadLine();
			}
			//*/

			FailedToLoadDLLs = new List<string>();

			AppDomain.CurrentDomain.UnhandledException += (S, E) => {
				if (!Debugger.IsAttached)
					Engine.LogFatal((E.ExceptionObject ?? "Unknown unhandled exception object").ToString());
			};

			//AppDomain.CurrentDomain.AssemblyResolve += (S, E) => TryLoadAssembly(E.Name, DllDirectory);

			if (Engine.LogFatal(RunGame)) {
				Console.WriteLine("\n\nENGINE TERMINATED UNEXPECTEDLY");

				while (true)
					Thread.Sleep(10);
			}
		}

		static void InitConsole() {
			Engine.GamePath = ConVar.Register("game", "basegame", ConVarType.Replicated | ConVarType.Init);
			//Engine.GamePath = ConVar.Register("game", "legprocessor", ConVarType.Replicated | ConVarType.Init);

			Engine.MaxFPS = ConVar.Register("maxfps", 60, ConVarType.Archive);
			Engine.WindowWidth = ConVar.Register("width", 1366, ConVarType.Archive);
			Engine.WindowHeight = ConVar.Register("height", 768, ConVarType.Archive);

			//Engine.WindowWidth = CVar.Register("width", 800, CVarType.Archive);
			//Engine.WindowHeight = CVar.Register("height", 600, CVarType.Archive);

			Engine.WindowBorderless = ConVar.Register("borderless", false, ConVarType.Archive);
			Engine.WindowResizable = ConVar.Register("resizable", false, ConVarType.Archive);
			Engine.ShowFPS = ConVar.Register("showfps", true, ConVarType.Archive);

			Engine.MSAA = ConVar.Register("msaa", 32, ConVarType.Archive);

			Engine.SourceGameDirs = ConVar.Register("source_game_dirs", "C:/Program Files (x86)/Steam/steamapps/common/GarrysMod", ConVarType.Archive);

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
			string GameDllPath = Path.Combine(BasePath, "Game.dll");

			Assembly GameAssembly = Reflect.LoadAssembly(GameDllPath);
			Type[] GameImplementations = Reflect.GetAllImplementationsOf(GameAssembly, typeof(LibTechGame)).ToArray();

			if (GameImplementations.Length == 0) {
				GConsole.WriteLine("No game implementations found in " + GameDllPath);
				Environment.Exit(0);
			} else if (GameImplementations.Length > 1) {
				GConsole.WriteLine("Too many game implementations in " + GameDllPath);
				Environment.Exit(0);
			}

			AppDomain.CurrentDomain.AssemblyResolve += (S, E) => TryLoadAssembly(E.Name, GameDllPath);
			Importers.RegisterAll(GameAssembly);

			Game = (LibTechGame)Activator.CreateInstance(GameImplementations[0]);
			Game.Load();
		}

		static void RunGame() {
			TimeStopwatch = Stopwatch.StartNew();
			InitConsole();

			Engine.VFS = new VirtualFileSystem(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			Engine.VFS.Mount("/content/", "./content");
			Engine.VFS.Mount("/materials/", "C:/Program Files (x86)/Steam/steamapps/common/GarrysMod/garrysmod/addons/quake_3_gmod_160207505/materials");

			string[] SourceGameDirs = Engine.SourceGameDirs.Value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Where(Pth => Directory.Exists(Pth)).ToArray();

			if (SourceGameDirs.Length > 0) {
				foreach (var GameDir in SourceGameDirs)
					Engine.VFS.GetSourceProvider().AddRoot(GameDir);
			}

			if (Directory.Exists(Path.Combine(Engine.GamePath, "content")))
				Engine.VFS.Mount("/content/", Path.Combine(Engine.GamePath, "content"));

			List<string> ZipResources = new List<string>();
			ZipResources.AddRange(Engine.VFS.GetFiles("/content/").Where(P => Path.GetExtension(P) == ".pk3" || Path.GetExtension(P) == ".zip"));

			foreach (var ZipResource in ZipResources)
				Engine.VFS.MountArchive("/content/", ZipResource);

			FileWatcher.Init("content");
			Importers.RegisterAll(Reflect.GetExeAssembly());

			Engine.GUI = new NuklearGUI();

			Engine.Window = new RenderWindow(Engine.WindowWidth, Engine.WindowHeight, "libTech", Engine.WindowResizable);
			Engine.Window.OnMouseMove += Engine.GUI.OnMouseMove;
			Engine.Window.OnKey += OnKey;
			Engine.Window.OnChar += Engine.GUI.OnChar;

			GConsole.Init();
			GConsole.WriteLine("Running {0}", RenderAPI.Renderer, RenderAPI.Version);

			// Screen framebuffer
			OpenGL.Gl.Get(OpenGL.Gl.MAX_SAMPLES, out int MaxMSAA);
			if (Engine.MSAA > MaxMSAA)
				Engine.MSAA.Value = MaxMSAA;

			{
				Engine.GBuffer = new RenderTexture(Engine.Window.WindowWidth, Engine.Window.WindowHeight, IsGBuffer: true);
				Engine.ScreenRT = new RenderTexture(Engine.Window.WindowWidth, Engine.Window.WindowHeight, Engine.MSAA);

				Engine.ScreenQuad = new Mesh2D();
				Engine.ScreenQuad.PrimitiveType = PrimitiveType.Triangles;
				Engine.ScreenQuad.SetVertices(Utils.EmitRectangleTris(new Vertex2[6], 0, 0, 0, Engine.Window.WindowWidth, Engine.Window.WindowHeight, 0, 0, 1, 1, Color.White));
			}

			Engine.GUI.Init(Engine.Window, new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/gui.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/gui.frag")));

			Engine.UI = new libGUI(Engine.Window);
			Lua.Init();

			Engine.LoadShaders();
			Engine.LoadMaterials();

			GConsole.Color = FishGfx.Color.Orange;
			foreach (var DllName in FailedToLoadDLLs)
				GConsole.WriteLine("Failed to load '{0}'", DllName);
			GConsole.Color = FishGfx.Color.White;

			// Graphics init
			Gfx.ShadersDirectory = "content/shaders";
			//Gfx.Line3D = DefaultShaders.Line3D;
			//Gfx.Point3D = DefaultShaders.Point3D;
			//Gfx.Default3D = DefaultShaders.DefaultColor3D;

			// Camera init
			Engine.Camera2D = new Camera();
			Engine.Camera2D.SetOrthogonal(0, 0, Engine.Window.WindowWidth, Engine.Window.WindowHeight);

			Engine.Camera3D = new Camera();
			Engine.Camera3D.SetPerspective(Engine.Window.WindowWidth, Engine.Window.WindowHeight, FarPlane: 16000);

			LoadGameDll(Engine.GamePath);
			Stopwatch SWatch = Stopwatch.StartNew();

			int MaxFPS = Engine.MaxFPS;
			if (MaxFPS <= 0)
				MaxFPS = 900;

			float FrameCap = 1.0f / MaxFPS;
			float Dt = 1.0f / 60.0f;

			while (!Engine.Window.ShouldClose) {
				Engine.FrameTime.Push(Dt);

				Update(Dt);
				Draw(Dt);

				// TODO: Move frame cap somewhere else
				while ((SWatch.ElapsedMilliseconds / 1000.0f) < FrameCap)
					Thread.Sleep(0);

				Dt = SWatch.ElapsedMilliseconds / 1000.0f;
				SWatch.Restart();
			}
		}

		static void Update(float Dt) {
			Events.Poll();
			Engine.Time = TimeStopwatch.ElapsedMilliseconds / 1000.0f;

			Engine.UI.Update(Dt);
			Game.Update(Dt);
			GConsole.Update();
		}

		static void Draw(float Dt) {


			Engine.GBuffer.Bind();
			{
				RenderState RS = Gfx.PeekRenderState();
				RS.EnableBlend = false;
				Gfx.PushRenderState(RS);
				Gfx.Clear();

				//Engine.Framebuffer3D.Clear();

				ShaderUniforms.Current.Camera = Engine.Camera3D;
				Game.DrawOpaque();

				Gfx.PopRenderState();
			}
			Engine.GBuffer.Unbind();

			/*FishGfx.Color[] Colors;
			Colors = Engine.GBuffer.Position.GetPixels();
			Colors = Engine.GBuffer.Normal.GetPixels();*/

			//Engine.Framebuffer3D.Blit();

			ShaderUniforms.Current.Camera = Engine.Camera2D;
			RenderState State = Gfx.PeekRenderState();
			State.EnableDepthTest = false;
			Gfx.PushRenderState(State);
			{
				Engine.ScreenRT.Bind();
				{
					ShaderProgram DefaultShader = Engine.GetShader("deferred_shading");
					DefaultShader.Uniform3f("ViewPos", Engine.Camera3D.Position);

					State = Gfx.PeekRenderState();
					State.FrontFace = FrontFace.CounterClockwise;
					State.EnableBlend = false;
					Gfx.PushRenderState(State);
					{
						Engine.GBuffer.Color.BindTextureUnit(0);
						Engine.GBuffer.Position.BindTextureUnit(1);
						Engine.GBuffer.Normal.BindTextureUnit(2);
						{
							DefaultShader.Bind(ShaderUniforms.Current);
							Engine.ScreenQuad.Draw();
							DefaultShader.Unbind();
						}
						Engine.GBuffer.Normal.UnbindTextureUnit(2);
						Engine.GBuffer.Position.UnbindTextureUnit(1);
						Engine.GBuffer.Color.UnbindTextureUnit(0);
					}
					Gfx.PopRenderState();

					Game.DrawTransparent();
				}
				Engine.ScreenRT.Unbind();

				Gfx.Clear();
				ShaderUniforms.Current.TextureSize = Engine.ScreenRT.Color.Size;
				ShaderUniforms.Current.MultisampleCount = Engine.ScreenRT.Color.Multisamples;
				Gfx.TexturedRectangle(0, 0, Engine.Window.WindowWidth, Engine.Window.WindowHeight, Texture: Engine.ScreenRT.Color, Shader: Engine.GetShader("framebuffer"));

				Engine.GUI.Draw(() => {
					Engine.UI.Draw();
					Game.DrawGUI(Dt);
				});//*/

			}
			Gfx.PopRenderState();
			Engine.Window.SwapBuffers();
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

			Engine.GUI.OnKey(Wnd, Key, Scancode, Pressed, Repeat, Mods);
		}
	}
}
