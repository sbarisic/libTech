using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;

namespace libTech.Graphics {
	public class TextGfx {
		bool Dirty;

		string _Text;
		public string Text {
			get {
				return _Text;
			}
			set {
				if (_Text == value)
					return;

				_Text = value;
				Dirty = true;
			}
		}

		Vector2 _Position;
		public Vector2 Position {
			get {
				return _Position;
			}
			set {
				if (_Position == value)
					return;

				_Position = value;
				Dirty = true;
			}
		}

		Color _Color;
		public Color Color {
			get {
				return _Color;
			}
			set {
				_Color = value;
				Dirty = true;
			}
		}


		public TextGfx(string Text, Vector2 Position, Color Color) {
			this.Text = Text;
			this.Position = Position;
			this.Color = Color;
		}

		void Refresh() {

		}

		public void Draw() {
			if (Dirty) {
				Dirty = false;
				Refresh();
			}


		}
	}
}
