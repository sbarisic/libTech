using FishGfx;
using FishGfx.Graphics;

using libTech.Map;
using libTech.Models;
using libTech.Weapons;
using libTech.Physics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using libTech.Graphics;

namespace libTech.Entities {
	public class Player : Entity {
		/*public Vector3 Position {
			get; set;
		}*/

		public Camera Camera {
			get; set;
		}

		public Camera ViewModelCamera {
			get; set;
		}

		float PlyWidth = 20;
		float PlyHeight = 46;
		float PlyEyeLevel = 46;

		PhysShape PlayerShape;
		PhysBodyDescription PlayerBody;

		bool NoClipOn = false;
		bool W, A, S, D, Jump, Crouch;
		Vector3 Velocity;
		Vector3 PrevVelocity;

		Vector3 DEBUG_WishDir;

		Vector3 GroundPoint;
		Vector3 GroundNormal;
		bool GroundPlane;
		bool Walking;

		bool LastPrimaryFire, LastSecondaryFire, LastReload;
		bool PrimaryFire, SecondaryFire, Reload;
		List<BaseWeapon> Weapons;
		BaseWeapon CurrentWeapon;

		int VelMax;

		Matrix4x4 WorldTransform;

		public Player() {
			Weapons = new List<BaseWeapon>();
			Camera = Engine.Camera3D;

			// TODO: Convar the view model FOV
			ViewModelCamera = new Camera();
			ViewModelCamera.SetPerspective(Engine.Window.WindowWidth, Engine.Window.WindowHeight, 54 * ((float)Math.PI / 180));


			//PlayerShape = new CapsuleShape(PlyWidth, PlyHeight);


			/*using (RigidBodyConstructionInfo RBInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(Matrix4x4.Identity), PlayerShape, PlayerShape.CalculateLocalInertia(0))) {
				PlayerBody = new RigidBody(RBInfo);
				PlayerBody.AngularFactor = new Vector3(0, 0, 0);

				PlayerBody.ActivationState = ActivationState.DisableDeactivation;
				PlayerBody.CollisionFlags = CollisionFlags.KinematicObject;
			}*/

			PlayerShape = PhysShape.CreateSphere(10);
			PlayerBody = new PhysBodyDescription(Engine.Map.PhysicsEngine, PlayerShape, 1);

			EnableNoclip(true);
		}

		public override void Spawned() {
			//Map.World.AddRigidBody(PlayerBody, CollisionFilterGroups.CharacterFilter, CollisionFilterGroups.AllFilter);

			Map.PhysicsEngine.AddShape(PlayerShape);
			Map.PhysicsEngine.AddBody(PlayerBody);
		}

		public virtual void OnKey(Key Key, bool Pressed, KeyMods Mods) {
			if (Key == Key.W)
				W = Pressed;

			if (Key == Key.A)
				A = Pressed;

			if (Key == Key.S)
				S = Pressed;

			if (Key == Key.D)
				D = Pressed;

			if (Key == Key.Space)
				Jump = Pressed;

			if (Key == Key.LeftControl)
				Crouch = Pressed;

			if (Key == Key.V && Pressed)
				NoClipOn = !NoClipOn;

			if (Key == Key.MouseLeft)
				PrimaryFire = Pressed;

			if (Key == Key.MouseRight)
				SecondaryFire = Pressed;

			if (Key == Key.R)
				Reload = Pressed;
		}

		public virtual bool WeaponPickUp(BaseWeapon Weapon) {
			for (int i = 0; i < Weapons.Count; i++) {
				if (Weapons[i].GetType() == Weapon.GetType()) {
					if (Weapons[i].PickUpDuplicate(Weapon))
						return true;

					return false;
				}
			}

			if (CurrentWeapon == null)
				CurrentWeapon = Weapon;

			//Weapon.Player = this;
			Weapons.Add(Weapon);
			return true;
		}

		public virtual bool WeaponDrop(BaseWeapon Weapon) {
			if (Weapons.Contains(Weapon)) {
				Weapons.Remove(Weapon);

				if (CurrentWeapon == Weapon)
					CurrentWeapon = null;

				//Weapon.Player = null;
				return true;
			}

			return false;
		}

		public virtual void MouseMove(Vector2 Dt) {
			Camera.Update(Dt);
		}

		public virtual void GetWorldTransform(out Vector3 Scale, out Quaternion Rotation, out Vector3 Position) {
			Scale = Vector3.Zero;
			Rotation = Quaternion.Identity;
			Position = Vector3.Zero;

			//PlayerBody.MotionState.WorldTransform

			// TODO
			//Matrix4x4.Decompose(PlayerBody.MotionState.WorldTransform, out Scale, out Rotation, out Position);
			Matrix4x4.Decompose(WorldTransform, out Scale, out Rotation, out Position);
		}

