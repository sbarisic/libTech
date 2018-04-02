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

namespace libTech {
	public unsafe static partial class Engine {
		internal static Glfw.Window Window;
		internal static RenderDevice RenderDevice;

		static int FPS;

		public static LibTechGame Game;
		public static int Width { get; private set; }
		public static int Height { get; private set; }

		public static Vector2 WindowSize {
			get {
				return new Vector2(Width, Height);
			}
		}

		public static Vector2 MousePos { get; private set; }
		public static Vector2 MouseDelta { get; private set; }

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

			CVar.Register("width", 1366, CVarType.Archive);
			CVar.Register("height", 768, CVarType.Archive);
			CVar.Register("borderless", false, CVarType.Archive);
			CVar.Register("resizable", false, CVarType.Archive);
			CVar.Register("gl_doublebuffer", true, CVarType.Archive);
			CVar.Register("gl_samples", 8, CVarType.Archive);

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

			FileWatcher.Init("content");
			Importers.RegisterAll(Reflect.GetExeAssembly());

			CreateContext();
			InitPhysics();

			LoadContent();

			Stopwatch SWatch = Stopwatch.StartNew();
			float Target = 1.0f / 120;
			float Dt = Target;

			TextureTarget Tgt = TextureTarget.Texture2dMultisample;
			if (CVar.GetInt("gl_samples") == 0)
				Tgt = TextureTarget.Texture2d;

			Texture ColorTex = new Texture(Width, Height, Tgt, 1, InternalFormat.Rgb16f);
			Texture DepthTex = new Texture(Width, Height, Tgt, 1, InternalFormat.Depth24Stencil8);

			Framebuffer FB = new Framebuffer();
			FB.AttachColorTexture(ColorTex);
			FB.AttachDepthTexture(DepthTex);

			Texture SkyboxCubeMap = new Texture(1024, 1024, TextureTarget.TextureCubeMap, 1, InternalFormat.Srgb8Alpha8);
			SkyboxCubeMap.SetFilter(Gl.LINEAR, Gl.LINEAR);
			SkyboxCubeMap.SubImage3D(Image.FromFile("content/textures/skybox/interstellar/front.png"), Z: Texture.FRONT);
			SkyboxCubeMap.SubImage3D(Image.FromFile("content/textures/skybox/interstellar/back.png"), Z: Texture.BACK);
			SkyboxCubeMap.SubImage3D(Image.FromFile("content/textures/skybox/interstellar/left.png"), Z: Texture.LEFT);
			SkyboxCubeMap.SubImage3D(Image.FromFile("content/textures/skybox/interstellar/right.png"), Z: Texture.RIGHT);
			SkyboxCubeMap.SubImage3D(Image.FromFile("content/textures/skybox/interstellar/top.png"), Z: Texture.TOP);
			SkyboxCubeMap.SubImage3D(Image.FromFile("content/textures/skybox/interstellar/bottom.png"), Z: Texture.BOTTOM);

			//FB.Clear(new Vector4(1, 0, 0, 1));

			Model SkyboxCube = Importers.Load<Model>("content/models/cube.obj");
			SkyboxCube.Scale = new Vector3(2);
			SkyboxCube.Meshes[0].Material.Shader = ShaderProgram.Skybox;
			SkyboxCube.Meshes[0].Material.Diffuse = SkyboxCubeMap;

			//bool DoDebug = true;

			while (!Glfw.WindowShouldClose(Window)) {
				/*if (Khronos.KhronosApi.LogEnabled) {
					Khronos.KhronosApi.LogCommand("END FRAME", null, null);
					Khronos.KhronosApi.LogEnabled = false;
				}

				if (DoDebug) {
					DoDebug = false;
					Khronos.KhronosApi.LogEnabled = true;
					Khronos.KhronosApi.LogCommand("BEGIN FRAME", null, null);
				}*/

				Update(Dt);
				Gl.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);

				FB.Bind();
				{
					Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
					SetupState();

					Gl.Disable(EnableCap.CullFace);
					Gl.DepthMask(false);
					SkyboxCube.Position = Camera.ActiveCamera?.Position ?? Vector3.Zero;
					SkyboxCube.Draw();
					Gl.DepthMask(true);

					DrawScene(Dt);
				}
				FB.Unbind();

				// Swap to GUI camera
				Gl.Disable(EnableCap.DepthTest);
				Gl.Disable(EnableCap.CullFace);
				Camera.ActiveCamera = Camera.GUICamera;

				Immediate.UseShaders(() => {
					if (ColorTex.Multisampled) {
						Immediate.TriangleShader = ShaderProgram.PostMultisample;
						Immediate.TriangleShader.Uniform2f("TexSize", new Vector2(ColorTex.Width, ColorTex.Height));
					} else
						Immediate.TriangleShader = ShaderProgram.Post;

					Immediate.TriangleShader.Uniform1f("Exposure", 1.0f);

					Gl.Enable(EnableCap.FramebufferSrgb);
					Immediate.Texture2D(Vector2.Zero, ColorTex, UVInvertY: true);
					Gl.Disable(EnableCap.FramebufferSrgb);
				});

				DrawGUI(Dt);
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

		static void Update(float Dt) {
			Glfw.PollEvents();

			Glfw.GetCursorPos(Window, out double CurX, out double CurY);
			Vector2 CurMousePos = new Vector2((float)CurX, (float)CurY);

			MouseDelta = CurMousePos - MousePos;
			MousePos = CurMousePos;

			if (DragDropPaths != null) {
				OnDragDrop?.Invoke(DragDropPaths);
				DragDropPaths = null;
			}

			UpdatePhysics(Dt);
			UpdateEntities(Dt);
			Game.Update(Dt);
		}

		static void SetupState() {
			Gl.Enable(EnableCap.DepthTest);
			Gl.DepthMask(true);

			Gl.Enable(EnableCap.CullFace);
			Gl.CullFace(CullFaceMode.Back);

			Gl.Enable(EnableCap.Blend);
			Gl.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
			Gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		}

		static void DrawScene(float Dt) {
			// Draw opaque entities
			DrawEntities(Dt);
			Game.Draw(Dt);

			// Draw transparent entities
			//Gl.Disable(EnableCap.CullFace);
			Gl.DepthMask(false);
			DrawTransparentEntities(Dt);
			Game.DrawTransparent(Dt);
			Gl.DepthMask(true);
		}

		static void DrawGUI(float Dt) {
			Game.DrawGUI(Dt);

			NuklearAPI.Frame(() => {
				GConsole.NuklearDraw(10, 10);
			});
		}
	}
}
