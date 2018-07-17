using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using libTech.Graphics;
using FishGfx;
using FishGfx.Graphics;

namespace libTech.GUI {
	public class Label : GUIControl {
		Text Text;

		public string String {
			get {
				return Text.String;
			}
			set {
				Text.String = value;
				Text.Refresh();
				Size = this.Text.StringSize;
			}
		}

		public Label(FreetypeFont Font, string Txt) {
			this.Text = new Text(Font, Txt);
			String = Txt;
		}

		public override void Draw() {
			Text.Position = new Vector3(GlobalPosition, 0);
			Text.Draw();
		}
	}
}
