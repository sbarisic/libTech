using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using BulletSharp;

namespace libTech.Entities {
	public abstract class PhysicsEntity : Entity {
		//public BEPUEntity PhysEntity;
		public RigidBody Body;

		public override Vector3 Position {
			get {
				return Body.WorldTransform.Translation;
			}

			set {
				//PhysEntity.Position = value.ToPhysVec3();
			}
		}

		/*public void CreateMobileMesh(IEnumerable<Vector3> Vertices) {
			Vector3[] VertArray = Vertices.ToArray();

			TForm Transform = new TForm(Vector3.One.ToPhysVec3(), Quaternion.Identity.ToPhysQuat(), Vector3.Zero.ToPhysVec3());
			MobileMesh Msh = new MobileMesh(VertArray.ToPhysVec3().ToArray(), (VertArray.Length).Range().ToArray(), Transform, MobileMeshSolidity.DoubleSided);
			Msh.Tag = this;

			//PhysEntity = Msh;
			SpaceObject = Msh;
		}*/

		public void CreateStaticMesh(IEnumerable<Vector3> Vertices) {
			/*Vector3[] VertArray = Vertices.ToArray();
			StaticMesh Msh = new StaticMesh(VertArray.ToPhysVec3().ToArray(), VertArray.Length.Range().ToArray());
			Msh.Tag = this;

			SpaceObject = Msh;*/


		}

		/*public void CreateCompoundBody() {
			CompoundBody Compound = new CompoundBody();
		}*/
	}
}
