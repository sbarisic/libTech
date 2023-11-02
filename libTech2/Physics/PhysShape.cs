using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Collidables;

namespace libTech.Physics {
	public class PhysShape {
		public Vector3 AABBMin;
		public Vector3 AABBMax;
		public Vector3 CenterOfVertices;

		IShape Shape;


		public static PhysShape FromVertices(IEnumerable<Vector3> Verts) {
			/*ConvexHullShape HullShape = new ConvexHullShape(Verts);
			HullShape.InitializePolyhedralFeatures();
			return new libTechCollisionShape(HullShape);*/

			return default;


		}

		public static PhysShape FromVerticesConcave(IEnumerable<Vector3> Verts) {
			/*Vector3[] VertsArray = Verts.ToArray();
			TriangleMesh TriMesh = new TriangleMesh();

			for (int i = 0; i < VertsArray.Length; i += 3)
				TriMesh.AddTriangle(VertsArray[i + 0], VertsArray[i + 1], VertsArray[i + 2]);

			BvhTriangleMeshShape TriShape = new BvhTriangleMeshShape(TriMesh, false);
			return new libTechCollisionShape(TriShape);*/

			return default;
		}

		public static PhysShape CreateBox(float X, float Y, float Z) {
			PhysShape Box = new PhysShape();
			Box.Shape = new Box(X / 2, Y / 2, Z / 2);
			return Box;
		}

		public static PhysShape CreateSphere(float Radius) {
			PhysShape S = new PhysShape();
			S.Shape = new Sphere(Radius);

			return S;
		}
	}
}
