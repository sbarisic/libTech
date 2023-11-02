//using BulletSharp;

using libTech.Entities;
using libTech.Graphics;
using libTech.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Weapons {
	/*
	 
	public class UtilityGun : BaseWeapon {
		public float MaxPickDistance;

		EntPhysics PickedEntity;
		Point2PointConstraint PointConst;
		bool IsPickingBody;

		float PickDist;
		float OldAngularDamping;
		float OldLinearDamping;

		Vector3 GlobalPickPoint;
		Vector3 LocalPickPoint;

		public UtilityGun() {
			MaxPickDistance = 2000;
			//ViewModel = Engine.Load<libTechModel>("/models/weapons/v_physcannon.mdl");
			ViewModelTranslation = new Vector3(0, 0, -20);
		}

		public override void PrimaryFire(bool Pressed) {
			if (Pressed)
				PickBody();
			else
				DropBody();
		}

		public override void SecondaryFire(bool Pressed) {
			if (Pressed && IsPickingBody) {
				EntPhysics FreezeEntity = PickedEntity;
				DropBody();

				FreezeEntity.Freeze();
			}
		}

		public override void Reload(bool Pressed) {
			if (Pressed) {
				EntPhysics Ent = RayCastEntity(out Vector3 PickPoint);

				if (Ent != null)
					Ent.Unfreeze();

				//Vector3[] Pts = Engine.Camera3D.GetFrustumPoints();

				// Connecting lines
				//Vector3 A = DrawRay(Pts[0], Pts[4]);
				//Vector3 B = DrawRay(Pts[1], Pts[5]);
				//Vector3 C = DrawRay(Pts[2], Pts[6]);
				//Vector3 D = DrawRay(Pts[3], Pts[7]);
				//
				//DbgDraw.DrawLine(A, B, FishGfx.Color.Red, Time: 30000);
				//DbgDraw.DrawLine(B, C, FishGfx.Color.Red, Time: 30000);
				//DbgDraw.DrawLine(C, D, FishGfx.Color.Red, Time: 30000);
				//DbgDraw.DrawLine(D, A, FishGfx.Color.Red, Time: 30000);
			}
		}

		Vector3 DrawRay(Vector3 From, Vector3 To) {
			if (Map.PhysicsEngine.RayCast(From, To, out Vector3 HitPos, out Vector3 Normal, out RigidBody Body)) {
				DbgDraw.DrawLine(From, HitPos, Time: 30000);
				return HitPos;
			}

			DbgDraw.DrawLine(From, To, Time: 30000);
			return To;
		}

		EntPhysics RayCastEntity(out Vector3 PickPoint, out Vector3 Normal) {
			Map.PhysicsEngine.RayCast(FireOrigin, FireOrigin + FireDirection * MaxPickDistance, out PickPoint, out Normal, out RigidBody Body);
			return Body?.UserObject as EntPhysics;
		}

		EntPhysics RayCastEntity(out Vector3 PickPoint) {
			return RayCastEntity(out PickPoint, out Vector3 Normal);
		}

		void PickBody() {
			DropBody();
			PickedEntity = RayCastEntity(out Vector3 PickPoint, out Vector3 Normal);

			if (PickedEntity != null) {
				DbgDraw.DrawLine(FireOrigin + FireDirection * 10, PickPoint, Time: 5000);
				DbgDraw.DrawCircle(PickPoint + Normal * 0.1f, Normal, FishGfx.Color.Red, 5, 8, Time: 5000);

				PickDist = Vector3.Distance(FireOrigin, PickPoint);
				GlobalPickPoint = PickPoint;

				Matrix4x4.Invert(PickedEntity.RigidBody.CenterOfMassTransform, out Matrix4x4 InvCenterOfMass);
				LocalPickPoint = PickPoint = Vector3.Transform(PickPoint, InvCenterOfMass);

				PickedEntity.Unfreeze();
				PickedEntity.DisableSleep();

				PointConst = new Point2PointConstraint(PickedEntity.RigidBody, PickPoint);
				PointConst.Setting.ImpulseClamp = 10000;
				PointConst.Setting.Tau = 0.01f;
				PointConst.Setting.Damping = 0.99f;
				Map.World.AddConstraint(PointConst);

				OldAngularDamping = PickedEntity.RigidBody.AngularDamping;
				OldLinearDamping = PickedEntity.RigidBody.LinearDamping;
				PickedEntity.RigidBody.SetDamping(OldLinearDamping, 0.99f);

				IsPickingBody = true;
			}
		}

		void DropBody() {
			if (!IsPickingBody)
				return;

			Map.World.RemoveConstraint(PointConst);
			PointConst.Dispose();
			PointConst = null;

			PickedEntity.Unfreeze();
			PickedEntity.RigidBody.SetDamping(OldLinearDamping, OldAngularDamping);
			PickedEntity = null;

			IsPickingBody = false;
		}

		public override void Update(float Dt) {
			if (!IsPickingBody)
				return;

			PointConst.PivotInB = FireOrigin + FireDirection * PickDist;
		}
	}

	//*/
}
