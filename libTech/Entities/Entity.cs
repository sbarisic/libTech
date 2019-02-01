using libTech.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Entities {
	public abstract class Entity {
		public libTechMap Map;

		public virtual void Spawned() {
		}

		public virtual void Update(float Dt) {
		}

		public virtual void DrawOpaque() {
		}

		public virtual void DrawTransparent() {
		}
	}
}
