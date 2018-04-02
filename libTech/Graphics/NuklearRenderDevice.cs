using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libTech.Reflection;
using libTech.Importer;
using System.Reflection;
using CARP;
using System.IO;
using System.Numerics;
using Matrix4 = System.Numerics.Matrix4x4;

using OpenGL;
using NuklearDotNet;
using System.Runtime.InteropServices;

namespace libTech.Graphics {
	internal unsafe class RenderDevice : NuklearDeviceTex<Texture> {
		VertexArray VertexArray;
		BufferObject Vertices, Colors, UVs, Elements;
		ShaderProgram Shader;

		public RenderDevice(ShaderProgram ShaderProg, int Width, int Height) {
			Shader = ShaderProg;
			VertexArray = new VertexArray();

			Elements = new BufferObject();
			VertexArray.BindElementBuffer(Elements);

			Vertices = new BufferObject();
			uint PosAttrib = (uint)ShaderProg.GetAttribLocation("Pos");
			VertexArray.AttribFormat(PosAttrib, Size: 2);
			VertexArray.AttribBinding(PosAttrib, VertexArray.BindVertexBuffer(Vertices, Stride: 2 * sizeof(float)));

			Colors = new BufferObject();
			uint ClrAttrib = (uint)ShaderProg.GetAttribLocation("Clr");
			VertexArray.AttribFormat(ClrAttrib, Size: 4);
			VertexArray.AttribBinding(ClrAttrib, VertexArray.BindVertexBuffer(Colors, Stride: 4 * sizeof(float)));

			UVs = new BufferObject();
			uint UVAttrib = (uint)ShaderProg.GetAttribLocation("UV");
			VertexArray.AttribFormat(UVAttrib, Size: 2);
			VertexArray.AttribBinding(UVAttrib, VertexArray.BindVertexBuffer(UVs, Stride: 2 * sizeof(float)));
		}

		public override Texture CreateTexture(int W, int H, IntPtr Data) {
			Texture T = new Texture(W, H, MipLevels: 1);
			T.SubImage(Data, 0, 0, 0, W, H, 0);
			return T;
		}

		public override void SetBuffer(NkVertex[] VertexBuffer, ushort[] IndexBuffer) {
			// TODO: Put NkVertex in a single buffer directly
			Vector4[] ColorArr = new Vector4[VertexBuffer.Length];
			Vector2[] VertArr = new Vector2[VertexBuffer.Length];
			Vector2[] UVArr = new Vector2[VertexBuffer.Length];

			for (int i = 0; i < VertexBuffer.Length; i++) {
				NkColor C = VertexBuffer[i].Color;
				NkVector2f Pos = VertexBuffer[i].Position;
				NkVector2f UV = VertexBuffer[i].UV;

				ColorArr[i] = new Vector4(C.R / 255.0f, C.G / 255.0f, C.B / 255.0f, C.A / 255.0f);
				VertArr[i] = new Vector2(Pos.X, Pos.Y);
				UVArr[i] = new Vector2(UV.X, UV.Y);
			}

			Colors.SetData(ColorArr);
			Vertices.SetData(VertArr);
			UVs.SetData(UVArr);
			Elements.SetData(IndexBuffer);
		}

		public override void Render(NkHandle Userdata, Texture Texture, NkRect ClipRect, uint Offset, uint Count) {
			Camera.ActiveCamera = Camera.GUICamera;

			Texture.BindTextureUnit();
			//Shader.UpdateCamera(GUICam);
			Shader.SetModelMatrix(Matrix4.Identity);
			Shader.Bind();

			Gl.Enable(EnableCap.ScissorTest);
			Gl.Scissor((int)ClipRect.X, (int)Camera.GUICamera.ViewportSize.Y - (int)ClipRect.Y - (int)ClipRect.H, (int)ClipRect.W, (int)ClipRect.H);

			VertexArray.DrawElements((int)(Offset * sizeof(ushort)), (int)Count);

			Gl.Disable(EnableCap.ScissorTest);
		}
	}
}
