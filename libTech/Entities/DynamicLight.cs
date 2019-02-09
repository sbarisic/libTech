using FishGfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Entities {
	public class DynamicLight : Entity {
		public bool Enabled;

		public Vector3 Position;
		public Color Color;
		public float LightRadius;

		public DynamicLight(Vector3 Position, Color Color, float LightRadius = 250) {
			Enabled = true;
			this.Position = Position;
			this.Color = Color;
			this.LightRadius = LightRadius;
		}

		public override string ToString() {
			return string.Format("DynamicLight({0})", Position);
		}
	}
}
