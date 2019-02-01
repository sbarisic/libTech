using BulletSharp;
using FishGfx;
using FishGfx.Graphics;
using libTech.Map;
using libTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Entities {
	public class Player : Entity {
		public Vector3 Position { get; set; }

		bool W, A, S, D, PerformPick;
		SphereShape PlayerShape;

		RigidBody PickedBody;
		Point2PointConstraint PointCst;
		float PickDist;

		public Player() {
			//PlayerShape = new CapsuleShape(30, 44);
			PlayerShape = new SphereShape(20);

			PlayerShape.GetAabb(Matrix4x4.Identity, out Vector3 AABBMin, out Vector3 AABBMax);

			// TODO: Remove
			BarrelModel = Engine.Load<libTechModel>("models/props_c17/oildrum001_explosive.mdl");
			BarrelModel.CenterModel();
		}

		// TODO: Remove
		libTechModel BarrelModel;

		public virtual void OnKey(Key Key, bool Pressed, KeyMods Mods) {
			if (Key == Key.W)
				W = Pressed;
			if (Key == Key.A)
				A = Pressed;
			if (Key == Key.S)
				S = Pressed;
			if (Key == Key.D)
				D = Pressed;

			if (Key == Key.MouseLeft)
				PerformPick = Pressed;

			// TODO: Remove
			if (Pressed && Key == Key.MouseRight) {
				if (Map.RayCast(Position, Position + Engine.Camera3D.WorldForwardNormal * 1000, out Vector3 HitPos, out Vector3 HitNormal, out RigidBody Body)) {
					EntPhysics Barrel = EntPhysics.FromModel(BarrelModel, 1);
					Barrel.SetPosition(HitPos + new Vector3(0, 20, 0));
					Map.SpawnEntity(Barrel);
				}
			}
		}

		public virtual void MouseMove(Vector2 Dt) {
			Engine.Camera3D.Update(Dt);
		}

		public override void Update(float Dt) {
			const int MoveSpeed = 600;

			if (PerformPick) {
				if (PickedBody == null) {
					Console.WriteLine("Picking!");

					PickedBody = Map.RayCastBody(Position, Position + Engine.Camera3D.WorldForwardNormal * 1000, out Vector3 PickPoint);
					if (PickedBody != null) {
						PickDist = Vector3.Distance(Position, PickPoint);
						Console.WriteLine("Distance: {0}", PickDist);

						Matrix4x4.Invert(PickedBody.CenterOfMassTransform, out Matrix4x4 InvCenterOfMass);
						PickPoint = Vector3.Transform(PickPoint, InvCenterOfMass);

						PickedBody.ActivationState = ActivationState.DisableDeactivation;

						PointCst = new Point2PointConstraint(PickedBody, PickPoint);
						PointCst.Setting.ImpulseClamp = 30;
						PointCst.Setting.Tau = 0.001f;
						Map.World.AddConstraint(PointCst);
					}
				}
			} else {
				if (PointCst != null) {
					Console.WriteLine("Dropping!");

					Map.World.RemoveConstraint(PointCst);
					PointCst.Dispose();
					PointCst = null;

					PickedBody.ForceActivationState(ActivationState.ActiveTag);
					PickedBody.DeactivationTime = 0;
					PickedBody = null;
				}
			}

			if (PointCst != null) {
				//PointCst.PivotInA = Position;
				PointCst.PivotInB = Position + Engine.Camera3D.WorldForwardNormal * PickDist;
			}

			if (W && Map.Sweep(PlayerShape, Position, Engine.Camera3D.WorldForwardNormal, 100).Distance > PlayerShape.Radius)
				Position += Engine.Camera3D.WorldForwardNormal * MoveSpeed * Dt;

			if (A && Map.Sweep(PlayerShape, Position, -Engine.Camera3D.WorldRightNormal, 100).Distance > PlayerShape.Radius)
				Position += -Engine.Camera3D.WorldRightNormal * MoveSpeed * Dt;

			if (S && Map.Sweep(PlayerShape, Position, -Engine.Camera3D.WorldForwardNormal, 100).Distance > PlayerShape.Radius)
				Position += -Engine.Camera3D.WorldForwardNormal * MoveSpeed * Dt;

			if (D && Map.Sweep(PlayerShape, Position, Engine.Camera3D.WorldRightNormal, 100).Distance > PlayerShape.Radius)
				Position += Engine.Camera3D.WorldRightNormal * MoveSpeed * Dt;

			Engine.Camera3D.Position = Position;
		}
	}
}
