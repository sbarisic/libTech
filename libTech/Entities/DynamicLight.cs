using FishGfx;
using FishGfx.Graphics;
using libTech.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Entities {
	public class DynamicLight : Entity, IShaderArgumentative {
		public bool Enabled;

		public Vector3 Position;
		public Color Color;
		public float LightRadius;
		public bool CastShadows;

		Action<ShaderProgram> SetUniformsAction;

		public DynamicLight(Vector3 Position, Color Color, float LightRadius = 250, bool CastShadows = true) {
			Enabled = true;
			this.Position = Position;
			this.Color = Color;
			this.LightRadius = LightRadius;
			this.CastShadows = CastShadows;

			SetUniformsAction = (Shader) => {
				Shader.Uniform3f("LightPosition", this.Position);
				Shader.Uniform3f("LightColor", (Vector3)this.Color);
				Shader.Uniform1f("LightRadius", this.LightRadius);
			};
		}

		public void SetUniforms(ShaderProgram Shader) {
			SetUniformsAction(Shader);
		}

		public override string ToString() {
			return string.Format("DynamicLight({0})", Position);
		}
	}
}
