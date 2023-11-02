using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;

using BepuUtilities.Memory;

namespace libTech.Physics {
	public class PhysShape {
		//public Vector3 AABBMin;
		//public Vector3 AABBMax;
		//public Vector3 CenterOfVertices;

		IShape Shape;

		TypedIndex Handle;
		bool HandleValid;

		public PhysShape() {
		}

		public void SetHandle(TypedIndex Handle) {
			this.Handle = Handle;
			HandleValid = true;
		}

		public TypedIndex GetHandle() {
			if (!HandleValid)
				throw new Exception();

			return Handle;
		}

		public void AddToSimulation(Simulation Sim) {
			if (HandleValid)
				return;

			switch (Shape) {
				case Sphere Shp:
					SetHandle(Sim.Shapes.Add(Shp));
					break;

				case Box Shp:
					SetHandle(Sim.Shapes.Add(Shp));
					break;

				case Mesh Shp:
					SetHandle(Sim.Shapes.Add(Shp));
					break;

				default:
					throw new NotImplementedException();
			}
		}

		public IShape GetShape() {
			return Shape;
		}

		public BodyInertia ComputeInertia(float Mass) {
			switch (Shape) {
				case Sphere Shp:
					return Shp.ComputeInertia(Mass);

				case Box Shp:
					return Shp.ComputeInertia(Mass);

				case Mesh Shp:
					return Shp.ComputeClosedInertia(Mass);

				default:
					throw new NotImplementedException();
			}
		}


		public static PhysShape FromVertices(PhysEngine PhysEng, IEnumerable<Vector3> Verts) {
			BufferPool Pool = PhysEng.GetPool();

			Vector3[] VertArray = Verts.ToArray();
			int TriCount = VertArray.Length / 3;

			Pool.Take(TriCount, out Buffer<Triangle> TriBuffer);

			for (int i = 0; i < TriCount; i++) {
				TriBuffer[i] = new Triangle(VertArray[i * 3 + 0], VertArray[i * 3 + 1], VertArray[i * 3 + 2]);
			}

			PhysShape Shp = new PhysShape();
			Shp.Shape = new Mesh(TriBuffer, Vector3.One, Pool);
			return Shp;
		}

		public static PhysShape FromVerticesConcave(PhysEngine PhysEng, IEnumerable<Vector3> Verts) {
			return FromVertices(PhysEng, Verts);

			/*Vector3[] VertsArray = Verts.ToArray();
			TriangleMesh TriMesh = new TriangleMesh();

			for (int i = 0; i < VertsArray.Length; i += 3)
				TriMesh.AddTriangle(VertsArray[i + 0], VertsArray[i + 1], VertsArray[i + 2]);

			BvhTriangleMeshShape TriShape = new BvhTriangleMeshShape(TriMesh, false);
			return new libTechCollisionShape(TriShape);*/
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
