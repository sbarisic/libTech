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
	public delegate void OnMouseEnterAction(bool Entered);
	public delegate void OnMouseAction(Key K, Vector2 Pos);
	public delegate void OnMouseDragAction(Key K, Vector2 Pos);
	public delegate void OnCharAction(string Char, uint Unicode);
	public delegate void OnKeyAction(Key K, Vector2 Pos, bool Pressed);

	public class GUIControl {
		public event OnMouseEnterAction OnMouseEnter;
		public event OnMouseAction OnMouseClick;
		public event OnMouseDragAction OnMouseDrag;
		public event OnCharAction OnChar;
		public event OnKeyAction OnKey;

		protected bool UseScissor = true;
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
			if (UseScissor)
				Gfx.PushScissor(Scissor);

			foreach (var Ctrl in Children) {
				if (Scissor.Collide(Ctrl.Scissor))
					Ctrl.Draw();
			}

			if (UseScissor)
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
			if (Children.Contains(Child)) {
				BringToFront(Child);
			} else {
				Child.Parent = this;
				Children.Add(Child);
			}
		}

		public T AddChild<T>(T Child) where T : GUIControl {
			AddChild((GUIControl)Child);
			return Child;
		}

		public virtual void RemoveChild(GUIControl Child) {
			if (Children.Contains(Child)) {
				Children.Remove(Child);
				Child.Parent = null;
			}
		}

		public virtual void RemoveAllChildren() {
			GUIControl[] ChildrenArray = Children.ToArray();
			foreach (var Child in ChildrenArray)
				RemoveChild(Child);
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

		internal virtual void SendOnMouseDrag(Key K, Vector2 Pos) {
			OnMouseDrag?.Invoke(K, Pos);
		}

		internal virtual void SendOnMouseEnter(bool Entered) {
			OnMouseEnter?.Invoke(Entered);
		}

		internal virtual void SendOnKey(Key K, Vector2 Pos, bool Pressed) {
			OnKey?.Invoke(K, Pos, Pressed);
		}

		internal virtual void SendOnMouseClick(Key K, Vector2 Pos) {
			OnMouseClick?.Invoke(K, Pos);
		}

		internal virtual void SendOnChar(string Char, uint Unicode) {
			OnChar?.Invoke(Char, Unicode);
		}

		internal virtual void SendOnPaste(string String) {
		}

		internal virtual string SendOnCopy() {
			return null;
		}

		internal virtual string SendOnCut() {
			return null;
		}
	}

	public class libGUI : GUIControl {
		GUIControl CurrentlyHovered;
		GUIControl LeftClickStart;
		GUIControl SelectedControl;
		Vector2 MousePos;

		public libGUI() {
			Position = Vector2.Zero;
			Size = new Vector2(float.PositiveInfinity);
		}

		public void SendOnMouseMove(Vector2 Pos) {
			MousePos = Pos;

			if (LeftClickStart != null)
				LeftClickStart.SendOnMouseDrag(Key.MouseLeft, Pos);

			if (GetItemAt(Pos, out GUIControl Ctrl)) {
				if (CurrentlyHovered == Ctrl)
					return;

				CurrentlyHovered?.SendOnMouseEnter(false);
				Ctrl.SendOnMouseEnter(true);
				CurrentlyHovered = Ctrl;
				return;
			}

			CurrentlyHovered?.SendOnMouseEnter(false);
			CurrentlyHovered = null;
		}

		public void Select(GUIControl Ctrl) {
			if (Ctrl != null)
				BringToFront(Ctrl);
			SelectedControl = Ctrl;
		}

		public void SendOnKey(Key Key, int Scancode, bool Pressed, bool Repeat, KeyMods Mods) {
			//Console.WriteLine("{0} x {1} : {2} - {3}", MousePos.X, MousePos.Y, Key, Pressed);

			if (Key == Key.V && Mods == KeyMods.Control && Pressed) {
				SelectedControl?.SendOnPaste(Engine.Window.ClipboardString);
				return;
			}

			if (Key == Key.C && Mods == KeyMods.Control && Pressed) {
				string CopyString = SelectedControl?.SendOnCopy();
				if (CopyString != null)
					Engine.Window.ClipboardString = CopyString;

				return;
			}

			if (Key == Key.X && Mods == KeyMods.Control && Pressed) {
				string CopyString = SelectedControl?.SendOnCut();
				if (CopyString != null)
					Engine.Window.ClipboardString = CopyString;

				return;
			}

			if (Key == Key.MouseLeft) {
				if (Pressed) {
					LeftClickStart = GetItemAt(MousePos, out GUIControl Ctrl) ? Ctrl : null;
					Select(LeftClickStart);

					LeftClickStart?.SendOnKey(Key, MousePos, true);
				} else {
					if (GetItemAt(MousePos, out GUIControl Ctrl)) {
						LeftClickStart?.SendOnKey(Key, MousePos, false);

						if (Ctrl == LeftClickStart)
							Ctrl.SendOnMouseClick(Key, MousePos);
					}

					LeftClickStart = null;
				}

				return;
			}

			SelectedControl?.SendOnKey(Key, MousePos, Pressed);
		}

		internal override void SendOnChar(string Char, uint Unicode) {
			if (SelectedControl != null) {
				SelectedControl.SendOnChar(Char, Unicode);
				return;
			}

			base.SendOnChar(Char, Unicode);
		}
	}
}
