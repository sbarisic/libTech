using libTech.Entities;
using libTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Weapons {
	public abstract class BaseWeapon : Entity {
		public virtual Vector3 FireOrigin { get; set; }
		public virtual Vector3 FireDirection { get; set; }

		public virtual Vector3 ViewModelTranslation { get; set; }
		public virtual libTechModel ViewModel { get; set; }

		public virtual void PrimaryFire(bool Pressed) {
		}

		public virtual void SecondaryFire(bool Pressed) {
		}

		public virtual void Reload(bool Pressed) {
		}

		public virtual bool PickUpDuplicate(BaseWeapon Other) {
			return false;
		}

		public virtual void CalcViewModelTranslation(Player Ply) {
			float Pi = (float)Math.PI;
			float HalfPi = Pi / 2;

			ViewModel.Position = Ply.Camera.Position + Ply.Camera.ToWorld(new Vector3(0, -68, 10) + ViewModelTranslation);
			ViewModel.Rotation = Ply.Camera.Rotation * Quaternion.CreateFromAxisAngle(Vector3.UnitX, -HalfPi) * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, Pi);
		}

		public virtual void DrawViewModel(Player Ply) {
			if (Ply != null && ViewModel != null) {
				CalcViewModelTranslation(Ply);
				ViewModel.Draw();
			}
		}
	}
}
