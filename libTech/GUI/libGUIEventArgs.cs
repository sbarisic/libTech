using FishGfx.Graphics;
using libTech.GUI.Controls;
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

		public bool IsMouseKey {
			get {
				return Key >= Key.MouseButton1 && Key <= Key.MouseButton8;
			}
		}

		public OnKeyEventArgs(libGUI GUI, Vector2 MousePos, bool Pressed, Key Key) : base(GUI) {
			this.MousePos = MousePos;
			this.Pressed = Pressed;
			this.Key = Key;
		}
	}

	public class OnButtonClickEventArgs : libGUIEventArgs {
		public OnButtonClickEventArgs(libGUI GUI) : base(GUI) {
		}
	}

	public delegate void OnButtonClickFunc(object Sender, OnButtonClickEventArgs Args);
}
