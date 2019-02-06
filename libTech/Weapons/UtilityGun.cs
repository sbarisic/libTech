using BulletSharp;
using libTech.Entities;
using libTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Weapons {
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
			ViewModel = Engine.Load<libTechModel>("/models/weapons/v_physcannon.mdl");
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
			}
		}

		EntPhysics RayCastEntity(out Vector3 PickPoint) {
			return Map.RayCastBody(FireOrigin, FireOrigin + FireDirection * MaxPickDistance, out PickPoint)?.UserObject as EntPhysics;
		}

		void PickBody() {
			DropBody();
			PickedEntity = RayCastEntity(out Vector3 PickPoint);

			if (PickedEntity != null) {
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
}
