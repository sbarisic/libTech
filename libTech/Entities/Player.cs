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

		Vector3 Velocity;
		CylinderShape PlayerShape;
		RigidBody PlayerBody;

		bool W, A, S, D, Jump, Crouch;

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


			using (RigidBodyConstructionInfo RBInfo = new RigidBodyConstructionInfo(1, new DefaultMotionState(Matrix4x4.Identity), PlayerShape, PlayerShape.CalculateLocalInertia(1))) {
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

			const int MoveAccel = 100;

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

			Vector3 Gravity = new Vector3(0, 0, 600);
			Vector3 Movement = new Vector3(0, 0, 0);

			Velocity = Velocity * 0.9f;

			SweepResult FallRes = Map.Sweep(PlayerShape, Position, -Camera.UpNormal, 100);
			if (FallRes.Distance <= 0) {
				Velocity.Y = 0;

				if (Jump) {
					Velocity.Y = 10;
				}
			} else
				Velocity.Y -= 25 * Dt;



			//if (Jump )



			/*if (W && Map.Sweep(PlayerShape, Position, Camera.WorldForwardNormal, 100).Distance > PlayerShape.Radius)
				Movement += Camera.WorldForwardNormal * MoveSpeed * Dt;

			if (A && Map.Sweep(PlayerShape, Position, -Camera.WorldRightNormal, 100).Distance > PlayerShape.Radius)
				Movement += -Camera.WorldRightNormal * MoveSpeed * Dt;

			if (S && Map.Sweep(PlayerShape, Position, -Camera.WorldForwardNormal, 100).Distance > PlayerShape.Radius)
				Movement += -Camera.WorldForwardNormal * MoveSpeed * Dt;

			if (D && Map.Sweep(PlayerShape, Position, Camera.WorldRightNormal, 100).Distance > PlayerShape.Radius)
				Movement += Camera.WorldRightNormal * MoveSpeed * Dt;*/

			if (W)
				Movement += Camera.WorldForwardNormal * MoveAccel * Dt;

			if (A)
				Movement += -Camera.WorldRightNormal * MoveAccel * Dt;

			if (S)
				Movement += -Camera.WorldForwardNormal * MoveAccel * Dt;

			if (D)
				Movement += Camera.WorldRightNormal * MoveAccel * Dt;


			Velocity += Movement;

			Console.WriteLine(Velocity);
			Position += Velocity;
			SetPosition(Position);
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
	}
}
