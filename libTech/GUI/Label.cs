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
		protected Vector2 BorderSize = new Vector2(2);
		protected Text Text;

		public bool Multiline;
		public AABB DrawRegion;

		NineSlice Borders;

		public override Vector2 Size {
			get => Text.StringSize;
			set => throw new InvalidOperationException();
		}

		public virtual string String {
			get {
				return Text.String;
			}
			set {
				string Str = value.Replace("\r", "");

				if (!Multiline)
					Str = Str.Replace("\n", "");

				Text.String = Str;
				Text.Refresh();
			}
		}

		public Color Color {
			get {
				return Text.Color;
			}
			set {
				Text.Color = value;
			}
		}

		public Label(FreetypeFont Font, string Txt = "") {
			this.Text = new Text(Font, Txt);
			String = Txt;
			Multiline = false;
			DrawRegion = AABB.Empty;
		}

		public virtual void Clear() {
			String = "";
			Text.ClearColors();
		}

		public virtual void AppendString(string Str, Color Clr) {
			Text.SetColor(Clr);
			Text.String += Str;
		}

		public virtual void AppendString(string Str) {
			Text.String += Str;
		}

		public override bool IsInside(Vector2 Pos) {
			if (!DrawRegion.IsEmpty) {
				return (DrawRegion + GlobalPosition).IsInside(Pos);
			}

			return base.IsInside(Pos);
		}

		public override void Draw() {
			if (!DrawRegion.IsEmpty) {
				if (Borders == null)
					Borders = new NineSlice(DefaultTextures.PanelTransparent, BorderSize.X);

				Borders.Position = GlobalPosition + DrawRegion.Position.XY() - BorderSize;
				Borders.Size = DrawRegion.Size.XY() + BorderSize * 2;
				Borders.Draw();

				Gfx.PushScissor(new AABB(DrawRegion.Position.XY() + GlobalPosition - BorderSize, DrawRegion.Size.XY() + BorderSize * 2));
				Text.Position = GlobalPosition + new Vector2(0, BorderSize.Y);
			} else
				Text.Position = GlobalPosition;

			Text.Draw();

			if (!DrawRegion.IsEmpty)
				Gfx.PopScissor();
		}
	}
}
