using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace libTech.Physics {
	public struct SweepResult {
		public bool HasHit;
		public float Fraction;
		public float Distance;

		public Vector3 SweepFrom;
		public Vector3 HitPoint;
		public Vector3 HitCenterOfMass;
		public Vector3 Normal;

		public override string ToString() {
			return string.Format("{0} {1} {2}", HasHit, (int)Distance, Fraction);
		}
	}
}
