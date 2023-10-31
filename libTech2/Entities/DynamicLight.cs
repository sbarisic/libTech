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
	[EntityClassName("light")]
	[EntityClassName("light_point")]
	public class DynamicLight : Entity, IShaderArgumentative {
		public bool Enabled;

		public Vector3 Position;
		public Color Color;
		public float LightRadius;
		public bool CastShadows;

		void Init(Vector3 Position, Color Color, float LightRadius = 250, bool CastShadows = true) {
			Enabled = true;
			this.Position = Position;
			this.Color = Color;
			this.LightRadius = LightRadius;
			this.CastShadows = CastShadows;
		}

		public DynamicLight(Vector3 Position, Color Color, float LightRadius = 250, bool CastShadows = true) {
			Init(Position, Color, LightRadius, CastShadows);
		}

		public DynamicLight(EntityKeyValues KVs) {
			float Radius = KVs.Get<float>("_distance");

			if (Radius == 0) {
				Radius = KVs.Get<float>("_zero_percent_distance");

				if (Radius == 0)
					Radius = 250;
			}

			Init(KVs.Get<Vector3>("position"), KVs.Get("_light", Color.White), Radius);
		}

		public void SetUniforms(ShaderProgram Shader) {
			Shader.Uniform3f("LightPosition", Position);
			Shader.Uniform4f("LightColor", (Vector4)Color);
			Shader.Uniform1f("LightRadius", LightRadius);
		}

		public override BoundSphere GetBoundingSphere() {
			return new BoundSphere(Position, LightRadius);
		}

		public override string ToString() {
			return string.Format("DynamicLight({0})", Position);
		}
	}
}
