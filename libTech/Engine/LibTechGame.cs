using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	public abstract class LibTechGame {
		public virtual void Load() {
		}

		public virtual void Unload() {
		}

		public abstract void Update(float Dt);
		public abstract void Draw(float Dt);
		public abstract void DrawTransparent(float Dt);
		public abstract void DrawGUI(float Dt);
	}
}
