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
		public override float BorderLeft { get => Skin.BorderLeft * Skin.BorderLeftScale; set => Skin.BorderLeft = value; }
		public override float BorderRight { get => Skin.BorderRight * Skin.BorderRightScale; set => Skin.BorderRight = value; }
		public override float BorderTop { get => Skin.BorderTop * Skin.BorderTopScale; set => Skin.BorderTop = value; }
		public override float BorderBottom { get => Skin.BorderBottom * Skin.BorderBottomScale; set => Skin.BorderBottom = value; }

		bool _Resizable;
		public bool Resizable {
			get {
				if (CanSetSize)
					return _Resizable;

				return false;
			}

			set {
				_Resizable = value;
			}
		}

		public bool Closable;
		public event Action OnClose;

		NineSlice Skin;
		bool IsBeingDragged;
		int DragSlice;
		Vector2 DragStartPos;
		Vector2 DragStartSize;
		Vector2 DragStartMousePos;

		Label WindowLabel;
		Button WindowCloseButton;

		public string Title {
			get {
				return WindowLabel.Text;
			}
			set {
				WindowLabel.Text = value;
			}
		}

		public Window(libGUI GUI) : base(GUI) {
			WindowLabel = new Label(GUI, GUI.DefaultFont);
			WindowLabel.Color = Color.Black;
			WindowLabel.Text = "Window";
			WindowLabel.Parent = this;
			WindowLabel.PerformClipping = false;
			//WindowLabel.DebugPaintClientArea = true;
			//AddChild(WindowLabel);

			WindowCloseButton = new Button(GUI);
			WindowCloseButton.ButtonSkin = GUI.ButtonCloseSkin;
			WindowCloseButton.ButtonHoverSkin = GUI.ButtonCloseHoverSkin;
			WindowCloseButton.ButtonClickSkin = GUI.ButtonCloseClickSkin;
			WindowCloseButton.Size = new Vector2(20, 17);
			WindowCloseButton.Parent = this;
			WindowCloseButton.OnClick += (S, E) => Close();
			WindowCloseButton.Refresh();

			Closable = true;
			Resizable = true;
			Title = "Window";
			Skin = new NineSlice(GUI.WindowSkin, 27, 4, 4, 4);
		}

		public virtual void Close() {
			if (Parent != null)
				Parent.RemoveChild(this);
			else
				GUI.RemoveControl(this);

			OnClose?.Invoke();
		}

		public override void Draw() {
			Skin.Position = GlobalPosition;
			Skin.Size = Size;
			Skin.Draw();

			RenderState RS = Gfx.PeekRenderState();
			RS.ScissorRegion = RS.ScissorRegion.Intersection(new AABB(GlobalPosition + new Vector2(BorderLeft, Size.Y - BorderTop), new Vector2(Size.X - BorderLeft - BorderRight, BorderTop)));
			Gfx.PushRenderState(RS);
			{
				float BtmTxtOffset = (BorderTop - WindowLabel.Size.Y) / 2.0f;
				WindowLabel.Position = new Vector2(BorderLeft * 2, Size.Y - BorderTop + BtmTxtOffset);
				WindowLabel.Draw();
			}
			Gfx.PopRenderState();

			if (Closable) {
				WindowCloseButton.Position = Size - WindowCloseButton.Size * new Vector2(1.5f, 1);
				WindowCloseButton.Draw();
			}
			base.Draw();
		}

		public override bool OnMouseMove(OnMouseMoveEventArgs E) {
			if (IsBeingDragged) {
				Vector2 D = E.Pos - DragStartMousePos;

				if (DragSlice == 1 || DragSlice == 2 || DragSlice == 3)
					Position = DragStartPos + D;
				else if (DragSlice == 6 && Resizable) {
					Vector2 NewSize = new Vector2(DragStartSize.X + D.X, DragStartSize.Y);

					if (NewSize.X >= MinSize.X && NewSize.Y >= MinSize.Y)
						Size = NewSize;
				} else if (DragSlice == 8 && Resizable) {
					Vector2 NewSize = new Vector2(DragStartSize.X, DragStartSize.Y - D.Y);
					Vector2 NewPosition = DragStartPos + new Vector2(0, D.Y);

					if (NewSize.X >= MinSize.X && NewSize.Y >= MinSize.Y) {
						Size = NewSize;
						Position = NewPosition;
					}
				} else if (DragSlice == 9 && Resizable) {
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

			if (Closable)
				WindowCloseButton.OnMouseMove(E);
			
			base.OnMouseMove(E);

			if (!E.Consumed && IsInside(E.Pos)) {
				E.Consumed = true;
				return true;
			}

			return false;
		}

		public override bool OnKey(OnKeyEventArgs E) {
			int Slice = Skin.Collides(E.MousePos - GlobalPosition);

			AABB ClientArea = new AABB(GlobalClientArea, ClientAreaSize);
			if (Slice == 5 && ClientArea.IsInside(E.MousePos)) {
				base.OnKey(E);

				if (E.Consumed)
					return false;
			}

			if (Closable)
				WindowCloseButton.OnKey(E);

			E.Consumed = true;
			return true;
			//return false;
		}

		public override void OnBeginHold(OnKeyEventArgs E) {
			if (E.Key == Key.MouseLeft) {
				int Slice = Skin.Collides(E.MousePos - GlobalPosition);

				if (Slice != 0) {
					if (Closable && Slice == 2 && WindowCloseButton.IsInside(E.MousePos)) {
						// Do nothing if inside window close button
					} else {
						IsBeingDragged = true;
						DragSlice = Slice;
						DragStartPos = Position;
						DragStartSize = Size;
						DragStartMousePos = E.MousePos;
					}
				}
			}
		}

		public override void OnEndHold(OnKeyEventArgs E) {
			if (E.Key == Key.MouseLeft)
				IsBeingDragged = false;
		}
	}
}
