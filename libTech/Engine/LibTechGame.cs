using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using libTech;
using libTech.Graphics;
using libTech.GUI;

namespace libTech {
	public abstract class LibTechGame {
		public virtual void Load() {
		}

		public virtual void Unload() {
		}

		public virtual void Update(float Dt) {
		}

		public virtual void Draw(float Dt) {
		}

		public virtual void DrawTransparent(float Dt) {
		}

		public virtual void DrawGUI(float Dt) {
		}
	}
}
