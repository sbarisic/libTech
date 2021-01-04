using BulletSharp;

using FishGfx;
using FishGfx.Graphics;

using libTech.Map;
using libTech.Models;
using libTech.Weapons;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

		float PlyWidth = 32;
		float PlyHeight = 72;
		float PlyEyeLevel = 64;

		CylinderShape PlayerShape;
		RigidBody PlayerBody;

		bool W, A, S, D, Jump, Crouch;
		Vector3 Velocity;

		bool LastPrimaryFire, LastSecondaryFire, LastReload;
		bool PrimaryFire, SecondaryFire, Reload;
		List<BaseWeapon> Weapons;
		BaseWeapon CurrentWeapon;

		public Player() {
			Weapons = new List<BaseWeapon>();
			Camera = Engine.Camera3D;

			// TODO: Convar the view model FOV
			ViewModelCamera = new Camera();
			ViewModelCamera.SetPerspective(Engine.Window.WindowWidth, Engine.Window.WindowHeight, 54 * ((float)Math.PI / 180));

			PlayerShape = new CylinderShape(PlyWidth / 2, PlyHeight / 2, PlyWidth / 2);
			//PlayerShape.GetAabb(Matrix4x4.Identity, out Vector3 AABBMin, out Vector3 AABBMax);
			//PlayerShape = new SphereShape(PlyHeight / 2);


			using (RigidBodyConstructionInfo RBInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(Matrix4x4.Identity), PlayerShape, PlayerShape.CalculateLocalInertia(0))) {
				PlayerBody = new RigidBody(RBInfo);
				PlayerBody.AngularFactor = new Vector3(0, 0, 0);

				PlayerBody.ActivationState = ActivationState.DisableDeactivation;
				PlayerBody.CollisionFlags = CollisionFlags.KinematicObject;
			}
		}

		public override void Spawned() {
			Map.World.AddRigidBody(PlayerBody);
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
			//PlayerBody.MotionState.WorldTransform

			Matrix4x4.Decompose(PlayerBody.MotionState.WorldTransform, out Scale, out Rotation, out Position);
		}

		public virtual void SetWorldTransform(Vector3 Scale, Quaternion Rotation, Vector3 Position) {
			// PlayerBody.WorldTransform
			PlayerBody.MotionState.WorldTransform = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
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
			Velocity += Gravity * Dt;


			bool IsGrounded = Map.Sweep(PlayerShape, Position, new Vector3(0, -1, 0), 1).HasHit;
			bool IsHittingHead = Map.Sweep(PlayerShape, Position, new Vector3(0, 1, 0), 1).HasHit;

			if (IsHittingHead)
			Console.WriteLine("Hitting head " + IsGrounded);

			PM_Friction(Dt, IsGrounded);
			Vector3 WishDir = new Vector3(0, 0, 0);

			if (W)
				WishDir += Camera.WorldForwardNormal;

			if (A)
				WishDir += -Camera.WorldRightNormal;

			if (S)
				WishDir += -Camera.WorldForwardNormal;

			if (D)
				WishDir += Camera.WorldRightNormal;

			WishDir.Y = 0;

			if (WishDir.Length() > 0.5)
				WishDir = Vector3.Normalize(WishDir);

			/*if (IsHittingHead && !IsGrounded) {
				if (Velocity.Y > 0)
					Velocity.Y = 0;
			}*/

			if (IsGrounded) {
				if (Velocity.Y < 0)
					Velocity.Y = 0;

				if (Jump) {
					Velocity.Y += 10;
				}

				// Walking
				PM_Accelerate(Dt, WishDir, 8, 8);
			} else {
				// Air movement
				PM_Accelerate(Dt, WishDir, 8, 1);
			}



			/*float MaxVel = 20;
			if ((Velocity + WishDir).Length() < MaxVel) {
				Velocity += WishDir;
			}*/

			//Console.WriteLine(Velocity.Length());
			//Console.WriteLine(Velocity);

			Position += Velocity;
			SetPosition(Position);
		}

		void PM_Jump(float Dt, Vector3 JumpDir, float JumpForce) {
			Velocity += JumpDir * JumpForce;
		}

		void PM_Accelerate(float Dt, Vector3 WishDir, float WishSpeed, float Accel) {
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

		void PM_Friction(float Dt, bool IsGrounded) {
			float pm_stopspeed = 1;
			float pm_friction = 9;

			float Speed = Velocity.Length();

			if (!IsGrounded) {
				return;
			}

			if (Speed < 1) {
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

		int VelMax;

		public virtual void DrawGUI() {
			int Vel = (int)Velocity.Length();

			if (Vel > VelMax)
				VelMax = Vel;

			Gfx.DrawText(Engine.UI.DefaultFont, (Engine.Window.WindowSize / 2) + new Vector2(0, -50), Vel.ToString(), Color.White, 32);
			Gfx.DrawText(Engine.UI.DefaultFont, (Engine.Window.WindowSize / 2) + new Vector2(0, -50 - 32), VelMax.ToString(), Color.Green, 32);
		}
	}
}
