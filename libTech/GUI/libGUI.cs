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

		/*
		public virtual Vector2 Min {
			get;
			protected set;
		}

		public virtual Vector2 Max {
			get;
			protected set;
		}
		*/

		public GUIControl Parent;

		public virtual Vector2 Position { get; set; }
		public virtual Vector2 Size { get; set; }
		public virtual AABB Scissor {
			get {
				return new AABB(GlobalPosition, Size);
			}
		}

		public virtual Vector2 GlobalPosition {
			get {
				if (Parent != null)
					return Parent.Position + Position;

				return Position;
			}
		}

		public GUIControl() {
			Children = new List<GUIControl>();
		}

		public virtual void Render() {
			Gfx.PushScissor(Scissor);

			foreach (var Ctrl in Children)
				Ctrl.Render();

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

	public class Window : GUIControl {
		readonly Vector2 Padding = new Vector2(10);

		NineSlice Panel;

		Vector2 ClickStart;
		Vector2 DragStartPos;
		Vector2 DragStartSize;

		int DragID;

		public override AABB Scissor {
			get {
				return new AABB(GlobalPosition - Padding, Size + Padding * 2);
			}
		}

		public Window(Vector2 Pos, Vector2 Size) {
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
			if (DragID == 1 || DragID == 2 || DragID == 3) {
				Position = DragStartPos + (Pos - ClickStart);
			}

			if (DragID == 9) {
				Vector2 NewSize = DragStartSize + ((Pos - ClickStart) * new Vector2(1, -1));

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

		public override void Render() {
			Panel.Position = new Vector3(GlobalPosition, 0);
			Panel.Size = Size;
			Panel.Draw();

			base.Render();
		}
	}

	public class TextButton : GUIControl {
		readonly Vector2 Padding = new Vector2(8);

		Text Text;
		NineSlice ButtonSkin;

		public string String {
			get {
				return Text.String;
			}
			set {
				Text.String = value;
				Text.Refresh();

				Size = this.Text.StringSize + Padding * 2;
			}
		}

		public TextButton(FreetypeFont Font, string Text) {
			this.Text = new Text(Font, Text);
			String = Text;

			ButtonSkin = new NineSlice(DefaultTextures.Button, 5);
		}

		public override bool IsInside(Vector2 Pos) {
			return base.IsInside(Pos + Padding);
		}

		public override bool GetItemAt(Vector2 Pos, out GUIControl Ctrl) {
			if (IsInside(Pos)) {
				Ctrl = this;
				return true;
			}

			Ctrl = null;
			return false;
		}

		internal override void OnMousePress(Key K, Vector2 Pos) {
			ButtonSkin.Texture = DefaultTextures.ButtonClick;

			base.OnMousePress(K, Pos);
		}

		internal override void OnMouseRelease(Key K, Vector2 Pos) {
			if (IsInside(Pos))
				ButtonSkin.Texture = DefaultTextures.ButtonHover;
			else
				ButtonSkin.Texture = DefaultTextures.Button;

			base.OnMouseRelease(K, Pos);
		}

		internal override void OnMouseEnter(bool Entered) {
			if (Entered)
				ButtonSkin.Texture = DefaultTextures.ButtonHover;
			else
				ButtonSkin.Texture = DefaultTextures.Button;

			base.OnMouseEnter(Entered);
		}

		public override void Render() {
			ButtonSkin.Position = new Vector3(GlobalPosition - Padding, 0);
			ButtonSkin.Size = Size;
			ButtonSkin.Draw();

			Text.Position = new Vector3(GlobalPosition, 0);
			Text.Draw();

			base.Render();
		}
	}
}
