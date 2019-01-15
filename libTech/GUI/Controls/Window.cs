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
	public class Window : Control {
		NineSlice Skin;

		public override float BorderLeft { get => Skin.BorderLeft * Skin.BorderLeftScale; set => Skin.BorderLeft = value; }
		public override float BorderRight { get => Skin.BorderRightScale * Skin.BorderRightScale; set => Skin.BorderRightScale = value; }
		public override float BorderTop { get => Skin.BorderTop * Skin.BorderTopScale; set => Skin.BorderTop = value; }
		public override float BorderBottom { get => Skin.BorderBottom * Skin.BorderBottomScale; set => Skin.BorderBottom = value; }

		bool IsBeingDragged;
		int DragSlice;
		Vector2 DragStartPos;
		Vector2 DragStartSize;
		Vector2 DragStartMousePos;

		public string Title;

		public Window(Texture SkinTexture) {
			Title = "Window";
			Skin = new NineSlice(SkinTexture, 27, 4, 4, 4);
		}

		public override void Draw() {
			Skin.Position = GlobalPosition;
			Skin.Size = Size;
			Skin.Draw();

			/*float BtmTxtOffset = (BorderTop - Font.MeasureString(Title).Y) / 3.0f;
			Gfx.DrawText(Font, GlobalPosition + new Vector2(BorderLeft * 2, Size.Y - BorderTop + BtmTxtOffset), Title, Color.Black);*/

			base.Draw();
		}

		public override bool OnMouseMove(OnMouseMoveEventArgs E) {
			base.OnMouseMove(E);

			if (!E.Consumed) {
				if (IsBeingDragged) {
					Vector2 D = E.Pos - DragStartMousePos;

					if (DragSlice == 1 || DragSlice == 2 || DragSlice == 3)
						Position = DragStartPos + D;
					else if (DragSlice == 6) {
						Vector2 NewSize = new Vector2(DragStartSize.X + D.X, DragStartSize.Y);

						if (NewSize.X >= MinSize.X && NewSize.Y >= MinSize.Y)
							Size = NewSize;
					} else if (DragSlice == 8) {
						Vector2 NewSize = new Vector2(DragStartSize.X, DragStartSize.Y - D.Y);
						Vector2 NewPosition = DragStartPos + new Vector2(0, D.Y);

						if (NewSize.X >= MinSize.X && NewSize.Y >= MinSize.Y) {
							Size = NewSize;
							Position = NewPosition;
						}
					} else if (DragSlice == 9) {
						Vector2 NewSize = new Vector2(DragStartSize.X + D.X, DragStartSize.Y - D.Y);
						Vector2 NewPosition = DragStartPos + new Vector2(0, D.Y);

						if (NewSize.X >= MinSize.X) {
							Size = new Vector2(NewSize.X, Size.Y);
							Position = new Vector2(NewPosition.X, Position.Y);
						}

						if (NewSize.Y >= MinSize.Y) {
							Size = new Vector2(Size.X, NewSize.Y);
							Position = new Vector2(Position.X, NewPosition.Y);
						}
					}

					E.Consumed = true;
					return true;
				}
			}

			return false;
		}

		public override bool OnKey(OnKeyEventArgs E) {
			base.OnKey(E);

			if (!E.Consumed && E.Key == Key.MouseLeft) {
				if (IsBeingDragged && !E.Pressed) {
					IsBeingDragged = false;
					E.Consumed = true;
					return true;
				}

				int Slice = Skin.Collides(E.MousePos - GlobalPosition);
				if (Slice != 0) {
					/*if (E.Pressed)
						Console.WriteLine("{0} - {1}", DebugName, Slice);*/

					if (!IsBeingDragged && E.Pressed) {
						IsBeingDragged = true;
						DragSlice = Slice;
						DragStartPos = Position;
						DragStartSize = Size;
						DragStartMousePos = E.MousePos;
					}
				}

				if (Slice != 0) {
					E.Consumed = true;
					return true;
				}
			}

			return false;
		}
	}
}
