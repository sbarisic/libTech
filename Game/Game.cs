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

		public override void Load() {
			DefaultCam = new Camera();
			DefaultCam.Position = new Vector3(0, 10, 100);
			DefaultCam.SetPerspective(Engine.Width, Engine.Height, 90 * 3.1415926535f / 180);

			ShaderProgram DefaultShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"),
				new ShaderStage(ShaderType.FragmentShader, "content/shaders/default.frag"));

			ShaderProgram NoTexShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"),
				new ShaderStage(ShaderType.FragmentShader, "content/shaders/default_notex.frag"));

			ShaderProgram PointShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/point.vert"),
				new ShaderStage(ShaderType.FragmentShader, "content/shaders/point.frag"));

			ShaderProgram LineShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/line.vert"),
				new ShaderStage(ShaderType.GeometryShader, "content/shaders/line.geom"),
				new ShaderStage(ShaderType.FragmentShader, "content/shaders/line.frag"));

			SampleModel = Importers.Load<Model>("content/models/skull.obj");
			SampleModel.Scale = new Vector3(50);
			SampleModel.Position = new Vector3(0, 0, 0);
			SampleModel.ShaderProgram = DefaultShader;
			SampleModel.Meshes[0].Material.Diffuse = new Texture(Image.FromFile("content/textures/difuso_flip_oscuro.jpg"));

			Immediate.TriangleShader = NoTexShader;
			Immediate.LineShader = LineShader;
			Immediate.PointShader = PointShader;
		}
		
		float MX;
		float MY;

		public override void Update(float Dt) {
			if (!GConsole.Open) {
				const float Scale = 50;

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

				//MX = -Engine.MousePos.X;
				//MY = -Engine.MousePos.Y;
				//const float MouseScale = 1.0f / 250;
				const float MouseScale = 1;

				float MouseVal = 1;
				if (Engine.GetKey(Glfw3.Glfw.KeyCode.Left))
					MX += MouseVal * Dt;
				if (Engine.GetKey(Glfw3.Glfw.KeyCode.Right))
					MX += -MouseVal * Dt;
				if (Engine.GetKey(Glfw3.Glfw.KeyCode.Up))
					MY += MouseVal * Dt;
				if (Engine.GetKey(Glfw3.Glfw.KeyCode.Down))
					MY += -MouseVal * Dt;

				Matrix4 Rot = Matrix4.CreateRotationX(MY * MouseScale) * Matrix4.CreateRotationY(MX * MouseScale);
				DefaultCam.Rotation = Quaternion.CreateFromRotationMatrix(Rot);
			}
		}

		Vector3 Pos = Vector3.Zero;
		Vector3 Scale = new Vector3(50);
		Quaternion Rot = Quaternion.Identity;

		public override void Draw(float Dt) {
			Camera.ActiveCamera = DefaultCam;

			SampleModel.Meshes[0].Material.DiffuseColor = new Vector4(1, 1, 1, 1);
			SampleModel.Meshes[0].Wireframe = false;
			SampleModel.Draw();

			Gl.Disable(EnableCap.CullFace);
			Gl.DepthMask(false);
			SampleModel.DrawTransparent();
			Gl.DepthMask(true);

			Immediate.Axes(Vector3.Zero, 100);
			Immediate.Line(Vector3.Zero, SampleModel.Position, Vector4.One);
			Immediate.GizmoInput(Engine.MousePos, Engine.GetMouseButton(Glfw3.Glfw.MouseButton.ButtonLeft), Engine.GetKey(Glfw3.Glfw.KeyCode.T),
				Engine.GetKey(Glfw3.Glfw.KeyCode.R), Engine.GetKey(Glfw3.Glfw.KeyCode.Y), Engine.GetKey(Glfw3.Glfw.KeyCode.L), Engine.GetKey(Glfw3.Glfw.KeyCode.LeftShift));

			Gl.Disable(EnableCap.DepthTest);
			if (Immediate.Gizmo(Dt, ref Pos, ref Rot, ref Scale, new Vector3(10, (45.0f / 2) * (float)Math.PI / 180, 10))) {
				SampleModel.Position = Pos;
				SampleModel.Rotation = Rot;
				SampleModel.Scale = Scale;
			}
		}
	}
}