using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using libTech.Entities;
using libTech.Graphics;
using libTech.Materials;
using libTech.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	static partial class Engine {
		internal static Mesh2D ScreenQuad;
		internal static libTechMesh PointLightMesh;
	}

	public enum StencilMaskMode {
		// Generate mask for rendering on intersections
		Intersection,

		// Generate mask for rendering either front or back face once
		AnyFaceDepthTested,

		AnyFace,
	}

	public static class EngineRenderer {
		public static void Init() {
			Engine.GBuffer = new RenderTexture(Engine.Window.WindowWidth, Engine.Window.WindowHeight, IsGBuffer: true);
			Engine.GBuffer.Framebuffer.SetLabel(OpenGL.ObjectIdentifier.Framebuffer, "GBuffer");

			Engine.ScreenRT = new RenderTexture(Engine.Window.WindowWidth, Engine.Window.WindowHeight);
			Engine.ScreenRT.Framebuffer.SetLabel(OpenGL.ObjectIdentifier.Framebuffer, "ScreenRT");

			Engine.ScreenQuad = new Mesh2D();
			Engine.ScreenQuad.PrimitiveType = PrimitiveType.Triangles;
			Engine.ScreenQuad.SetVertices(Utils.EmitRectangleTris(new Vertex2[6], 0, 0, 0, Engine.Window.WindowWidth, Engine.Window.WindowHeight, 0, 0, 1, 1, Color.White));
			Engine.ScreenQuad.VAO.SetLabel(OpenGL.ObjectIdentifier.VertexArray, "Screen Quad");

			var Msh = FishGfx.Formats.Obj.Load("content/models/sphere_2.obj").First();
			Vertex3[] MshVerts = Msh.Vertices.ToArray();
			AABB PointLightBound = AABB.CalculateAABB(MshVerts.Select(V => V.Position));

			for (int i = 0; i < MshVerts.Length; i++)
				MshVerts[i].Position /= PointLightBound.Size;

			Engine.PointLightMesh = new libTechMesh();
			Engine.PointLightMesh.SetVertices(MshVerts);
			Engine.PointLightMesh.SetLabel("Point Light Volume");

			// Screen framebuffer
			OpenGL.Gl.Get(OpenGL.Gl.MAX_SAMPLES, out int MaxMSAA);
			if (Engine.MSAA > MaxMSAA)
				Engine.MSAA.Value = MaxMSAA;

			LoadShaders();
		}

		internal static void LoadShaders() {
			// Shaders

			Engine.RegisterShader("nop", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/nop.frag")));

			Engine.RegisterShader("default", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/default_tex_clr.frag")));
			Engine.RegisterShader("default_deferred", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default_deferred.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/default_deferred.frag")));
			Engine.RegisterShader("deferred_shading", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/deferred_shading.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/deferred_shading.frag")));
			Engine.RegisterShader("deferred_ambient", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/deferred_shading.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/deferred_ambient.frag")));
			Engine.RegisterShader("shadow_volume", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/shadow_volume.vert"), new ShaderStage(ShaderType.GeometryShader, "content/shaders/shadow_volume.geom"),
				new ShaderStage(ShaderType.FragmentShader, "content/shaders/shadow_volume.frag")));

			Engine.RegisterShader("water", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/water.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/water.frag")));
			Engine.RegisterShader("framebuffer", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/fb.frag")));
			Engine.RegisterShader("depth_thickness", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/depth_thickness.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/depth_thickness.frag")));
			Engine.RegisterShader("skybox", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/skybox.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/skybox.frag")));

			Engine.RegisterShader("error", Engine.GetShader("default"));

			// Textures

			Texture ErrorTexture = Texture.FromFile("content/textures/error.png");
			ErrorTexture.SetWrap(TextureWrap.Repeat);
			Engine.RegisterTexture("error", ErrorTexture);

			Texture SkyboxTex = Texture.FromFileCubemap("content/textures/skybox/cloudtop/cloudtop");
			Engine.RegisterTexture("skybox", SkyboxTex);

			// Materials

			Engine.RegisterMaterial(new ShaderMaterial("shadow_volume"));
			Engine.RegisterMaterial(new ShaderMaterial("skybox"));
			Engine.RegisterMaterial(new FogMaterial());

			ShaderMaterial WaterMaterial = new ShaderMaterial("water");
			WaterMaterial.Translucent = true;
			Engine.RegisterMaterial(WaterMaterial);

			TexturedShaderMaterial ErrorMaterial = new TexturedShaderMaterial("error", ErrorTexture);
			Engine.RegisterMaterial(ErrorMaterial);

			string[] MaterialDefs = Engine.VFS.GetFiles("/content/materials/").Where(FName => Path.GetExtension(FName) == ".ltm").ToArray();

			foreach (var MatDefFile in MaterialDefs) {
				string[] MatDefs = Engine.VFS.ReadAllText(MatDefFile).Replace("\r", "").Trim().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

				foreach (var MatDef in MatDefs) {
					string[] KV = MatDef.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					Texture Texx = Engine.GetTexture("/content/" + KV[1]);
					Texx.SetFilter(TextureFilter.Linear);
					Texx.SetWrap(TextureWrap.Repeat);

					TexturedShaderMaterial Mat = new TexturedShaderMaterial(KV[0], Texx);
					Engine.RegisterMaterial(Mat);
				}
			}
		}

		public static void Draw(float Dt, bool AmbientLighting = true, bool PointLighting = true) {
			Engine.GetTexture("skybox").BindTextureUnit(10);

			DbgDraw.Enabled = Engine.DebugDraw;
			ShaderUniforms.Current.Resolution = Engine.Window.WindowSize;

			// Deferred opaque pass

			Engine.GBuffer.Push();
			{
				RenderState RS = Gfx.PeekRenderState();
				RS.EnableBlend = false;
				Gfx.PushRenderState(RS);
				Gfx.Clear(Color.Transparent);

				ShaderUniforms.Current.Camera = Engine.Camera3D;

				Engine.Map?.DrawOpaque();
				Engine.Game.DrawOpaque();

				Gfx.PopRenderState();
			}
			Engine.GBuffer.Pop();

			// Lighting/transparency pass

			Engine.ScreenRT.Push();
			{
				// Clear canvas, copy depth buffer, draw skybox

				Gfx.Clear(Color.Black);
				Engine.GBuffer.Framebuffer.Blit(false, true, false, Destination: Engine.ScreenRT.Framebuffer);
				Engine.Map?.DrawSkybox();

				Engine.GBuffer.Color.BindTextureUnit(0);
				Engine.GBuffer.Position.BindTextureUnit(1);
				Engine.GBuffer.Normal.BindTextureUnit(2);
				Engine.GBuffer.DepthStencil.BindTextureUnit(3);

				{
					RenderState State = Gfx.PeekRenderState();
					State.FrontFace = FrontFace.CounterClockwise;
					State.EnableBlend = true;
					State.BlendFunc_Src = BlendFactor.SrcAlpha;
					State.BlendFunc_Dst = BlendFactor.One;
					State.EnableDepthTest = false;

					Gfx.PushRenderState(State);
					ShaderUniforms.Current.Camera = Engine.Camera2D;

					if (AmbientLighting) {
						// Ambient lighting
						ShaderProgram AmbientShader = Engine.GetShader("deferred_ambient");
						AmbientShader.Bind(ShaderUniforms.Current);
						Engine.ScreenQuad.Draw();
						AmbientShader.Unbind();
					}

					// Point lighting
					Gfx.PopRenderState();
					State.FrontFace = FrontFace.CounterClockwise;
					State.EnableDepthMask = false;
					State.EnableDepthTest = true;
					State.EnableStencilTest = true;
					Gfx.PushRenderState(State);
					ShaderUniforms.Current.Camera = Engine.Camera3D;

					if (Engine.Map != null && PointLighting) {
						DynamicLight[] Lights = Engine.Map.GetLights();

						for (int i = 0; i < Lights.Length; i++) {
							DbgDraw.DrawCross(Lights[i].Position);

							State.SetColorMask(false);
							State.EnableBlend = false;
							State.EnableDepthTest = true;
							State.EnableCullFace = false;

							State.StencilFunc(StencilFunction.Always, 0, 0);
							State.StencilOpSeparate(StencilFace.Front, StencilOperation.Keep, StencilOperation.IncrWrap, StencilOperation.Keep);
							State.StencilOpSeparate(StencilFace.Back, StencilOperation.Keep, StencilOperation.DecrWrap, StencilOperation.Keep);

							State.DepthFunc = DepthFunc.LessOrEqual;

							Gfx.PushRenderState(State);
							Gfx.ClearStencil();

							DrawPointLightMask(Lights[i]);
							DrawPointLightShadow(Lights[i]);

							Gfx.PopRenderState();

							State.EnableBlend = true;
							State.SetColorMask(true);
							State.EnableDepthTest = false;
							State.EnableCullFace = true;
							State.StencilFunc(StencilFunction.Equal, 1, 0xFF);
							State.StencilOp(StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep);
							Gfx.PushRenderState(State);

							ShadePointLight(Lights[i]);

							Gfx.PopRenderState();
						}
					}

					Gfx.PopRenderState();
				}
				Engine.GBuffer.DepthStencil.UnbindTextureUnit(3);
				Engine.GBuffer.Normal.UnbindTextureUnit(2);
				Engine.GBuffer.Position.UnbindTextureUnit(1);
				Engine.GBuffer.Color.UnbindTextureUnit(0);

				// Draw transparent items
				Engine.Map?.DrawTransparent();
				Engine.Game.DrawTransparent();

				// Draw debug lines
				DbgDraw.FinalizeDraw((long)(Engine.Time * 1000));
			}
			Engine.ScreenRT.Pop();

			{
				RenderState State = Gfx.PeekRenderState();
				State.EnableDepthTest = false;
				Gfx.PushRenderState(State);
				{
					Gfx.Clear();
					ShaderUniforms.Current.Camera = Engine.Camera2D;
					ShaderUniforms.Current.TextureSize = Engine.ScreenRT.Color.Size;
					ShaderUniforms.Current.MultisampleCount = Engine.ScreenRT.Color.Multisamples;
					Gfx.TexturedRectangle(0, 0, Engine.Window.WindowWidth, Engine.Window.WindowHeight, Texture: Engine.ScreenRT.Color, Shader: Engine.GetShader("framebuffer"));

					Engine.GUI.Draw(() => {
						float FT = Engine.FrameTime.Average();
						float FPS = 1.0f / FT;
						string DebugString = string.Format("{0} ms\n{1} FPS\n{2} Lights", FT, FPS, Engine.Map.GetLights().Length);
						int Lines = 3;

						Gfx.DrawText(Engine.UI.DebugFont, new Vector2(2, Engine.WindowHeight - Engine.UI.DebugFont.ScaledLineHeight * Lines - 2), DebugString, Color.White);

						Engine.UI.Draw();
						Engine.Game.DrawGUI(Dt);
					});
				}
				Gfx.PopRenderState();
			}

			Engine.Window.SwapBuffers();
		}

		public static void BeginDrawStencilMask(StencilMaskMode MaskMode) {
			RenderState RS = Gfx.PeekRenderState();
			RS.EnableBlend = false;
			RS.EnableCullFace = false;

			RS.SetColorMask(false);
			RS.EnableDepthMask = false;

			switch (MaskMode) {
				case StencilMaskMode.Intersection:
					RS.EnableDepthTest = true;
					RS.DepthFunc = DepthFunc.LessOrEqual;

					RS.StencilFunc(StencilFunction.Always, 0, 0);
					RS.StencilOpSeparate(StencilFace.Front, StencilOperation.Keep, StencilOperation.IncrWrap, StencilOperation.Keep);
					RS.StencilOpSeparate(StencilFace.Back, StencilOperation.Keep, StencilOperation.DecrWrap, StencilOperation.Keep);
					break;

				case StencilMaskMode.AnyFaceDepthTested:
					RS.EnableDepthTest = true;
					RS.DepthFunc = DepthFunc.Always;

					goto case StencilMaskMode.AnyFace;

				case StencilMaskMode.AnyFace:
					RS.StencilFunc(StencilFunction.Always, 0xFF, 0xFF);
					RS.StencilOp(StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Replace);
					break;

				default:
					throw new InvalidOperationException();
			}

			Gfx.PushRenderState(RS);
			Gfx.ClearStencil();
		}

		public static void EndDrawStencilMask() {
			Gfx.PopRenderState();
		}

		public static void BeginUseStencilMask(StencilFunction Func, int Ref, uint Mask, bool WriteDepth = true, bool CullFront = true) {
			RenderState RS = Gfx.PeekRenderState();
			RS.EnableStencilTest = true;
			RS.StencilFunc(Func, Ref, Mask);
			RS.StencilOp(StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep);

			RS.EnableDepthTest = false;

			//if (CullFront) {
			RS.EnableCullFace = true;
			RS.CullFace = CullFace.Front;
			//}

			RS.SetColorMask(true);
			RS.EnableDepthMask = WriteDepth;

			Gfx.PushRenderState(RS);
		}

		public static void EndUseStencilMask() {
			Gfx.PopRenderState();
		}

		static void PreparePointLight(DynamicLight Light) {
			ShaderUniforms.Current.Model = Matrix4x4.CreateScale(Light.LightRadius) * Matrix4x4.CreateTranslation(Light.Position);
		}

		static void DrawPointLightShadow(DynamicLight Light) {
			if (!Light.CastShadows)
				return;

			ShaderMaterial ShadowVolume = (ShaderMaterial)Engine.GetMaterial("shadow_volume");

			RenderState RS = Gfx.PeekRenderState();
			RS.FrontFace = FrontFace.Clockwise;
			Gfx.PushRenderState(RS);

			Light.SetUniforms(ShadowVolume.Shader);
			//Engine.Map.DrawShadowVolume(ShadowVolume);
			Engine.Map.DrawEntityShadowVolume(Light, ShadowVolume);

			Gfx.PopRenderState();
		}

		static ShaderMaterial StencilMat;
		static void DrawPointLightMask(DynamicLight Light) {
			if (StencilMat == null)
				StencilMat = new ShaderMaterial("nop", Engine.GetShader("nop"));

			PreparePointLight(Light);
			Engine.PointLightMesh.Material = StencilMat;
			Engine.PointLightMesh.Draw();
		}

		static ShaderMaterial DeferredShadingMat;
		static void ShadePointLight(DynamicLight Light) {
			if (DeferredShadingMat == null)
				DeferredShadingMat = new ShaderMaterial("deferred_shading", Engine.GetShader("deferred_shading"));

			Light.SetUniforms(DeferredShadingMat.Shader);

			PreparePointLight(Light);
			Engine.PointLightMesh.Material = DeferredShadingMat;
			Engine.PointLightMesh.Draw();
		}

	}
}