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

using NuklearDotNet;
using System.Runtime.InteropServices;

namespace libTech.Renderer {
	/*
	internal unsafe class RenderDevice : NuklearDeviceTex<Texture2D>, IFrameBuffered {
		Engine Engine;

		VertexDeclaration NuklearVertexDecl;

		IUltravioletWindow Wind;
		IUltravioletGraphics Graphics;
		RenderTarget2D RenderTarget;
		RenderBuffer2D ColorBuffer;
		RenderBuffer2D DepthStencilBuffer;
		SpriteBatch Batch;

		DepthStencilState DepthStencil;
		BlendState Blending;
		RasterizerState Rasterizing;
		Viewport VPort;

		BasicEffect Effect;
		GeometryStream GeomStream;
		DynamicVertexBuffer VertexBuffer;
		DynamicIndexBuffer IndexBuffer;

		public RenderDevice(Engine Engine) {
			this.Engine = Engine;

			NuklearVertexDecl = new VertexDeclaration(new VertexElement[] {
					new VertexElement((int)Marshal.OffsetOf(typeof(NkVertex), nameof(NkVertex.Position)), VertexFormat.Vector2, VertexUsage.Position, 0),
					new VertexElement((int)Marshal.OffsetOf(typeof(NkVertex), nameof(NkVertex.UV)), VertexFormat.Vector2, VertexUsage.TextureCoordinate, 0),
					new VertexElement((int)Marshal.OffsetOf(typeof(NkVertex), nameof(NkVertex.Color)), VertexFormat.Color, VertexUsage.Color, 0)
			});
		}

		public override void Init() {
			Wind = Engine.Ultraviolet.GetPlatform().Windows.First();
			Graphics = Engine.Ultraviolet.GetGraphics();

			Batch = SpriteBatch.Create();

			RenderTarget = RenderTarget2D.Create(Wind.WindowedClientSize.Width, Wind.WindowedClientSize.Height, RenderTargetUsage.DiscardContents);

			ColorBuffer = RenderBuffer2D.Create(RenderBufferFormat.Color, RenderTarget.Width, RenderTarget.Height);
			RenderTarget.Attach(ColorBuffer);

			DepthStencilBuffer = RenderBuffer2D.Create(RenderBufferFormat.Depth24Stencil8, RenderTarget.Width, RenderTarget.Height);
			RenderTarget.Attach(DepthStencilBuffer);

			DepthStencil = DepthStencilState.Create();
			DepthStencil.StencilEnable = false;
			DepthStencil.DepthBufferEnable = false;

			Blending = BlendState.AlphaBlend;

			Rasterizing = RasterizerState.Create();
			Rasterizing.ScissorTestEnable = true;
			Rasterizing.CullMode = CullMode.None;
			Rasterizing.FillMode = FillMode.Solid;

			VPort = new Viewport(0, 0, Wind.ClientSize.Width, Wind.ClientSize.Height);

			Effect = BasicEffect.Create();
			Effect.World = Matrix.Identity;
			Effect.View = Matrix.Identity;
			Effect.Projection = Matrix.CreateOrthographicOffCenter(0, Wind.ClientSize.Width, Wind.ClientSize.Height, 0, 1, -1);

			Effect.VertexColorEnabled = true;
			Effect.TextureEnabled = true;

			EnsureBufferSize();
		}

		void EnsureBufferSize(int MinVert = 4096, int MinInd = 4096) {
			// TODO: Properly dispose
			bool GeometryDirty = false;

			if ((VertexBuffer != null && VertexBuffer.VertexCount < MinVert) || VertexBuffer == null) {
				VertexBuffer = DynamicVertexBuffer.Create(NuklearVertexDecl, MinVert);
				GeometryDirty = true;
			}

			if ((IndexBuffer != null && IndexBuffer.IndexCount < MinInd) || IndexBuffer == null) {
				IndexBuffer = DynamicIndexBuffer.Create(IndexBufferElementType.Int16, MinInd);
				GeometryDirty = true;
			}

			if (GeometryDirty || GeomStream == null) {
				GeomStream = GeometryStream.Create();
				GeomStream.Attach(VertexBuffer);
				GeomStream.Attach(IndexBuffer);
			}
		}

		public override Texture2D CreateTexture(int W, int H, IntPtr Data) {
			Color[] ColorData = new Color[W * H];

			for (int y = 0; y < H; y++)
				for (int x = 0; x < W; x++) {
					NkColor C = ((NkColor*)Data)[y * W + x];
					ColorData[y * W + x] = new Color(C.R / 255.0f, C.G / 255.0f, C.B / 255.0f, C.A / 255.0f);
				}

			Surface2D Surf = Surface2D.Create(W, H);
			Surf.SetData(ColorData);
			return Surf.CreateTexture();
		}

		public override void SetBuffer(NkVertex[] Verts, ushort[] Inds) {
			EnsureBufferSize(Verts.Length, Inds.Length);
			
			IndexBuffer.SetData(Inds);
			VertexBuffer.SetData(Verts);
		}

		public void BeginBuffering() {
			Graphics.SetRenderTarget(RenderTarget, Color.Transparent);

			Graphics.SetBlendState(Blending);
			Graphics.SetDepthStencilState(DepthStencil);

			Graphics.SetViewport(VPort);
			Graphics.SetRasterizerState(Rasterizing);
			Graphics.SetGeometryStream(GeomStream);
		}

		public override void Render(NkHandle Userdata, Texture2D Texture, NkRect ClipRect, uint Offset, uint Count) {
			Effect.Texture = Texture;

			foreach (var Pass in Effect.CurrentTechnique.Passes) {
				Pass.Apply();

				gl.PolygonMode(gl.GL_FRONT_AND_BACK, gl.GL_LINE);

				Graphics.SetScissorRectangle((int)ClipRect.X, (int)ClipRect.Y, (int)ClipRect.W, (int)ClipRect.H);
				Graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, (int)Offset, (int)Count / 3);

				gl.PolygonMode(gl.GL_FRONT_AND_BACK, gl.GL_FILL);
			}
		}

		public void EndBuffering() {
			Graphics.SetRenderTarget(null);
		}

		public void RenderFinal() {
			Batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
			Batch.Draw(ColorBuffer, Vector2.Zero, Color.White);
			Batch.End();
			//Batch.Flush();

			Graphics.UnbindTexture(ColorBuffer);
		}
	}
	*/
}
