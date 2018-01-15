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

using libTech.Reflection;
using libTech.Importer;
using System.Reflection;
using CARP;
using System.IO;

using NuklearDotNet;
using System.Runtime.InteropServices;
using Ultraviolet.Input;

namespace libTech {
	internal unsafe class RenderDevice : NuklearDeviceTex<Texture2D> {
		Engine Engine;

		VertexDeclaration NuklearVertexDecl;

		IUltravioletWindow Wind;
		IUltravioletGraphics Graphics;
		RenderTarget2D RenderTarget;
		RenderBuffer2D ColorBuffer;
		SpriteBatch Batch;

		SamplerState Sampling;
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

			RenderTarget = RenderTarget2D.Create(Wind.WindowedClientSize.Width, Wind.WindowedClientSize.Height);
			ColorBuffer = RenderBuffer2D.Create(RenderBufferFormat.Color, RenderTarget.Width, RenderTarget.Height);
			RenderTarget.Attach(ColorBuffer);

			Sampling = SamplerState.Create();
			Sampling.Filter = TextureFilter.Point;
			Sampling.AddressU = Sampling.AddressV = TextureAddressMode.Clamp;

			DepthStencil = DepthStencilState.Create();
			DepthStencil.StencilEnable = false;
			DepthStencil.DepthBufferEnable = false;

			Blending = BlendState.AlphaBlend;

			Rasterizing = RasterizerState.Create();
			Rasterizing.ScissorTestEnable = true;
			Rasterizing.CullMode = CullMode.None;
			//Rasterizing.FillMode = FillMode.Wireframe;
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

		public void BeginBuffering() {
			Graphics.SetRenderTarget(RenderTarget);
			Graphics.Clear(Color.Transparent);
		}

		public override void SetBuffer(NkVertex[] Verts, ushort[] Inds) {
			EnsureBufferSize(Verts.Length, Inds.Length);

			for (int i = 0; i < Verts.Length; i++) {
				Verts[i].Position.X = (int)Verts[i].Position.X;
				Verts[i].Position.Y = (int)Verts[i].Position.Y;
			}

			IndexBuffer.SetData(Inds);
			VertexBuffer.SetData(Verts);
		}

		public override void Render(NkHandle Userdata, Texture2D Texture, NkRect ClipRect, uint Offset, uint Count) {
			Effect.Texture = Texture;

			foreach (var Pass in Effect.CurrentTechnique.Passes) {
				Pass.Apply();

				Graphics.SetViewport(VPort);
				Graphics.SetBlendState(Blending);
				Graphics.SetRasterizerState(Rasterizing);
				Graphics.SetDepthStencilState(DepthStencil);
				Graphics.SetSamplerState(0, Sampling);

				Graphics.SetScissorRectangle((int)ClipRect.X, (int)ClipRect.Y, (int)ClipRect.W, (int)ClipRect.H);
				Graphics.SetGeometryStream(GeomStream);
				Graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, (int)Offset, (int)Count / 3);
			}
		}

		public void EndBuffering() {
			Graphics.SetRenderTarget(null);
		}

