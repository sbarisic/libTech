using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace libTech.Entities {
	public abstract class Entity {
		public abstract Vector3 Position { get; set; }

		public virtual void Update(float Dt) {
		}

		public virtual void Draw(float Dt) {
		}

		public virtual void DrawTransparent(float Dt) {
		}
	}
}
