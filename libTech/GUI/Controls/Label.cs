using FishGfx;
using FishGfx.Graphics;
using libTech.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.GUI.Controls {
	public class Label : Control {
		bool Dirty;

		string _Text;
		public string Text {
			get {
				return _Text;
			}

			set {
				if (_Text != value) {
					Dirty = true;
					_Text = value;
				}
			}
		}

		public Label(GfxFont Font) {

		}

		void Refresh() {

		}

		public override void Draw() {
			if (Dirty) {
				Dirty = false;
				Refresh();
			}

			base.Draw();
		}
	}
}
