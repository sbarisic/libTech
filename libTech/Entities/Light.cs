using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace libTech.Entities {
	public enum LightType {
		Point
	}

	public class Light : Entity {
		public override Vector3 Position { get; set; }

		public LightType LightType;
		public Vector4 Color;
		public float Radius;
	}
}