		public void RenderFinal() {
			Batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
			Batch.Draw(ColorBuffer, Vector2.Zero, Color.White);
			Batch.End();

			Graphics.UnbindTexture(ColorBuffer);
		}
	}

	public class Engine : UltravioletApplication {
		internal RenderDevice RenderDevice;

		static void Main(string[] args) {
			Console.Title = "libTech";

			CVar.InitMode = true;
			CVar.Register("game", "basegame", CVarType.Replicated | CVarType.Init, (This, Old, New) => This.Value = Path.GetFullPath((string)New));

			CVar.Register("width", 800, CVarType.Archive);
			CVar.Register("height", 600, CVarType.Archive);
			CVar.Register("borderless", false, CVarType.Archive);
			CVar.Register("resizable", false, CVarType.Archive);

			// Parse all arguments and set CVars
			foreach (var Arg in ArgumentParser.All) {
				CVar CVar = CVar.Find(Arg.Key);

				if (CVar != null)
					CVar.Value = Arg.Value.LastOrDefault();
				else
					CVar.Register(Arg.Key, Arg.Value.LastOrDefault());
			}

			CVar.InitMode = false;
			foreach (var CVar in CVar.GetAll())
				Console.WriteLine(CVar);

			foreach (var Type in Reflect.GetAllTypes(Reflect.GetExeAssembly()))
				if (!Type.IsAbstract && Reflect.Inherits(Type, typeof(Importer.Importer)))
					Importers.Register(Type);

			using (Engine P = new Engine()) {
				P.Run();
			}
		}

		public Engine() : base("Carpmanium", "libTech") {
			RenderDevice = new RenderDevice(this);
		}

		protected override UltravioletContext OnCreatingUltravioletContext() {
			OpenGLUltravioletConfiguration Cfg = new OpenGLUltravioletConfiguration();
			Cfg.MinimumOpenGLVersion = new Version(3, 2);

			Cfg.WindowIsResizable = CVar.GetBool("resizable");
			Cfg.WindowIsBorderless = CVar.GetBool("borderless");

#if DEBUG
			Cfg.Debug = true;
			Cfg.DebugLevels = DebugLevels.Warning | DebugLevels.Error;

			Cfg.DebugCallback = (UV, Lvl, Msg) => Console.WriteLine("{0}: {1}", Lvl, Msg);
#endif

			OpenGLUltravioletContext Ctx = new OpenGLUltravioletContext(this, Cfg);
			Console.WriteLine("{0}", gl.GetString(gl.GL_VERSION));
			return Ctx;
		}

		protected override void OnInitialized() {
			base.OnInitialized();

			IUltravioletPlatform Platform = Ultraviolet.GetPlatform();
			IUltravioletWindow Window = Platform.Windows.FirstOrDefault();

			if (Window != null) {
				Window.Caption = "libTech";
				Window.ClientSize = new Size2(CVar.GetInt("width", 800), CVar.GetInt("height", 600));
				Window.SetWindowedClientSizeCentered(Window.ClientSize);
			}

			IUltravioletInput Input = Ultraviolet.GetInput();
			MouseDevice Mouse = Input.GetMouse();
			KeyboardDevice Keyboard = Input.GetKeyboard();

			Mouse.Moved += (IUltravioletWindow W, MouseDevice Dev, int X, int Y, int DX, int DY) => {
				RenderDevice.OnMouseMove(X, Y);
			};

			Mouse.WheelScrolled += (W, Dev, X, Y) => {
				RenderDevice.OnScroll(X, Y);
			};

			Mouse.ButtonPressed += (W, Dev, Button) => {
				Point2 Pos = Dev.Position;
				if (Button == MouseButton.Left || Button == MouseButton.Middle || Button == MouseButton.Right)
					RenderDevice.OnMouseButton((NuklearEvent.MouseButton)(Button - 1), Pos.X, Pos.Y, true);
			};

			Mouse.ButtonReleased += (W, Dev, Button) => {
				Point2 Pos = Dev.Position;
				if (Button == MouseButton.Left || Button == MouseButton.Middle || Button == MouseButton.Right)
					RenderDevice.OnMouseButton((NuklearEvent.MouseButton)(Button - 1), Pos.X, Pos.Y, false);
			};

			NuklearAPI.Init(RenderDevice);
		}

		protected override void OnUpdating(UltravioletTime time) {
			base.OnUpdating(time);
		}

		protected override void OnDrawing(UltravioletTime time) {
			base.OnDrawing(time);

			NuklearAPI.Frame(() => {
				NuklearAPI.Window("Ultraviolet", 100, 100, 200, 200, NkPanelFlags.BorderTitle | NkPanelFlags.MovableScalable | NkPanelFlags.Minimizable, () => {
					NuklearAPI.LayoutRowDynamic(35);

					for (int i = 0; i < 5; i++)
						NuklearAPI.ButtonLabel("Some Button " + i);

					if (NuklearAPI.ButtonLabel("Exit"))
						Environment.Exit(0);
				});
			});
		}
	}
}
