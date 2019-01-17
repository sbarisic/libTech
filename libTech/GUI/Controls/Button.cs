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
	public class Button : Control {
		public event OnButtonClickFunc OnClick;

		NineSlice Skin;
		Label ButtonLabel;

		public Texture ButtonHoverSkin;
		public Texture ButtonSkin;
		public Texture ButtonClickSkin;
		public Texture ButtonDisabledSkin;

		public override float BorderLeft { get => Skin.BorderLeft * Skin.BorderLeftScale; set => Skin.BorderLeft = value; }
		public override float BorderRight { get => Skin.BorderRight * Skin.BorderRightScale; set => Skin.BorderRight = value; }
		public override float BorderTop { get => Skin.BorderTop * Skin.BorderTopScale; set => Skin.BorderTop = value; }
		public override float BorderBottom { get => Skin.BorderBottom * Skin.BorderBottomScale; set => Skin.BorderBottom = value; }

		public string Text {
			get {
				return ButtonLabel.Text;
			}
			set {
				ButtonLabel.Text = value;
			}
		}

		public Button(libGUI GUI) : base(GUI) {
			this.ButtonHoverSkin = GUI.ButtonHoverSkin;
			this.ButtonSkin = GUI.ButtonSkin;
			this.ButtonClickSkin = GUI.ButtonClickSkin;
			this.ButtonDisabledSkin = GUI.ButtonDisabledSkin;

			ButtonLabel = new Label(GUI, GUI.DefaultFont);
			ButtonLabel.Color = Color.Black;
			ButtonLabel.Parent = this;
			ButtonLabel.PerformClipping = false;
			Text = "";

			Skin = new NineSlice(this.ButtonSkin, 2);
		}

		public override void Refresh() {
			base.Refresh();

			Skin.Texture = ButtonSkin;
		}

		public override void OnBeginHover() {
			Skin.Texture = ButtonHoverSkin;
		}

		public override void OnEndHover() {
			Skin.Texture = ButtonSkin;
		}

		public override bool OnKey(OnKeyEventArgs E) {
			base.OnKey(E);

			if (!E.Consumed && IsMouseHovered) {
				if (E.Key == Key.MouseLeft) {
					if (E.Pressed)
						Skin.Texture = ButtonClickSkin;
					else
						Skin.Texture = ButtonHoverSkin;

					E.Consumed = true;
					return true;
				}
			}

			return false;
		}

		public override void OnMouseClick(OnKeyEventArgs E) {
			OnClick?.Invoke(this, new OnButtonClickEventArgs(E.GUI));
		}

		public override void Draw() {
			Skin.Position = GlobalPosition;
			Skin.Size = Size;
			Skin.Draw();

			RenderState RS = Gfx.PeekRenderState();
			RS.ScissorRegion = RS.ScissorRegion.Intersection(new AABB(GlobalClientArea, ClientAreaSize));
			Gfx.PushRenderState(RS);

			ButtonLabel.Position = (ClientAreaSize / 2) - (ButtonLabel.Size / 2);
			ButtonLabel.Draw();

			Gfx.PopRenderState();

			base.Draw();
		}
	}
}
