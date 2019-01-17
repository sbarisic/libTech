using FishGfx;
using FishGfx.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.GUI.Controls {
	public abstract class Control {
		public readonly libGUI GUI;

		public bool DebugPaintClientArea;
		public string DebugName;

		public Control Parent;
		public List<Control> Children = new List<Control>();

		public virtual Vector2 Position { get; set; }
		public virtual Vector2 Size { get; set; }
		public virtual Vector2 MinSize { get { return new Vector2(70, 40); } }

		public virtual Vector2 GlobalPosition {
			get {
				Vector2 ParentPos = Vector2.Zero;

				if (Parent != null)
					ParentPos = Parent.GlobalPosition;

				return ParentPos + Position;
			}
		}

		public virtual Vector2 ClientArea {
			get {
				return new Vector2(BorderLeft, BorderBottom);
			}
		}

		public virtual Vector2 ClientAreaSize {
			get {
				return Size - new Vector2(BorderLeft + BorderRight, BorderTop + BorderBottom);
			}
		}

		public virtual Vector2 GlobalClientArea {
			get {
				return GlobalPosition + ClientArea;
			}
		}

		public virtual float BorderLeft { get; set; }
		public virtual float BorderRight { get; set; }
		public virtual float BorderTop { get; set; }
		public virtual float BorderBottom { get; set; }

		public virtual bool IsMouseHovered { get; set; }

		public Control(libGUI GUI) {
			this.GUI = GUI;
		}

		public virtual void Update(float Dt) {
			foreach (var C in Children)
				C.Update(Dt);
		}

		public virtual void Draw() {
			if (ClientAreaSize.X <= 0 || ClientAreaSize.Y <= 0)
				return;

			if (DebugPaintClientArea)
				Gfx.FilledRectangle(GlobalClientArea.X, GlobalClientArea.Y, ClientAreaSize.X, ClientAreaSize.Y, Color.Red);

			RenderState RS = Gfx.PeekRenderState();
			RS.ScissorRegion = RS.ScissorRegion.Intersection(new AABB(GlobalClientArea, ClientAreaSize));
			Gfx.PushRenderState(RS);

			foreach (var C in Children)
				C.Draw();

			Gfx.PopRenderState();
		}

		public virtual void AddChild(Control Ctrl) {
			Ctrl.Parent = this;
			Children.Add(Ctrl);
		}

		public virtual void RemoveChild(Control Ctrl) {
			Children.Remove(Ctrl);
			Ctrl.Parent = null;
		}

		public virtual bool IsInside(Vector2 Pos) {
			if (Parent != null) {
				if (!Parent.IsInside(Pos))
					return false;
			}

			AABB Ctrl = new AABB(GlobalPosition, Size);
			return Ctrl.IsInside(Pos);
		}

		public virtual bool IsInClientArea(Vector2 Pos) {
			if (IsInside(Pos)) {
				AABB CArea = new AABB(GlobalClientArea, ClientAreaSize);
				return CArea.IsInside(Pos);
			}

			return false;
		}

		public virtual void Refresh() {
		}

		/// <summary>
		/// Gets a child control inside of the client area at the global position Pos
		/// </summary>
		/// <param name="Pos"></param>
		/// <returns></returns>
		public virtual Control GetRecursiveClientAreaChildAt(Vector2 Pos) {
			AABB CArea = new AABB(GlobalClientArea, ClientAreaSize);

			if (CArea.IsInside(Pos)) {
				foreach (var C in Children) {
					if (C.IsInside(Pos)) {
						Control ChildAtPos = C.GetRecursiveClientAreaChildAt(Pos);

						if (ChildAtPos != null)
							return ChildAtPos;

						return C;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Gets a child control inside of bounds of current control at the global position Pos
		/// </summary>
		/// <param name="Pos"></param>
		/// <returns></returns>
		public virtual Control GetRecursiveChildAt(Vector2 Pos) {
			if (IsInside(Pos)) {
				foreach (var C in Children) {
					if (C.IsInside(Pos)) {
						Control ChildAtPos = C.GetRecursiveChildAt(Pos);

						if (ChildAtPos != null)
							return ChildAtPos;

						return C;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Called when mouse is moved. Returns true if this control consumed the event, otherwise false.
		/// </summary>
		/// <param name="E"></param>
		/// <returns></returns>
		public virtual bool OnMouseMove(OnMouseMoveEventArgs E) {
			if (E.Consumed)
				return false;

			if (IsInside(E.Pos) && !IsMouseHovered) {
				IsMouseHovered = true;
				OnBeginHover();
			} else if (!IsInside(E.Pos) && IsMouseHovered) {
				IsMouseHovered = false;
				OnEndHover();
			}

			foreach (var C in Children) {
				C.OnMouseMove(E);

				if (E.Consumed)
					return false;
			}

			return false;
		}

		bool WasMousePressedDown = false;

		/// <summary>
		/// Called when a key is pressed, including mouse buttons. Returns true if this control consumed the event, otherwise false.
		/// </summary>
		/// <param name="E"></param>
		/// <returns></returns>
		public virtual bool OnKey(OnKeyEventArgs E) {
			if (E.Consumed)
				return false;

			foreach (var C in Children) {
				C.OnKey(E);

				if (E.Consumed)
					return false;
			}

			if (E.Key == Key.MouseLeft) {
				if (IsInside(E.MousePos)) {
					if (E.Pressed)
						WasMousePressedDown = true;
					else if (WasMousePressedDown) {
						WasMousePressedDown = false;
						OnMouseClick(E);
					}
				} else
					WasMousePressedDown = false;
			}

			return false;
		}

		/// <summary>
		/// Called when mouse is pressed and then released inside of the control. Pressed is always false here. 
		/// </summary>
		/// <param name="E"></param>
		public virtual void OnMouseClick(OnKeyEventArgs E) {
		}

		/// <summary>
		/// Called when a mouse key is being pressed on the control
		/// </summary>
		/// <param name="E"></param>
		public virtual void OnBeginHold(OnKeyEventArgs E) {
		}

		/// <summary>
		/// Called when a pressed key is being released from the control.
		/// Mouse does not have to be inside of the control for this event to occur
		/// </summary>
		/// <param name="E"></param>
		public virtual void OnEndHold(OnKeyEventArgs E) {
		}

		public virtual void OnBeginHover() {
		}

		public virtual void OnEndHover() {
		}
	}

}
