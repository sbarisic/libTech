using CARP;
using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using libTech.Entities;
using libTech.FileSystem;
using libTech.Graphics;
using libTech.GUI;
using libTech.Importer;
using libTech.Materials;
using libTech.Models;
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
		internal static libTechMesh PointLightMesh;

		public static ConVar<int> MaxFPS;
		public static ConVar<string> GamePath;
		public static ConVar<int> WindowWidth;
		public static ConVar<int> WindowHeight;
		public static ConVar<bool> WindowResizable;
		public static ConVar<bool> WindowBorderless;
		public static ConVar<int> MSAA;
		public static ConVar<bool> ShowFPS;
		public static ConVar<string> SourceGameDirs;
		public static ConVar<bool> DebugDraw;

		public static Map.libTechMap Map;

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

			Engine.MaxFPS = ConVar.Register("maxfps", 200, ConVarType.Archive);
			Engine.WindowWidth = ConVar.Register("width", 1366, ConVarType.Archive);
			Engine.WindowHeight = ConVar.Register("height", 768, ConVarType.Archive);

			//Engine.WindowWidth = CVar.Register("width", 800, CVarType.Archive);
			//Engine.WindowHeight = CVar.Register("height", 600, CVarType.Archive);

			Engine.WindowBorderless = ConVar.Register("borderless", false, ConVarType.Archive);
			Engine.WindowResizable = ConVar.Register("resizable", false, ConVarType.Archive);
			Engine.ShowFPS = ConVar.Register("showfps", true, ConVarType.Archive);

			Engine.MSAA = ConVar.Register("msaa", 32, ConVarType.Archive);

			Engine.SourceGameDirs = ConVar.Register("source_game_dirs", "C:/Program Files (x86)/Steam/steamapps/common/GarrysMod", ConVarType.Archive);
			Engine.DebugDraw = ConVar.Register("debugdraw", true, ConVarType.Cheat);

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
				Engine.GBuffer.Framebuffer.SetLabel(OpenGL.ObjectIdentifier.Framebuffer, "GBuffer");

				Engine.ScreenRT = new RenderTexture(Engine.Window.WindowWidth, Engine.Window.WindowHeight);
				Engine.ScreenRT.Framebuffer.SetLabel(OpenGL.ObjectIdentifier.Framebuffer, "ScreenRT");

				Engine.ScreenQuad = new Mesh2D();
				Engine.ScreenQuad.PrimitiveType = PrimitiveType.Triangles;
				Engine.ScreenQuad.SetVertices(Utils.EmitRectangleTris(new Vertex2[6], 0, 0, 0, Engine.Window.WindowWidth, Engine.Window.WindowHeight, 0, 0, 1, 1, Color.White));
				Engine.ScreenQuad.VAO.SetLabel(OpenGL.ObjectIdentifier.VertexArray, "Screen Quad");

				var Msh = FishGfx.Formats.Obj.Load("content/models/sphere_2.obj").First();
				Vertex3[] MshVerts = Msh.Vertices.ToArray();
				AABB PointLightBound = AABB.CalculateAABB(MshVerts.Select(V => V.Position));

				for (int i = 0; i < MshVerts.Length; i++)
					MshVerts[i].Position /= PointLightBound.Size;

				Engine.PointLightMesh = new libTechMesh();
				Engine.PointLightMesh.SetVertices(MshVerts);
				Engine.PointLightMesh.SetLabel("Point Light Volume");
			}



			Engine.GUI.Init(Engine.Window, new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/gui.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/gui.frag")));
			Engine.UI = new libGUI(Engine.Window);

			DbgDraw.Init();
			Lua.Init();

			Engine.LoadContent();

			GConsole.Color = Color.Orange;
			foreach (var DllName in FailedToLoadDLLs)
				GConsole.WriteLine("Failed to load '{0}'", DllName);
			GConsole.Color = Color.White;

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
			Engine.Map?.Update(Dt);
			GConsole.Update();
		}

		static void Draw(float Dt, bool AmbientLighting = true, bool PointLighting = true) {
			DbgDraw.Enabled = Engine.DebugDraw;

			ShaderUniforms.Current.Resolution = Engine.Window.WindowSize;
			Engine.GetTexture("skybox").BindTextureUnit(10);

			Engine.GBuffer.Bind();
			{
				RenderState RS = Gfx.PeekRenderState();
				RS.EnableBlend = false;
				Gfx.PushRenderState(RS);
				Gfx.Clear(Color.Transparent);

				//Engine.Framebuffer3D.Clear();

				ShaderUniforms.Current.Camera = Engine.Camera3D;

				Engine.Map?.DrawOpaque();
				Game.DrawOpaque();

				Gfx.PopRenderState();
			}
			Engine.GBuffer.Unbind();

			/*FishGfx.Color[] Colors;
			Colors = Engine.GBuffer.Position.GetPixels();
			Colors = Engine.GBuffer.Normal.GetPixels();*/

			//Engine.Framebuffer3D.Blit();

			Engine.ScreenRT.Bind();
			{
				Gfx.Clear(Color.Black);
				//Engine.Map?.DrawSkybox();

				Engine.GBuffer.Framebuffer.Blit(false, true, false, Destination: Engine.ScreenRT.Framebuffer);

				Engine.Map?.DrawSkybox();

				Engine.GBuffer.Color.BindTextureUnit(0);
				Engine.GBuffer.Position.BindTextureUnit(1);
				Engine.GBuffer.Normal.BindTextureUnit(2);
				Engine.GBuffer.DepthStencil.BindTextureUnit(3);

				{
					RenderState State = Gfx.PeekRenderState();
					State.FrontFace = FrontFace.CounterClockwise;
					State.EnableBlend = true;
					State.BlendFunc_Src = BlendFactor.SrcAlpha;
					State.BlendFunc_Dst = BlendFactor.One;
					State.EnableDepthTest = false;

					Gfx.PushRenderState(State);
					ShaderUniforms.Current.Camera = Engine.Camera2D;

					if (AmbientLighting) {
						// Ambient lighting
						ShaderProgram AmbientShader = Engine.GetShader("deferred_ambient");
						AmbientShader.Bind(ShaderUniforms.Current);
						Engine.ScreenQuad.Draw();
						AmbientShader.Unbind();
					}

					// Point lighting
					Gfx.PopRenderState();
					State.FrontFace = FrontFace.CounterClockwise;
					State.EnableDepthMask = false;
					State.EnableDepthTest = true;
					State.EnableStencilTest = true;
					Gfx.PushRenderState(State);
					ShaderUniforms.Current.Camera = Engine.Camera3D;

					if (Engine.Map != null && PointLighting) {
						DynamicLight[] Lights = Engine.Map.GetLights();

						for (int i = 0; i < Lights.Length; i++) {
							DbgDraw.DrawCross(Lights[i].Position);

							State.SetColorMask(false);
							State.EnableBlend = false;
							State.EnableDepthTest = true;
							State.EnableCullFace = false;
							State.StencilFunc(StencilFunction.Always, 0, 0);

							State.StencilFrontSFail = StencilOperation.Keep;
							State.StencilFrontDPFail = StencilOperation.IncrWrap;
							State.StencilFrontDPPass = StencilOperation.Keep;

							State.StencilBackSFail = StencilOperation.Keep;
							State.StencilBackDPFail = StencilOperation.DecrWrap;
							State.StencilBackDPPass = StencilOperation.Keep;

							State.DepthFunc = DepthFunc.LessOrEqual;

							Gfx.PushRenderState(State);
							Gfx.ClearStencil(0);

							DrawPointLightMask(Lights[i]);
							DrawPointLightShadow(Lights[i]);

							Gfx.PopRenderState();

							State.EnableBlend = true;
							State.SetColorMask(true);
							State.EnableDepthTest = false;
							State.EnableCullFace = true;
							State.StencilFunc(StencilFunction.Equal, 1, 0xFF);
							State.StencilOp(StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep);
							Gfx.PushRenderState(State);

							ShadePointLight(Lights[i]);

							Gfx.PopRenderState();
						}
					}

					Gfx.PopRenderState();
				}

				Engine.GBuffer.DepthStencil.UnbindTextureUnit(3);
				Engine.GBuffer.Normal.UnbindTextureUnit(2);
				Engine.GBuffer.Position.UnbindTextureUnit(1);
				Engine.GBuffer.Color.UnbindTextureUnit(0);

				Engine.Map?.DrawTransparent();
				Game.DrawTransparent();
				DbgDraw.FinalizeDraw((long)(Engine.Time * 1000));
			}
			Engine.ScreenRT.Unbind();

			{
				RenderState State = Gfx.PeekRenderState();
				State.EnableDepthTest = false;
				Gfx.PushRenderState(State);
				{
					Gfx.Clear();
					ShaderUniforms.Current.Camera = Engine.Camera2D;
					ShaderUniforms.Current.TextureSize = Engine.ScreenRT.Color.Size;
					ShaderUniforms.Current.MultisampleCount = Engine.ScreenRT.Color.Multisamples;
					Gfx.TexturedRectangle(0, 0, Engine.Window.WindowWidth, Engine.Window.WindowHeight, Texture: Engine.ScreenRT.Color, Shader: Engine.GetShader("framebuffer"));

					Engine.GUI.Draw(() => {
						float FT = Engine.FrameTime.Average();
						float FPS = 1.0f / FT;
						string DebugString = string.Format("{0} ms\n{1} FPS\n{2} Lights", FT, FPS, Engine.Map.GetLights().Length);
						int Lines = 3;

						Gfx.DrawText(Engine.UI.DebugFont, new Vector2(2, Engine.WindowHeight - Engine.UI.DebugFont.ScaledLineHeight * Lines - 2), DebugString, Color.White);

						Engine.UI.Draw();
						Game.DrawGUI(Dt);
					});//*/
				}
				Gfx.PopRenderState();
			}

			Engine.Window.SwapBuffers();
		}

		static void PreparePointLight(DynamicLight Light) {
			ShaderUniforms.Current.Model = Matrix4x4.CreateScale(Light.LightRadius) * Matrix4x4.CreateTranslation(Light.Position);
		}

		static void DrawPointLightShadow(DynamicLight Light) {
			if (!Light.CastShadows)
				return;

			ShaderMaterial ShadowVolume = (ShaderMaterial)Engine.GetMaterial("shadow_volume");

			RenderState RS = Gfx.PeekRenderState();
			RS.FrontFace = FrontFace.Clockwise;
			Gfx.PushRenderState(RS);

			Light.SetUniforms(ShadowVolume.Shader);
			//Engine.Map.DrawShadowVolume(ShadowVolume);
			Engine.Map.DrawEntityShadowVolume(Light, ShadowVolume);

			Gfx.PopRenderState();
		}

		static ShaderMaterial StencilMat;
		static void DrawPointLightMask(DynamicLight Light) {
			if (StencilMat == null)
				StencilMat = new ShaderMaterial("nop", Engine.GetShader("nop"));

			PreparePointLight(Light);
			Engine.PointLightMesh.Material = StencilMat;
			Engine.PointLightMesh.Draw();
		}

		static ShaderMaterial DeferredShadingMat;
		static void ShadePointLight(DynamicLight Light) {
			if (DeferredShadingMat == null)
				DeferredShadingMat = new ShaderMaterial("deferred_shading", Engine.GetShader("deferred_shading"));

			Light.SetUniforms(DeferredShadingMat.Shader);

			PreparePointLight(Light);
			Engine.PointLightMesh.Material = DeferredShadingMat;
			Engine.PointLightMesh.Draw();
		}

		/*static float CalcLightRadius(Color Clr, float Constant, float Linear, float Quadratic) {
			float LightMax = Math.Max(Math.Max(Clr.R, Clr.G), Clr.B) / 255.0f;
			return ((-Linear + (float)Math.Sqrt(Linear * Linear - 4 * Quadratic * (Constant - (256.0 / 5.0) * LightMax))) / (2 * Quadratic));
		}*/

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
