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
	// TODO: Generic button class which inherits non-generic button
	// Generic type would be a GUIControl which represents what's inside the button
	// Text button, image button, spinner button, video button....

	public class TextButton : GUIControl {
		Vector2 Padding = new Vector2(5);
		Label Lbl;
		NineSlice ButtonSkin;

		Vector2 _Size;
		public override Vector2 Size {
			//get => Lbl.Size;
			get {
				return _Size;
			}

			set {
				_Size = (value).Max(Lbl.Size);
				Lbl.Center(Size / 2);
			}
		}

		public TextButton(FreetypeFont Font, string Text) {
			ButtonSkin = new NineSlice(DefaultTextures.Button, Padding.X);

			Lbl = AddChild(new Label(Font));
			Lbl.String = Text;
			Size = Lbl.Size;

			UseScissor = false;
		}

		public TextButton(FreetypeFont Font, string Text, float Height) : this(Font, Text) {
			Size = new Vector2(Size.X, Height - Padding.Y * 2);
		}

		public override bool IsInside(Vector2 Pos) {
			return new AABB(GlobalPosition - Padding, Size + Padding * 2).IsInside(Pos);
		}

		public override bool GetItemAt(Vector2 Pos, out GUIControl Ctrl) {
			if (IsInside(Pos)) {
				Ctrl = this;
				return true;
			}

			Ctrl = null;
			return false;
		}

		internal override void SendOnKey(Key K, Vector2 Pos, bool Pressed) {
			if (K == Key.MouseLeft) {
				if (Pressed)
					ButtonSkin.Texture = DefaultTextures.ButtonClick;
				else {
					if (IsInside(Pos))
						ButtonSkin.Texture = DefaultTextures.ButtonHover;
					else
						ButtonSkin.Texture = DefaultTextures.Button;
				}
			}

			base.SendOnKey(K, Pos, Pressed);
		}

		internal override void SendOnMouseEnter(bool Entered) {
			if (Entered)
				ButtonSkin.Texture = DefaultTextures.ButtonHover;
			else
				ButtonSkin.Texture = DefaultTextures.Button;

			base.SendOnMouseEnter(Entered);
		}

		public override void Draw() {
			ButtonSkin.Position = GlobalPosition;
			ButtonSkin.Size = Size;
			ButtonSkin.Draw();

			base.Draw();
		}
	}
}