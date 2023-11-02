//using BulletSharp;

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuPhysics.Trees;

using BepuUtilities;

using FishGfx.Graphics;

using libTech.Entities;
using libTech.Graphics;
using libTech.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Weapons {
	
	 
	public class UtilityGun : BaseWeapon {
		Grabber Grb;

		public float MaxPickDistance;

		EntPhysics PickedEntity;
		//Point2PointConstraint PointConst;
		bool IsPickingBody;

		float PickDist;
		float OldAngularDamping;
		float OldLinearDamping;

		Vector3 GlobalPickPoint;
		Vector3 LocalPickPoint;

		bool LeftMouseDown;

		public UtilityGun() {
			MaxPickDistance = 2000;
			//ViewModel = Engine.Load<libTechModel>("/models/weapons/v_physcannon.mdl");
			ViewModelTranslation = new Vector3(0, 0, -20);
		}

		public override void PrimaryFire(bool Pressed) {
			LeftMouseDown = Pressed;

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
			if (Engine.Map.PhysicsEngine.RayCast(From, To, out Vector3 HitPos, out Vector3 Normal, out object Body)) {
				DbgDraw.DrawLine(From, HitPos, Time: 30000);
				return HitPos;
			}

			DbgDraw.DrawLine(From, To, Time: 30000);
			return To;
		}

		EntPhysics RayCastEntity(out Vector3 PickPoint, out Vector3 Normal) {
			Engine.Map.PhysicsEngine.RayCast(FireOrigin, FireOrigin + FireDirection * MaxPickDistance, out PickPoint, out Normal, out object Body);
			//return Body?.UserObject as EntPhysics;

			return Body as EntPhysics;

			//PickPoint = Vector3.Zero;
			//Normal = Vector3.Zero;
			//return null;
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

				//


				IsPickingBody = true;
			}
		}

		void DropBody() {
			if (!IsPickingBody)
				return;

	
			//

			IsPickingBody = false;
		}

		public override void Update(float Dt) {
			//if (!IsPickingBody)
			//	return;


			Grb.Update(Engine.Map.PhysicsEngine.GetSimulation(), Engine.Camera3D, true, LeftMouseDown, Quaternion.Identity, new Vector2(0.5f, 0.5f));

			//
		}
	}

	//*/





	struct Grabber {
		bool active;
		BodyReference body;
		float t;
		Vector3 localGrabPoint;
		Quaternion targetOrientation;
		ConstraintHandle linearMotorHandle;
		ConstraintHandle angularMotorHandle;

		struct RayHitHandler : IRayHitHandler {
			public float T;
			public CollidableReference HitCollidable;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool AllowTest(CollidableReference collidable) {
				return true;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool AllowTest(CollidableReference collidable, int childIndex) {
				return true;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex) {
				//We are only interested in the earliest hit. This callback is executing within the traversal, so modifying maximumT informs the traversal
				//that it can skip any AABBs which are more distant than the new maximumT.
				maximumT = t;
				//Cache the earliest impact.
				T = t;
				HitCollidable = collidable;
			}
		}

		readonly void CreateMotorDescription(Vector3 target, float inverseMass, out OneBodyLinearServo linearDescription, out OneBodyAngularServo angularDescription) {
			linearDescription = new OneBodyLinearServo {
				LocalOffset = localGrabPoint,
				Target = target,
				ServoSettings = new ServoSettings(float.MaxValue, 0, 360 / inverseMass),
				SpringSettings = new SpringSettings(5, 2),
			};

			angularDescription = new OneBodyAngularServo {
				TargetOrientation = targetOrientation,
				ServoSettings = new ServoSettings(float.MaxValue, 0, localGrabPoint.Length() * 180 / inverseMass),
				SpringSettings = new SpringSettings(5, 2),
			};
		}

		public void Update(Simulation simulation, Camera camera, bool mouseLocked, bool shouldGrab, Quaternion rotation, in Vector2 normalizedMousePosition) {
			//On the off chance some demo modifies the kinematic state, treat that as a grab terminator.
			bool bodyExists = body.Exists && !body.Kinematic;

			if (active && (!shouldGrab || !bodyExists)) {

				active = false;
				if (bodyExists) {
					//If the body wasn't removed, then the constraint should be removed.
					//(Body removal forces connected constraints to removed, so in that case we wouldn't have to worry about it.)
					simulation.Solver.Remove(linearMotorHandle);
					if (!Bodies.HasLockedInertia(body.LocalInertia.InverseInertiaTensor))
						simulation.Solver.Remove(angularMotorHandle);
				}
				body = new BodyReference();

			} else if (shouldGrab && !active) {

				//Vector3 rayDirection = camera.GetRayDirection(mouseLocked, normalizedMousePosition);
				Vector3 rayDirection = camera.WorldForwardNormal;

				var hitHandler = default(RayHitHandler);

				hitHandler.T = float.MaxValue;
				simulation.RayCast(camera.Position, rayDirection, float.MaxValue, ref hitHandler);
				if (hitHandler.T < float.MaxValue && hitHandler.HitCollidable.Mobility == CollidableMobility.Dynamic) {
					//Found something to grab!
					t = hitHandler.T;
					body = simulation.Bodies[hitHandler.HitCollidable.BodyHandle];
					var hitLocation = camera.Position + rayDirection * t;
					RigidPose.TransformByInverse(hitLocation, body.Pose, out localGrabPoint);
					targetOrientation = body.Pose.Orientation;
					active = true;
					CreateMotorDescription(hitLocation, body.LocalInertia.InverseMass, out var linearDescription, out var angularDescription);
					linearMotorHandle = simulation.Solver.Add(body.Handle, linearDescription);
					if (!Bodies.HasLockedInertia(body.LocalInertia.InverseInertiaTensor))
						angularMotorHandle = simulation.Solver.Add(body.Handle, angularDescription);
				}

			} else if (active) {

				//Vector3 rayDirection = camera.GetRayDirection(mouseLocked, normalizedMousePosition);
				Vector3 rayDirection = camera.WorldForwardNormal;

				var targetPoint = camera.Position + rayDirection * t;
				targetOrientation = QuaternionEx.Normalize(QuaternionEx.Concatenate(targetOrientation, rotation));

				CreateMotorDescription(targetPoint, body.LocalInertia.InverseMass, out var linearDescription, out var angularDescription);
				simulation.Solver.ApplyDescription(linearMotorHandle, linearDescription);
				if (!Bodies.HasLockedInertia(body.LocalInertia.InverseInertiaTensor))
					simulation.Solver.ApplyDescription(angularMotorHandle, angularDescription);
				body.Activity.TimestepsUnderThresholdCount = 0;

			}
		}

		/*public void Draw(LineExtractor lines, Camera camera, bool mouseLocked, bool shouldGrab, in Vector2 normalizedMousePosition) {
			if (shouldGrab && !active && mouseLocked) {

				//Draw a crosshair if there is no mouse cursor.
				var center = camera.Position + camera.Forward * (camera.NearClip * 10);
				var crosshairLength = 0.1f * camera.NearClip * MathF.Tan(camera.FieldOfView * 0.5f);
				var rightOffset = camera.Right * crosshairLength;
				var upOffset = camera.Up * crosshairLength;
				lines.Allocate() = new LineInstance(center - rightOffset, center + rightOffset, new Vector3(1, 0, 0), new Vector3());
				lines.Allocate() = new LineInstance(center - upOffset, center + upOffset, new Vector3(1, 0, 0), new Vector3());

			}
		}*/
	}


}
