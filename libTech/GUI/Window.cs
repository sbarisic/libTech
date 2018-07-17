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
	public class Window : GUIControl {
		readonly Vector2 Padding = new Vector2(10);

		NineSlice Panel;

		Vector2 ClickStart;
		Vector2 DragStartPos;
		Vector2 DragStartSize;

		int DragID;

		public bool Resizable;
		public bool Movable;

		public override AABB Scissor {
			get {
				return new AABB(GlobalPosition - Padding, Size + Padding * 2);
			}
		}

		public Window(Vector2 Pos, Vector2 Size) {
			Resizable = true;
			Movable = true;
			Position = Pos;
			this.Size = Size;

			Panel = new NineSlice(DefaultTextures.Panel, Padding.X);
			OnDrag += Window_OnDrag;

			OnPress += (K, P) => {
				if (K == Key.MouseLeft) {
					ClickStart = P;
					DragStartPos = this.Position;
					DragStartSize = this.Size;

					DragID = Panel.Collides(P - Position);
				}
			};

			OnRelease += (K, P) => {
				if (K == Key.MouseLeft)
					DragID = 0;
			};
		}

		private void Window_OnDrag(Key K, Vector2 Pos) {
			if ((DragID == 1 || DragID == 2 || DragID == 3) && Movable) {
				Position = DragStartPos + (Pos - ClickStart);
			}

			if ((DragID == 6 || DragID == 8 || DragID == 9) && Resizable) {
				Vector2 MousePos;
				Vector2 MouseClickStart;

				if (DragID == 6) { // Horizontal resize
					MousePos = Pos.GetWidth();
					MouseClickStart = ClickStart.GetWidth();
				} else if (DragID == 8) { // Vertical resize
					MousePos = Pos.GetHeight();
					MouseClickStart = ClickStart.GetHeight();
				} else { // Corner
					MousePos = Pos;
					MouseClickStart = ClickStart;
				}

				Vector2 NewSize = DragStartSize + ((MousePos - MouseClickStart) * new Vector2(1, -1));

				const float MinSize = 5;
				if (NewSize.X >= MinSize && NewSize.Y >= MinSize) {
					Size = NewSize;
					Position = DragStartPos + DragStartSize.GetHeight() - NewSize.GetHeight();
				}
			}
		}

		public override bool IsInside(Vector2 Pos) {
			return Panel.Collides(Pos - GlobalPosition) != 0;
		}

		public override void Draw() {
			Panel.Position = new Vector3(GlobalPosition, 0);
			Panel.Size = Size;
			Panel.Draw();

			base.Draw();
		}

		public virtual void Close() {
			if (Parent != null) 
				Parent.RemoveChild(this);
		}
	}
}
