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

namespace Game {
	public class Game : LibTechGame {
		ShaderProgram DefaultShader;
		VertexArray VertexArray;
		Texture Tex;

		public override void Load() {
			ShaderStage DefaultVert = new ShaderStage(ShaderType.VertexShader);
			DefaultVert.SetSourceFile("content/shaders/default.vert");
			DefaultVert.Compile();

			ShaderStage DefaultFrag = new ShaderStage(ShaderType.FragmentShader);
			DefaultFrag.SetSourceFile("content/shaders/default.frag");
			DefaultFrag.Compile();

			DefaultShader = new ShaderProgram();
			DefaultShader.AttachShader(DefaultVert);
			DefaultShader.AttachShader(DefaultFrag);
			DefaultShader.Link();

			Image Img = Image.FromFile("content/textures/test.png");
			Tex = new Texture(Img.Width, Img.Height);
			Tex.SubImage2D(Img);

			BufferObject VertexBuffer = new BufferObject();
			VertexBuffer.SetData(new Vector3[] { new Vector3(0, 0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, -0.5f, 0) });

			BufferObject ColorBuffer = new BufferObject();
			ColorBuffer.SetData(new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) });

			BufferObject UVBuffer = new BufferObject();
			UVBuffer.SetData(new Vector2[] { new Vector2(0.5f, 1), new Vector2(1, 0), new Vector2(0, 0) });

			VertexArray = new VertexArray();

			uint PosAttrib = (uint)DefaultShader.GetAttribLocation("Pos");
			VertexArray.AttribFormat(PosAttrib);
			VertexArray.AttribBinding(PosAttrib, VertexArray.BindVertexBuffer(VertexBuffer));

			uint ClrAttrib = (uint)DefaultShader.GetAttribLocation("Clr");
			VertexArray.AttribFormat(ClrAttrib);
			VertexArray.AttribBinding(ClrAttrib, VertexArray.BindVertexBuffer(ColorBuffer));

			uint UVAttrib = (uint)DefaultShader.GetAttribLocation("UV");
			VertexArray.AttribFormat(UVAttrib, Size: 2);
			VertexArray.AttribBinding(UVAttrib, VertexArray.BindVertexBuffer(UVBuffer, Stride: 2 * sizeof(float)));
		}

		public override void Update(float Dt) {
		}

		public override void Draw(float Dt) {
			Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			Tex.BindTextureUnit();
			DefaultShader.Bind();



			VertexArray.Draw(0, 3);
		}
	}
}