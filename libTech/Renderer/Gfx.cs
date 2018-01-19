using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ultraviolet;
using Ultraviolet.Content;
using Ultraviolet.Core;
using Ultraviolet.Core.Text;
using Ultraviolet.Graphics;
using Ultraviolet.Graphics.Graphics2D;
using Ultraviolet.Graphics.Graphics2D.Text;
using Ultraviolet.OpenGL;
using Ultraviolet.Platform;
using Ultraviolet.OpenGL.Bindings;

namespace libTech.Renderer {
	public static class Gfx {
		internal static IUltravioletGraphics UVGfx;

		static Stack<Viewport> Viewports = new Stack<Viewport>();
		static Stack<BlendState> BlendStates = new Stack<BlendState>();
		static Stack<RasterizerState> RasterizerStates = new Stack<RasterizerState>();
		static Stack<DepthStencilState> DepthStencilStates = new Stack<DepthStencilState>();
		static Stack<Rectangle?> ScissorRectangles = new Stack<Rectangle?>();

		public static void PushViewport(Viewport V) {
			Viewports.Push(UVGfx.GetViewport());
			UVGfx.SetViewport(V);
		}

		public static void PopViewport() {
			UVGfx.SetViewport(Viewports.Pop());
		}

		public static void PushBlendState(BlendState BS) {
			BlendStates.Push(UVGfx.GetBlendState());
			UVGfx.SetBlendState(BS);
		}

		public static void PopBlendState() {
			UVGfx.SetBlendState(BlendStates.Pop());
		}

		public static void PushRasterizerState(RasterizerState RS) {
			RasterizerStates.Push(UVGfx.GetRasterizerState());
			UVGfx.SetRasterizerState(RS);
		}

		public static void PopRasterizerState() {
			UVGfx.SetRasterizerState(RasterizerStates.Pop());
		}

		public static void PushDepthStencilState(DepthStencilState DSS) {
			DepthStencilStates.Push(UVGfx.GetDepthStencilState());
			UVGfx.SetDepthStencilState(DSS);
		}

		public static void PopDepthStencilState() {
			UVGfx.SetDepthStencilState(DepthStencilStates.Pop());
		}

		public static void PushScissorRectangle(Rectangle? R) {
			ScissorRectangles.Push(UVGfx.GetScissorRectangle());
			UVGfx.SetScissorRectangle(R);
		}

		public static void PopScissorRectangle() {
			UVGfx.SetScissorRectangle(ScissorRectangles.Pop());
		}
	}
}
