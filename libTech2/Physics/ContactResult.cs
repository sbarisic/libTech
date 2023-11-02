using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Physics {
	public class ContactResult  {
		public Vector3[] CollisionPointsLocal;
		public Vector3[] CollisionPointsWorld;
		public Vector3[] CollisionNormals;
		public int CollisionPointCount;

		public ContactResult() {
			CollisionPointsLocal = new Vector3[32];
			CollisionPointsWorld = new Vector3[32];
			CollisionNormals = new Vector3[32];
			CollisionPointCount = 0;
		}

		public bool AnyNormalLower(Vector3 Normal, float AngleDeg) {
			// 1 - parallel
			// 0 - orthogonal
			// -1 - opposite directions
			float DotExpected = 1 - (AngleDeg / 90);

			for (int i = 0; i < CollisionPointCount; i++) {
				float Dot = Vector3.Dot(CollisionNormals[i], Normal);

				if (Dot >= DotExpected)
					return true;
			}

			return false;
		}

		public override string ToString() {
			return string.Format("{0}", CollisionPointCount);
		}
	}
}
