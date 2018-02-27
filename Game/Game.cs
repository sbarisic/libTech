using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using libTech;
using libTech.UI;
using libTech.Graphics;
using OpenGL;
using System.Numerics;
using Matrix4 = System.Numerics.Matrix4x4;
using libTech.Importer;
using libTechGeometry;
using libTechGeometry.ConstructiveSolidGeometry;
using libTech.libNative;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Game {
	public unsafe class Game : LibTechGame {
		Camera DefaultCam;
		Model SampleModel;
		Texture CrosshairTex;
		ShaderProgram DefaultShader, MSDF;

		MSDFFont FontTest;
		string SomeString;

		public override void Load() {
			DefaultCam = new Camera();
			DefaultCam.Position = new Vector3(0, 10, 100);
			DefaultCam.SetPerspective(Engine.Width, Engine.Height, 90 * 3.1415926535f / 180);
			DefaultCam.MouseMovement = true;

			FontTest = new MSDFFont("content/fonts/Hack.ttf");
			SomeString = "The quick brown";

			CrosshairTex = Importers.Load<Texture>("content/textures/crosshair_default.png");
			/*SampleMSDF = Importers.Load<Texture>("content/textures/msdf_test.png");
			SampleMSDF.SetFilter(Gl.LINEAR, Gl.LINEAR);
			SampleMSDF.SetWrap(Gl.REPEAT);*/

			DefaultShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"),
			   new ShaderStage(ShaderType.FragmentShader, "content/shaders/default.frag"));

			MSDF = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"),
			   new ShaderStage(ShaderType.FragmentShader, "content/shaders/msdf.frag"));

			ShaderProgram NoTexShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"),
				new ShaderStage(ShaderType.FragmentShader, "content/shaders/default_notex.frag"));

			ShaderProgram PointShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/point.vert"),
				new ShaderStage(ShaderType.FragmentShader, "content/shaders/point.frag"));

			ShaderProgram LineShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/line.vert"),
				new ShaderStage(ShaderType.GeometryShader, "content/shaders/line.geom"),
				new ShaderStage(ShaderType.FragmentShader, "content/shaders/line.frag"));

			SampleModel = Importers.Load<Model>("content/models/sandbox.obj");
			SampleModel.Scale = new Vector3(1);
			SampleModel.Position = new Vector3(0, 0, 0);
			SampleModel.ShaderProgram = DefaultShader;
			//SampleModel.Meshes[0].Material.Diffuse = Importers.Load<Texture>("content/textures/difuso_flip_oscuro.jpg");

			Immediate.TriangleShader = NoTexShader;
			Immediate.LineShader = LineShader;
			Immediate.PointShader = PointShader;

			GConsole.RegisterCommand("getcam", () => {
				GConsole.WriteLine("Pos: " + DefaultCam.Position.ToString());
				GConsole.WriteLine("Rot: " + DefaultCam.Rotation.ToString());
			});

			//Pos = Vector3.Zero;
			//Rot = Quaternion.Identity;
			//Scl = Vector3.One;

			// Pos: <250,4927. 204,3378. 42,06482>
			// Rot: { X:  Y:  Z:  W: }

			/*Pos = new Vector3(250.4927f, 204.3378f, 42.06482f);
			Rot = new Quaternion(-2.848585E-07f, -0.6858193f, 2.684378E-07f, -0.7277719f);
			Scl = Vector3.One;
			Mat = Camera.CreateModel(Pos, Scl, Rot);*/
			GizmoEnabled = false;
		}

		public override void Update(float Dt) {
			if (!GConsole.Open) {
				Engine.CaptureMouse(true);
				DefaultCam.Update(Engine.MouseDelta);

				const float Scale = 100;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.W))
					DefaultCam.Position += DefaultCam.WorldForwardNormal * Dt * Scale;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.A))
					DefaultCam.Position -= DefaultCam.WorldRightNormal * Dt * Scale;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.S))
					DefaultCam.Position -= DefaultCam.WorldForwardNormal * Dt * Scale;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.D))
					DefaultCam.Position += DefaultCam.WorldRightNormal * Dt * Scale;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.Space))
					DefaultCam.Position += DefaultCam.WorldUpNormal * Dt * Scale;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.LeftControl))
					DefaultCam.Position -= DefaultCam.WorldUpNormal * Dt * Scale;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.N))
					GizmoEnabled = true;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.M))
					GizmoEnabled = false;
			} else
				Engine.CaptureMouse(false);
		}

		bool GizmoEnabled;
		Vector3 Pos, Scl;
		Quaternion Rot;
		Matrix4 Mat;

		public override void Draw(float Dt) {
			Camera.ActiveCamera = DefaultCam;
			SampleModel.Draw();

			Gl.Disable(EnableCap.CullFace);
			Gl.DepthMask(false);
			SampleModel.DrawTransparent();

			/*Immediate.UseShaders(() => {
				Vector2 Scale = new Vector2(1);
				Immediate.TriangleShader = MSDF;
				Immediate.Matrix = Mat;

				FontTest.GetGlyphsForString(SomeString, (Tex, X, Y) => {
					if (Tex == null)
						return;

					Immediate.Texture2D(new Vector2(X, Y) * Scale, Tex, Scale: Scale);
				});

				Immediate.Matrix = Matrix4.Identity;
			});*/
			Gl.DepthMask(true);

			/*Immediate.Axes(Vector3.Zero, 100);
			Immediate.Line(Vector3.Zero, SampleModel.Position, Vector4.One);
			Immediate.GizmoInput(Engine.WindowSize / 2, Engine.GetMouseButton(Glfw3.Glfw.MouseButton.ButtonLeft), Engine.GetKey(Glfw3.Glfw.KeyCode.T),
				Engine.GetKey(Glfw3.Glfw.KeyCode.R), Engine.GetKey(Glfw3.Glfw.KeyCode.Y), Engine.GetKey(Glfw3.Glfw.KeyCode.L), Engine.GetKey(Glfw3.Glfw.KeyCode.LeftShift));

			Gl.Disable(EnableCap.DepthTest);
			if (Immediate.Gizmo(Dt, ref SampleModel.Position, ref SampleModel.Rotation, ref SampleModel.Scale, new Vector3(10, (45.0f / 2) * (float)Math.PI / 180, 10))) {
			}*/

			if (GizmoEnabled) {
				Gl.Clear(ClearBufferMask.DepthBufferBit);

				Immediate.GizmoInput(Engine.WindowSize / 2, Engine.GetMouseButton(Glfw3.Glfw.MouseButton.ButtonLeft), Engine.GetKey(Glfw3.Glfw.KeyCode.T),
					Engine.GetKey(Glfw3.Glfw.KeyCode.R), Engine.GetKey(Glfw3.Glfw.KeyCode.Y), Engine.GetKey(Glfw3.Glfw.KeyCode.L), Engine.GetKey(Glfw3.Glfw.KeyCode.LeftShift));

				if (Immediate.Gizmo(Dt, ref Pos, ref Rot, ref Scl))
					Mat = Camera.CreateModel(Pos, Scl, Rot);
			}
		}

		public override void DrawGUI(float Dt) {
			Immediate.UseShaders(() => {
				Immediate.TriangleShader = DefaultShader;
				Immediate.Texture2D(Engine.WindowSize / 2, CrosshairTex, true);
				
				Immediate.TriangleShader = MSDF;
				Gl.BlendFunc(BlendingFactor.SrcColor, BlendingFactor.One);

				Vector2 Scale = new Vector2(1);
				FontTest.GetGlyphsForString(SomeString, (Tex, X, Y) => {
					if (Tex == null)
						return;

					Immediate.Texture2D(new Vector2(X, Y) * Scale, Tex, Scale: Scale);
				});

				Gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			});
		}
	}
}