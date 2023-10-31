using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BulletSharp;

namespace libTech.Physics {
	public class ContactResult : ContactResultCallback {
		public Vector3[] CollisionPointsLocal;
		public Vector3[] CollisionPointsWorld;
		public Vector3[] CollisionNormals;

		public int CollisionPointCount;

		RigidBody Body;

		public ContactResult(RigidBody Body) {
			this.Body = Body;

			CollisionPointsLocal = new Vector3[32];
			CollisionPointsWorld = new Vector3[32];
			CollisionNormals = new Vector3[32];
			CollisionPointCount = 0;
		}

		/*// If you don't want to consider collisions where the bodies are joined by a constraint, override NeedsCollision:
		// However, if you use a CollisionObject for #body instead of a RigidBody,
		//  then this is unnecessary — CheckCollideWithOverride isn't available.
		public override bool NeedsCollision(BroadphaseProxy proxy) {
			// superclass will check CollisionFilterGroup and CollisionFilterMask
			if (base.NeedsCollision(proxy)) {
				// if passed filters, may also want to avoid contacts between constraints
				return _monitoredBody.CheckCollideWithOverride(proxy.ClientObject as CollisionObject);
			}

			return false;
		}*/

		// Called with each contact for your own processing (e.g. test if contacts fall in within sensor parameters)
		public override float AddSingleResult(ManifoldPoint contact, CollisionObjectWrapper colObj0, int partId0, int index0, CollisionObjectWrapper colObj1, int partId1, int index1) {
			if (CollisionPointCount >= CollisionPointsLocal.Length)
				return 0;

			Vector3 ColPointLocal = contact.LocalPointA;
			Vector3 ColPointWorld = contact.PositionWorldOnA;
			Vector3 ColNormal = contact.NormalWorldOnB;

			CollisionPointsLocal[CollisionPointCount] = ColPointLocal;
			CollisionPointsWorld[CollisionPointCount] = ColPointWorld;
			CollisionNormals[CollisionPointCount] = ColNormal;


			CollisionPointCount++;
			return 0;
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
