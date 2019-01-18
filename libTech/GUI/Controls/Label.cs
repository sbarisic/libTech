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
	public enum TextAlign {
		Left,
		Center,
		Right
	}

	public class Label : Control {
		bool Dirty;

		string _Text;
		public virtual string Text {
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

		public GfxFont Font;
		public Color Color = Color.Black;
		public virtual bool PerformClipping { get; set; }

		Vector2 _TextSize;
		public override Vector2 Size {
			get {
				Refresh();
				return _TextSize;
			}
			set => throw new InvalidOperationException();
		}

		public Label(libGUI GUI, GfxFont Font) : base(GUI) {
			this.Font = Font;
			PerformClipping = true;
		}

		public Label(libGUI GUI) : this(GUI, GUI.DefaultFont) {
		}

		public override void Refresh() {
			base.Refresh();

			if (!Dirty)
				return;
			Dirty = false;

			if (Text.Length == 0)
				_TextSize = Vector2.Zero;
			else
				_TextSize = Font.MeasureString(Text);
		}

		public override void Draw() {
			Refresh();

			if ((Text?.Length ?? 0) != 0) {
				if (PerformClipping) {
					RenderState RS = Gfx.PeekRenderState();
					RS.ScissorRegion = RS.ScissorRegion.Intersection(new AABB(GlobalClientArea, ClientAreaSize));
					Gfx.PushRenderState(RS);
				}

				Gfx.DrawText(Font, GlobalPosition, Text, Color);

				if (PerformClipping)
					Gfx.PopRenderState();
			}

			base.Draw();
		}
	}
}
