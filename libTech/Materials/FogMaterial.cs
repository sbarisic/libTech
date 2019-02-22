using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Materials {
	public class FogMaterial : Material {
		public const string Name = nameof(FogMaterial);

		static RenderTexture DepthCapture;
		static ShaderProgram DepthThicknessShader, FogShader;

		static Texture ThicknessTex;

		public FogMaterial(string MaterialName = Name) : base(null, MaterialName) {
			Translucent = true;

			if (DepthCapture == null) {
				DepthCapture = new RenderTexture(Engine.Window.WindowWidth, Engine.Window.WindowHeight, CreateColor: false);
				DepthCapture.Framebuffer.SetLabel(OpenGL.ObjectIdentifier.Framebuffer, "Fog depth capture");

				ThicknessTex = DepthCapture.CreateNewColorAttachment(0, TextureInternalFmt.Rgba32f);
				ThicknessTex.SetFilter(TextureFilter.Nearest);
				ThicknessTex.SetWrap(TextureWrap.ClampToEdge);
			}

			if (FogShader == null)
				FogShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/fog.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/fog.frag"));

			if (DepthThicknessShader == null)
				DepthThicknessShader = Engine.GetShader("depth_thickness");

			ShaderEnabled = false;
		}

		public override void DrawMesh(Mesh3D Mesh) {
			DepthCapture.Push();
			{
				Gfx.Clear(Color.Transparent);
				RenderState RS = Gfx.PeekRenderState();
				RS.EnableBlend = true;
				RS.BlendFunc(BlendFactor.One, BlendFactor.One);
				RS.EnableDepthTest = false;


				RS.EnableCullFace = true;
				RS.CullFace = CullFace.Back;
				RS.SetColorMask(true, true, false, false);

				DepthThicknessShader.Uniform1f("Near", Engine.Camera3D.Near);
				DepthThicknessShader.Uniform1f("Far", Engine.Camera3D.Far);
				DepthThicknessShader.Bind(ShaderUniforms.Current);

				// First pass
				Gfx.PushRenderState(RS);
				{
					DepthThicknessShader.Uniform1f("Scale", 1.0f);
					Mesh.Draw();
				}
				Gfx.PopRenderState();

				// Second pass
				RS.CullFace = CullFace.Front;
				RS.SetColorMask(true, false, false, false);
				Gfx.PushRenderState(RS);
				{
					DepthThicknessShader.Uniform1f("Scale", -1.0f);
					Mesh.Draw();
				}
				Gfx.PopRenderState();

				DepthThicknessShader.Unbind();
			}
			DepthCapture.Pop();

			FogShader.Bind(ShaderUniforms.Current);
			ThicknessTex.BindTextureUnit();

			// Final pass
			{
				/*// Depth sort to make sure fragment shader executes only once
				RenderState RS = Gfx.PeekRenderState();
				RS.SetColorMask(false);
				RS.EnableDepthMask = true;
				Gfx.PushRenderState(RS);
				Mesh.Draw();
				Gfx.PopRenderState();

				// Actual shading
				RS.SetColorMask(true);
				RS.EnableDepthMask = false;
				RS.DepthFunc = DepthFunc.Equal;
				Gfx.PushRenderState(RS);
				Mesh.Draw();
				Gfx.PopRenderState();*/

				EngineRenderer.BeginDrawStencilMask(StencilMaskMode.AnyFaceDepthTested);
				Mesh.Draw();
				EngineRenderer.EndDrawStencilMask();

				EngineRenderer.BeginUseStencilMask(StencilFunction.Equal, 0xFF, 0xFF, false);
				Mesh.Draw();
				EngineRenderer.EndUseStencilMask();
			}

			ThicknessTex.UnbindTextureUnit();
			FogShader.Unbind();
		}
	}
}
