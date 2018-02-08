using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libTech;
using libTech.UI;
using libTech.Graphics;
using OpenGL;
using System.Numerics;

namespace Game {
	public class Game : LibTechGame {
		ShaderProgram DefaultShader;
		VertexArray VertexArray;

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

			BufferObject VertexBuffer = new BufferObject();
			VertexBuffer.SetData(new Vector3[] { new Vector3(0, 0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, -0.5f, 0) });

			BufferObject ColorBuffer = new BufferObject();
			ColorBuffer.SetData(new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) });

			VertexArray = new VertexArray();

			uint PosAttrib = (uint)DefaultShader.GetAttribLocation("Pos");
			VertexArray.AttribFormat(PosAttrib);
			VertexArray.AttribBinding(PosAttrib, VertexArray.BindVertexBuffer(VertexBuffer));

			uint ClrAttrib = (uint)DefaultShader.GetAttribLocation("Clr");
			VertexArray.AttribFormat(ClrAttrib);
			VertexArray.AttribBinding(ClrAttrib, VertexArray.BindVertexBuffer(ColorBuffer));
		}
		
		public override void Update(float Dt) {
		}

		public override void Draw(float Dt) {
			Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			DefaultShader.Bind();
			VertexArray.Draw(0, 3);
		}
	}
}