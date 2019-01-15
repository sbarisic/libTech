using FishGfx.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.GUI {
	public class libGUIEventArgs : EventArgs {
		public libGUI GUI;
		public bool Consumed;

		public libGUIEventArgs(libGUI GUI) {
			this.GUI = GUI;
			Consumed = false;
		}
	}

	public class OnMouseMoveEventArgs : libGUIEventArgs {
		public Vector2 Pos;

		public OnMouseMoveEventArgs(libGUI GUI, Vector2 Pos) : base(GUI) {
			this.Pos = Pos;
		}
	}

	public class OnKeyEventArgs : libGUIEventArgs {
		public Vector2 MousePos;
		public bool Pressed;
		public Key Key;

		public OnKeyEventArgs(libGUI GUI, Vector2 MousePos, bool Pressed, Key Key) : base(GUI) {
			this.MousePos = MousePos;
			this.Pressed = Pressed;
			this.Key = Key;
		}
	}
}
