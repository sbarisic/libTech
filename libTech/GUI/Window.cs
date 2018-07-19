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
	public delegate void OnResizeAction(Window Wnd, Vector2 Size);

	public class Window : GUIControl {
		readonly Vector2 Padding = new Vector2(10);

		NineSlice Panel;

		Vector2 ClickStart;
		Vector2 DragStartPos;
		Vector2 DragStartSize;

		int DragID;

		public bool ResizableHorizontal;
		public bool ResizableVertical;
		public bool Resizable;
		public bool Movable;
		public Vector2 MinimumSize;
		public Color Color {
			get => Panel.Color;
			set => Panel.Color = value;
		}

		public event OnResizeAction OnResize;

		public override AABB Scissor {
			get {
				return new AABB(GlobalPosition - Padding, Size + Padding * 2);
			}
		}

		public override Vector2 Size {
			get {
				return base.Size;
			}
			set {
				base.Size = value;
				OnResize?.Invoke(this, base.Size);
			}
		}

		public Window(Vector2 Pos, Vector2 Size) {
			ResizableHorizontal = ResizableVertical = Resizable = Movable = true;
			Position = Pos;
			MinimumSize = new Vector2(5, 5);
			this.Size = Size;

			Panel = new NineSlice(DefaultTextures.Panel, Padding.X);
			OnMouseDrag += Window_OnDrag;

			OnKey += (K, P, Down) => {
				if (K == Key.MouseLeft) {
					if (Down) {
						ClickStart = P;
						DragStartPos = this.Position;
						DragStartSize = this.Size;

						DragID = Panel.Collides(P - Position);
					} else
						DragID = 0;
				}
			};
		}

		public Window() : this(Vector2.Zero, Vector2.Zero) {
		}

		private void Window_OnDrag(Key K, Vector2 Pos) {
			if ((DragID == 1 || DragID == 2 || DragID == 3) && Movable) {
				Position = DragStartPos + (Pos - ClickStart);
			} else if ((DragID == 6 || DragID == 8 || DragID == 9) && Resizable) {
				Vector2 MousePos;
				Vector2 MouseClickStart;

				if (DragID == 6 && ResizableHorizontal) { // Horizontal resize
					MousePos = Pos.GetWidth();
					MouseClickStart = ClickStart.GetWidth();
				} else if (DragID == 8 && ResizableVertical) { // Vertical resize
					MousePos = Pos.GetHeight();
					MouseClickStart = ClickStart.GetHeight();
				} else if (ResizableHorizontal && ResizableVertical) { // Corner
					MousePos = Pos;
					MouseClickStart = ClickStart;
				} else
					return;

				Vector2 NewSize = (DragStartSize + ((MousePos - MouseClickStart) * new Vector2(1, -1))).Max(MinimumSize);

				if (NewSize != Size) {
					Size = NewSize;
					Position = DragStartPos + DragStartSize.GetHeight() - NewSize.GetHeight();
				}
			}
		}

		public override bool IsInside(Vector2 Pos) {
			return Panel.Collides(Pos - GlobalPosition) != 0;
		}

		public override void Draw() {
			Panel.Position = GlobalPosition;
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
