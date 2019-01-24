using FishGfx.Graphics;
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

		public Material(ShaderProgram Shader, string MaterialName) {
			this.Shader = Shader;
			this.MaterialName = MaterialName;

			Translucent = false;
			NoCull = false;
			AlphaTest = false;
		}

		float OldAlphaTest;

		public virtual void Bind() {
			RenderState RS = Gfx.PeekRenderState();

			if (NoCull) {
				RS.EnableCullFace = false;
			}

			Gfx.PushRenderState(RS);

			OldAlphaTest = ShaderUniforms.Current.AlphaTest;
			ShaderUniforms.Current.AlphaTest = AlphaTest ? 0.5f : 0.0f;
			Shader.Bind(ShaderUniforms.Current);
		}

		public virtual void Unbind() {
			Shader.Unbind();
			ShaderUniforms.Current.AlphaTest = OldAlphaTest;
			Gfx.PopRenderState();
		}
	}
}