		public virtual void SetWorldTransform(Vector3 Scale, Quaternion Rotation, Vector3 Position) {
			// PlayerBody.WorldTransform

			// TODO
			//PlayerBody.MotionState.WorldTransform = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
			WorldTransform = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
		}

		public virtual void SetPosition(Vector3 Pos) {
			GetWorldTransform(out Vector3 Scale, out Quaternion Rotation, out Vector3 BodyPosition);
			SetWorldTransform(Scale, Rotation, Pos);
			Camera.Position = GetEyePosition(Pos);
		}

		public virtual Vector3 GetPosition() {
			GetWorldTransform(out Vector3 Scale, out Quaternion Rotation, out Vector3 BodyPosition);
			return BodyPosition;
		}

		public virtual Vector3 GetEyePosition(Vector3 Pos) {
			return Pos + new Vector3(0, PlyEyeLevel - (PlyHeight / 2), 0);
		}

		public override void Update(float Dt) {
			Vector3 Position = GetPosition();
			Vector3 EyePosition = GetEyePosition(Position);
			PrevVelocity = Velocity;

			if (CurrentWeapon != null) {
				CurrentWeapon.FireOrigin = EyePosition;
				CurrentWeapon.FireDirection = Camera.WorldForwardNormal;

				if (PrimaryFire != LastPrimaryFire) {
					CurrentWeapon.PrimaryFire(PrimaryFire);
					LastPrimaryFire = PrimaryFire;
				}

				if (SecondaryFire != LastSecondaryFire) {
					CurrentWeapon.SecondaryFire(SecondaryFire);
					LastSecondaryFire = SecondaryFire;
				}

				if (Reload != LastReload) {
					CurrentWeapon.Reload(Reload);
					LastReload = Reload;
				}
			}

			Vector3 Gravity = new Vector3(0, -50, 0);
			PM_GroundTrace(Position);

			//bool IsGrounded = Map.SweepTest(PlayerShape, Position, new Vector3(0, -1, 0), 0.25f, CollisionFilterGroups.CharacterFilter).HasHit;
			/*ContactResult HitSomething = Map.ContactTest(PlayerBody);

			bool IsGrounded = false;
			if (HitSomething.AnyNormalLower(new Vector3(0, 1, 0), 45)) {
				IsGrounded = true;
			}*/

			Vector3 WishDir = new Vector3(0, 0, 0);

			if ((W || S) && !(W && S)) {
				float Dir = W ? 1 : -1;

				if (NoClipOn) {
					WishDir += Camera.WorldForwardNormal * Dir;
				} else {
					Vector3 Fwd = Camera.WorldForwardNormal;
					Fwd.Y = 0;
					float Len = Fwd.Length();

					if (Len > 0.1f) {
						WishDir += Vector3.Normalize(new Vector3(Fwd.X, 0, Fwd.Z)) * Dir;
					}
				}
			}

			if (A)
				WishDir += -Camera.WorldRightNormal;

			if (D)
				WishDir += Camera.WorldRightNormal;


			if (NoClipOn) {
				PM_Friction(Dt);
				PM_Accelerate(Dt, WishDir, 8, 8);

				Position += Velocity;
			} else {
				//WishDir.Y = 0;
				if (!GroundPlane) {
					Velocity += Gravity * Dt;
				}

				PM_Friction(Dt);

				if (Walking) {
					if (Jump && GroundPlane) {
						Velocity.Y += 10;
					}

					// Walking
					PM_Accelerate(Dt, WishDir, 8, 8);
				} else {
					// Air movement
					PM_Accelerate(Dt, WishDir, 8, 1);
				}

				// Collision response

				SweepResult Res = Map.PhysicsEngine.SweepTest(PlayerShape, Position, Position + Velocity);
				if (Res.HasHit) {
					Velocity = Utils.Slide(Velocity, Res.Normal);
					Position = Res.HitCenterOfMass;
				}

				Position += Velocity;
			}

			if (WishDir.X != 0 && WishDir.Y != 0 && WishDir.Z != 0)
				DEBUG_WishDir = WishDir;

			SetPosition(Position);
		}

