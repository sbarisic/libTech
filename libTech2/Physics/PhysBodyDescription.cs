using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;

using FishGfx.Graphics;

namespace libTech.Physics {
	public class PhysBodyDescription {
		PhysShape Shape;

		BodyDescription BodyDesc;
		StaticDescription StaticDesc;

		BodyHandle BHandle;
		StaticHandle SHandle;
		bool HandleValid;

		BodyInertia Inertia;
		float Mass;

		public bool IsStatic {
			get; private set;
		}

		public PhysBodyDescription(PhysEngine PhysEng, PhysShape Shape, float Mass) {
			HandleValid = false;
			IsStatic = Mass == 0;

			this.Shape = Shape;
			this.Mass = Mass;
			PhysEng.AddShape(Shape);

			if (IsStatic) {
				StaticDesc = new StaticDescription(RigidPose.Identity, Shape.GetHandle());
			} else {
				Inertia = Shape.ComputeInertia(Mass);
				BodyDesc = BodyDescription.CreateDynamic(RigidPose.Identity, Inertia, Shape.GetHandle(), 0.01f);
			}
		}

		public void AddToSimulation(Simulation Sim) {
			if (IsStatic) {

				//StaticDesc = new StaticDescription(new RigidPose(Pos, Rot), Shape.GetHandle());
				SHandle = Sim.Statics.Add(StaticDesc);
				HandleValid = true;

			} else {

				//BodyDesc = BodyDescription.CreateDynamic(new RigidPose(Pos, Rot), Inertia, Shape.GetHandle(), 0.01f);
				BHandle = Sim.Bodies.Add(BodyDesc);
				HandleValid = true;

			}
		}

		public virtual void GetWorldTransform(PhysEngine Engine, out Vector3 Scale, out Quaternion Rotation, out Vector3 Position) {
			if (!HandleValid)
				throw new InvalidOperationException();

			Simulation Sim = Engine.GetSimulation();
			RigidPose Tmp = new RigidPose();
			ref RigidPose Pose = ref Tmp;

			if (IsStatic) {
				Pose = ref Sim.Statics[SHandle].Pose;
			} else {
				Pose = ref Sim.Bodies[BHandle].Pose;
			}


			Scale = Vector3.One;
			Rotation = Pose.Orientation;
			Position = Pose.Position;

			//Matrix4x4.Decompose(WorldTransform, out Scale, out Rotation, out Position);
		}

		public virtual void SetWorldTransform(PhysEngine Engine, Vector3 Scale, Quaternion Rotation, Vector3 Position) {
			//WorldTransform = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);

			if (!HandleValid) {
				if (IsStatic) {
					StaticDesc.Pose = new RigidPose(Position, Rotation);
				} else {
					BodyDesc.Pose = new RigidPose(Position, Rotation);
				}

				return;
				// throw new InvalidOperationException();
			}

			Simulation Sim = Engine.GetSimulation();
			RigidPose Tmp = new RigidPose();
			ref RigidPose Pose = ref Tmp;

			if (IsStatic) {
				Pose = ref Sim.Statics[SHandle].Pose;
			} else {
				Pose = ref Sim.Bodies[BHandle].Pose;
			}

			Pose.Orientation = Rotation;
			Pose.Position = Position;
		}

		/*public BodyDescription GetBodyDesc() {
			return BodyDesc;
		}

		public void SetHandle(BodyHandle Handle) {
			this.Handle = Handle;
			HandleValid = true;
		}*/
	}
}
