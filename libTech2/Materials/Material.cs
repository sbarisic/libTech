using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Materials {
	public abstract class Material {
		public ShaderProgram Shader;
		public string MaterialName;

		public bool AlphaTest;
		public bool Translucent;
		public bool NoCull;
		public bool BlendTintByBaseAlpha;

		protected bool ShaderEnabled;

		public Material(ShaderProgram Shader, string MaterialName) {
			this.Shader = Shader;
			this.MaterialName = MaterialName;

			Translucent = false;
			NoCull = false;
			AlphaTest = false;
			ShaderEnabled = true;
		}

		float OldAlphaTest;

		public virtual void BeginDraw(int PassNumber) {
			if (NoCull) {
				RenderState RS = Gfx.PeekRenderState();
				RS.EnableCullFace = false;
				Gfx.PushRenderState(RS);
			}

			if (ShaderEnabled) {
				OldAlphaTest = ShaderUniforms.Current.AlphaTest;
				ShaderUniforms.Current.AlphaTest = AlphaTest ? 0.5f : 0.0f;
				Shader.Bind(ShaderUniforms.Current);
			}
		}

		public virtual void DrawMesh(Mesh3D Mesh) {
			BeginDraw(0);
			Mesh.Draw();
			EndDraw(0);
		}

		public virtual void EndDraw(int PassNumber) {
			if (ShaderEnabled) {
				Shader.Unbind();
				ShaderUniforms.Current.AlphaTest = OldAlphaTest;
			}

			if (NoCull)
				Gfx.PopRenderState();
		}
	}
}
