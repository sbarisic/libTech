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
	public class TextButton : GUIControl {
		readonly Vector2 Padding = new Vector2(5);

		Label Lbl;
		NineSlice ButtonSkin;

		public override Vector2 Size {
			get => Lbl.Size + Padding * 2;
			set => Lbl.Size = value - Padding * 2;
		}

		public TextButton(FreetypeFont Font, string Text) {
			Lbl = AddChild(new Label(Font, Text));
			Lbl.Position += Padding;

			ButtonSkin = new NineSlice(DefaultTextures.Button, Padding.X);
		}

		public override bool IsInside(Vector2 Pos) {
			return base.IsInside(Pos);
		}

		public override bool GetItemAt(Vector2 Pos, out GUIControl Ctrl) {
			if (IsInside(Pos)) {
				Ctrl = this;
				return true;
			}

			Ctrl = null;
			return false;
		}

		internal override void OnMousePress(Key K, Vector2 Pos) {
			ButtonSkin.Texture = DefaultTextures.ButtonClick;

			base.OnMousePress(K, Pos);
		}

		internal override void OnMouseRelease(Key K, Vector2 Pos) {
			if (IsInside(Pos))
				ButtonSkin.Texture = DefaultTextures.ButtonHover;
			else
				ButtonSkin.Texture = DefaultTextures.Button;

			base.OnMouseRelease(K, Pos);
		}

		internal override void OnMouseEnter(bool Entered) {
			if (Entered)
				ButtonSkin.Texture = DefaultTextures.ButtonHover;
			else
				ButtonSkin.Texture = DefaultTextures.Button;

			base.OnMouseEnter(Entered);
		}

		public override void Draw() {
			ButtonSkin.Position = new Vector3(GlobalPosition, 0);
			ButtonSkin.Size = Size;
			ButtonSkin.Draw();

			base.Draw();
		}
	}
}