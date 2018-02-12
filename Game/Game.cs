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

namespace Game {
	public class Game : LibTechGame {
		Camera DefaultCam;
		ShaderProgram DefaultShader;

		Model SampleModel;

		public override void Load() {
			DefaultCam = new Camera();
			DefaultCam.Position = new Vector3(0, 0, 800);
			DefaultCam.SetPerspective(Engine.Width, Engine.Height, 3.141592653f / 3);
			DefaultCam.LookAt(new Vector3(0, Engine.Height / 4, 0), Vector3.UnitY);

			DefaultShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"),
				new ShaderStage(ShaderType.FragmentShader, "content/shaders/default.frag"));
			DefaultShader.UpdateCamera(DefaultCam);
			
			//SampleModel = Importers.Load<Model>("content/models/corsa.fbx");
			SampleModel = Importers.Load<Model>("content/models/skull.obj");
			SampleModel.Scale = new Vector3(300);
			SampleModel.Position = new Vector3(0, -100, 0);
			SampleModel.ShaderProgram = DefaultShader;
			SampleModel.Meshes[0].Material.Diffuse = new Texture(Image.FromFile("content/textures/difuso_flip_oscuro.jpg"));
		}

		public override void Update(float Dt) {
			SampleModel.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -Engine.TimeSinceStart);
		}

		public override void Draw(float Dt) {
			SampleModel.Draw();

			Gl.DepthMask(false);
			SampleModel.DrawTransparent();
			Gl.DepthMask(true);
		}
	}
}