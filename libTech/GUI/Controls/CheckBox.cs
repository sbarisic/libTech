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
	public class CheckBox : Label {
		public event OnCheckBoxToggledFunc OnToggled;

		public virtual bool Checked { get; set; }

		public virtual float CheckBoxButtonSpacing { get; set; }
		public virtual Vector2 CheckBoxSize { get; set; }

		public override Vector2 Size {
			get {
				Vector2 S = base.Size + new Vector2(CheckBoxSize.X + CheckBoxButtonSpacing, 0);
				S.Y = Math.Max(S.Y, CheckBoxSize.Y);
				return S;
			}
			set {
				base.Size = value;
			}
		}

		public CheckBox(libGUI GUI, GfxFont Font) : base(GUI, Font) {
			CheckBoxSize = GUI.CheckBoxSkin.Size;
			CheckBoxButtonSpacing = 5;
		}

		public CheckBox(libGUI GUI) : this(GUI, GUI.DefaultFont) {
		}

		public override void OnMouseClick(OnKeyEventArgs E) {
			Checked = !Checked;
			OnToggled?.Invoke(this, new OnCheckBoxToggledEventArgs(GUI, Checked));
		}

		public override void Draw() {
			Vector2 Pos = GlobalPosition;

			Gfx.TexturedRectangle(Pos.X, Pos.Y, CheckBoxSize.X, CheckBoxSize.Y, Texture: Checked ? GUI.CheckBoxCheckedSkin : GUI.CheckBoxSkin);

			if (IsMouseHovered)
				Gfx.TexturedRectangle(Pos.X, Pos.Y, CheckBoxSize.X, CheckBoxSize.Y, Texture: GUI.CheckBoxHoverOverlaySkin);

			Matrix4x4 Old = ShaderUniforms.Current.Model;
			ShaderUniforms.Current.Model = Old * Matrix4x4.CreateTranslation(new Vector3(CheckBoxSize.X + CheckBoxButtonSpacing, 0, 0));
			base.Draw();
			ShaderUniforms.Current.Model = Old;
		}
	}
}
