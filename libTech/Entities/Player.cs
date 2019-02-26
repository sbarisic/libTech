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
		public Vector3 Position { get; set; }
		public Camera Camera { get; set; }
		public Camera ViewModelCamera { get; set; }

		bool W, A, S, D;
		SphereShape PlayerShape;

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

			PlayerShape = new SphereShape(20);
			PlayerShape.GetAabb(Matrix4x4.Identity, out Vector3 AABBMin, out Vector3 AABBMax);
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

		public virtual void SetPosition(Vector3 Pos) {
			Position = Pos;
			Camera.Position = Pos;
		}

		public override void Update(float Dt) {
			const int MoveSpeed = 600;

			if (CurrentWeapon != null) {
				CurrentWeapon.FireOrigin = Position;
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

			if (W && Map.Sweep(PlayerShape, Position, Camera.WorldForwardNormal, 100).Distance > PlayerShape.Radius)
				Position += Camera.WorldForwardNormal * MoveSpeed * Dt;

			if (A && Map.Sweep(PlayerShape, Position, -Camera.WorldRightNormal, 100).Distance > PlayerShape.Radius)
				Position += -Camera.WorldRightNormal * MoveSpeed * Dt;

			if (S && Map.Sweep(PlayerShape, Position, -Camera.WorldForwardNormal, 100).Distance > PlayerShape.Radius)
				Position += -Camera.WorldForwardNormal * MoveSpeed * Dt;

			if (D && Map.Sweep(PlayerShape, Position, Camera.WorldRightNormal, 100).Distance > PlayerShape.Radius)
				Position += Camera.WorldRightNormal * MoveSpeed * Dt;

			Camera.Position = Position;
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