		void PM_GroundTrace(Vector3 Position) {
			SweepResult Result = Map.PhysicsEngine.SweepTest(PlayerShape, Position, new Vector3(0, -1, 0), 0.25f);

			if (!Result.HasHit) {
				// TODO: Ground trace missed
				GroundPlane = false;
				Walking = false;
				return;
			}

			GroundPoint = Result.HitPoint;
			GroundNormal = Result.Normal;
			float FloorAngle = Vector3.Dot(Vector3.UnitY, GroundNormal); // 1 - normal looking up, -1 normal looking down;

			// Floor steeper than 45°
			if (FloorAngle < 0.5f) {
				GroundPlane = true;
				Walking = false;
				return;
			}

			GroundPlane = true;
			Walking = true;
		}

		Vector3 PM_ClipVelocity(Vector3 In, Vector3 Normal, float OverBounce = 1.001f) {
			float Backoff = Vector3.Dot(In, Normal);

			if (Backoff < 0)
				Backoff *= OverBounce;
			else
				Backoff /= OverBounce;

			Vector3 Change = Normal * Backoff;
			return In - Change;
		}

		Vector3 ProjectToNormal(Vector3 D, Vector3 N) {
			float T = Vector3.Dot(D, N);
			return D - (N * T);
		}

		Vector3 ProjectToNormal2(Vector3 D, Vector3 N) {
			return Utils.Slide(D, N);
		}

		void PM_Jump(float Dt, Vector3 JumpDir, float JumpForce) {
			Velocity += JumpDir * JumpForce;
		}

		void PM_Accelerate(float Dt, Vector3 WishDir, float WishSpeed, float Accel) {
			if (WishDir.X == 0 && WishDir.Y == 0 && WishDir.Z == 0)
				return;

			float CurSpeed = Vector3.Dot(Velocity, WishDir);
			float AddSpeed = WishSpeed - CurSpeed;

			if (AddSpeed <= 0) {
				return;
			}

			float AccelSpeed = Accel * WishSpeed * Dt;
			if (AccelSpeed > AddSpeed) {
				AccelSpeed = AddSpeed;
			}

			Velocity += AccelSpeed * WishDir;
		}

		void PM_Friction(float Dt) {
			float pm_stopspeed = 1;
			float pm_friction = 9;
			float Speed = Velocity.Length();

			if (!NoClipOn) {
				if (Walking) {
					Velocity.Y = 0;
				}
			}

			if (!NoClipOn && !GroundPlane)
				return;

			if (Speed < 0.1f) {
				Velocity = Vector3.Zero;
				return;
			}

			float Control = Speed < pm_stopspeed ? pm_stopspeed : Speed;
			float Drop = Control * pm_friction * Dt;

			float NewSpeed = Speed - Drop;
			if (NewSpeed < 0)
				NewSpeed = 0;

			NewSpeed /= Speed;
			Velocity = Velocity * NewSpeed;
		}

		public override void DrawOpaque() {
			RenderAPI.DbgPushGroup("Player DrawOpaque");

			/*for (int i = 0; i < HitSomething.CollisionPointCount; i++) {
				Vector3 A = HitSomething.CollisionPointsWorld[i];
				Vector3 B = A + HitSomething.CollisionNormals[i] * 5;

				DbgDraw.DrawArrow(A, B, Color.Blue, 3);
			}*/

			if (GroundPlane) {
				DbgDraw.DrawArrow(GroundPoint, GroundPoint + GroundNormal * 16, Color.Blue, 2);

				DbgDraw.DrawArrow(GroundPoint, GroundPoint + DEBUG_WishDir * 32, Color.Orange, 2);
			}

			RenderAPI.DbgPopGroup();
		}

		public virtual void DrawViewModel() {
			RenderAPI.DbgPushGroup("Player DrawViewModel");

			ViewModelCamera.Position = Camera.Position;
			ViewModelCamera.Rotation = Camera.Rotation;

			Camera OldCam = ShaderUniforms.Current.Camera;
			ShaderUniforms.Current.Camera = ViewModelCamera;
			CurrentWeapon?.DrawViewModel(this);
			ShaderUniforms.Current.Camera = OldCam;

			RenderAPI.DbgPopGroup();
		}

		public virtual void DrawGUI() {
			Vector3 VelH = new Vector3(Velocity.X, 0, Velocity.Z);
			int Vel = (int)VelH.Length();

			if (Vel > VelMax)
				VelMax = Vel;

			Gfx.DrawText(Engine.UI.DefaultFont, (Engine.Window.WindowSize / 2) + new Vector2(0, -50), Vel.ToString(), Color.White, 32);
			Gfx.DrawText(Engine.UI.DefaultFont, (Engine.Window.WindowSize / 2) + new Vector2(0, -50 - 32), VelMax.ToString(), Color.Green, 32);
		}

		public virtual void EnableNoclip(bool Enable) {
			NoClipOn = Enable;
		}
	}
}
