using libTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Entities {
	public class EntStatic : Entity {
		public libTechModel Model;

		public virtual Vector3 Position {
			get {
				return Model.Position;
			}

			set {
				Model.Position = value;
			}
		}

		public virtual Quaternion Rotation {
			get {
				return Model.Rotation;
			}

			set {
				Model.Rotation = value;
			}
		}

		public EntStatic(libTechModel Model) {
			this.Model = Model;
		}

		public override void DrawOpaque() {
			Model.DrawOpaque();
		}

		public override void DrawTransparent() {
			Model.DrawTransparent();
		}
	}
}
