using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using FishGfx;
using FishGfx.Graphics;
using libTech.Graphics;

namespace libTech.GUI {
	public delegate void OnMouseAction(Key K, Vector2 Pos);
	public delegate void OnMouseDrag(Key K, Vector2 Pos);

	public class GUIControl {
		public event OnMouseAction OnClick;
		public event OnMouseAction OnPress;
		public event OnMouseAction OnRelease;
		public event OnMouseDrag OnDrag;

		protected List<GUIControl> Children;

		public GUIControl Parent;

		public virtual Vector2 Position {
			get; set;
		}

		public virtual Vector2 Size {
			get; set;
		}

		public virtual AABB Scissor {
			get {
				return new AABB(GlobalPosition, Size);
			}
		}

		public virtual Vector2 GlobalPosition {
			get {
				if (Parent != null)
					return Parent.GlobalPosition + Position;

				return Position;
			}
		}

		public GUIControl() {
			Children = new List<GUIControl>();
		}

		public virtual void AutoResize(Vector2 Padding) {
			Vector2 MaxSize = new Vector2(float.MinValue);

			foreach (var Child in Children)
				MaxSize = MaxSize.Max(Child.Position + Child.Size);

			Size = MaxSize + Padding;
		}

		public virtual void Center(Vector2 Pos) {
			//Vector2 Pos = Position - GlobalPosition;

			Vector2 HalfSize = Size / 2;
			Position = Pos - HalfSize;
		}

		public virtual void Draw() {
			Gfx.PushScissor(Scissor);

			foreach (var Ctrl in Children) {
				if (Scissor.Collide(Ctrl.Scissor))
					Ctrl.Draw();
			}

			Gfx.PopScissor();
		}

		public virtual bool BringToFront(GUIControl Ctrl, bool Recursive = true) {
			if (Children.Contains(Ctrl)) {
				Children.Remove(Ctrl);
				Children.Add(Ctrl);
				return true;
			} else if (Recursive) {
				GUIControl C = null;

				foreach (var Child in Children)
					if (Child.BringToFront(Ctrl)) {
						C = Child;
						break;
					}

				if (C != null) {
					Children.Remove(C);
					Children.Add(C);
					return true;
				}
			}

			return false;
		}

		public virtual void AddChild(GUIControl Child) {
			Child.Parent = this;
			Children.Add(Child);
		}

		public T AddChild<T>(T Child) where T : GUIControl {
			AddChild((GUIControl)Child);
			return Child;
		}

		public virtual void RemoveChild(GUIControl Child) {
			Children.Remove(Child);
			Child.Parent = null;
		}

		public virtual bool IsInside(Vector2 Pos) {
			return new AABB(GlobalPosition, Size).IsInside(Pos);
		}

		public virtual bool GetItemAt(Vector2 Pos, out GUIControl Ctrl) {
			if (IsInside(Pos)) {
				for (int i = Children.Count - 1; i >= 0; i--)
					if (Children[i].IsInside(Pos))
						return Children[i].GetItemAt(Pos, out Ctrl);

				Ctrl = this;
				return true;
			}

			Ctrl = null;
			return false;
		}

		internal virtual void OnMouseDrag(Key K, Vector2 Pos) {
			OnDrag?.Invoke(K, Pos);
		}

		internal virtual void OnMouseEnter(bool Entered) {
		}

		internal virtual void OnMousePress(Key K, Vector2 Pos) {
			OnPress?.Invoke(K, Pos);
		}

		internal virtual void OnMouseRelease(Key K, Vector2 Pos) {
			OnRelease?.Invoke(K, Pos);
		}

		internal virtual void OnMouseClick(Key K, Vector2 Pos) {
			OnClick?.Invoke(K, Pos);
		}
	}

	public class libGUI : GUIControl {
		GUIControl CurrentlyHovered;

		GUIControl LeftClickStart;

		Vector2 MousePos;

		public libGUI() {
			Position = Vector2.Zero;
			Size = new Vector2(float.PositiveInfinity);
		}

		public void OnMouseMove(Vector2 Pos) {
			MousePos = Pos;

			if (LeftClickStart != null)
				LeftClickStart.OnMouseDrag(Key.MouseLeft, Pos);

			if (GetItemAt(Pos, out GUIControl Ctrl)) {
				if (CurrentlyHovered == Ctrl)
					return;

				CurrentlyHovered?.OnMouseEnter(false);
				Ctrl.OnMouseEnter(true);
				CurrentlyHovered = Ctrl;
				return;
			}

			CurrentlyHovered?.OnMouseEnter(false);
			CurrentlyHovered = null;
		}

		public void OnKey(Key Key, int Scancode, bool Pressed, bool Repeat, KeyMods Mods) {
			//Console.WriteLine("{0} x {1} : {2} - {3}", MousePos.X, MousePos.Y, Key, Pressed);

			if (Key == Key.MouseLeft) {
				if (Pressed) {
					LeftClickStart = GetItemAt(MousePos, out GUIControl Ctrl) ? Ctrl : null;

					if (LeftClickStart != null)
						BringToFront(LeftClickStart);

					LeftClickStart?.OnMousePress(Key, MousePos);
				} else {
					if (GetItemAt(MousePos, out GUIControl Ctrl)) {
						LeftClickStart?.OnMouseRelease(Key, MousePos);

						if (Ctrl == LeftClickStart)
							Ctrl.OnMouseClick(Key, MousePos);
					}

					LeftClickStart = null;
				}
			}
		}
	}
}
