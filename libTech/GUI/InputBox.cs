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
	public delegate void OnTextEnteredAction(InputBox Input, string Text);

	public class InputBox : Label {
		public event OnTextEnteredAction OnTextEntered;

		StringBuilder InputString = new StringBuilder();
		bool Dirty;

		public override string String {
			get {
				return InputString.ToString();
			}
			set {
				InputString.Clear();
				InputString.Append(value);
				Dirty = true;
			}
		}

		string _Prompt;
		public string Prompt {
			get {
				return _Prompt;
			}
			set {
				if (_Prompt == value)
					return;

				_Prompt = value;
				Dirty = true;
			}
		}

		public InputBox(FreetypeFont Font) : base(Font) {
			Prompt = "> ";
		}

		internal override void SendOnChar(string Char, uint Unicode) {
			InputString.Append(Char);
			Dirty = true;
			base.SendOnChar(Char, Unicode);
		}

		internal override void SendOnPaste(string String) {
			String = String.Replace("\r", "");

			if (!Multiline)
				String = String.Replace("\n", "");

			if (String.Length == 0)
				return;

			InputString.Append(String);
			Dirty = true;
		}

		internal override string SendOnCopy() {
			return InputString.ToString();
		}

		internal override string SendOnCut() {
			string Str = SendOnCopy();
			InputString.Length = 0;
			Dirty = true;
			return Str;
		}

		internal override void SendOnKey(Key K, Vector2 Pos, bool Pressed) {
			if (Pressed) {
				if (K == Key.Backspace && InputString.Length > 0) {
					InputString.Length--;
					Dirty = true;
					return;
				}

				if (K == Key.Enter || K == Key.NumpadEnter) {
					OnTextEntered?.Invoke(this, String);
					return;
				}
			}

			base.SendOnKey(K, Pos, Pressed);
		}

		void UpdateInput() {
			if (!Dirty)
				return;
			Dirty = false;
			
			Text.String = Prompt + String + "_";
			Text.Refresh();
		}

		public override void Draw() {
			UpdateInput();
			base.Draw();
		}
	}
}
