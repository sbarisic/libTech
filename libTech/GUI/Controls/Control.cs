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
					ParentPos = Parent.Position;

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

		public virtual void Update(float Dt) {
			foreach (var C in Children)
				C.Update(Dt);
		}

		public virtual void Draw() {
			if (ClientAreaSize.X <= 0 || ClientAreaSize.Y <= 0)
				return;

			if (DebugPaintClientArea)
				Gfx.FilledRectangle(GlobalClientArea.X, GlobalClientArea.Y, ClientAreaSize.X, ClientAreaSize.Y, Color.Red);

			Gfx.PushScissor(new AABB(GlobalClientArea, ClientAreaSize));

			foreach (var C in Children)
				C.Draw();

			Gfx.PopScissor();
		}

		public virtual void AddChild(Control Ctrl) {
			Ctrl.Parent = this;
			Children.Add(Ctrl);
		}

		public virtual bool IsInside(Vector2 Pos) {
			AABB Ctrl = new AABB(GlobalPosition, Size);
			return Ctrl.IsInside(Pos);
		}
		
		/// <summary>
		/// Called when mouse is moved. Returns true if this control consumed the event, otherwise false.
		/// </summary>
		/// <param name="E"></param>
		/// <returns></returns>
		public virtual bool OnMouseMove(OnMouseMoveEventArgs E) {
			if (E.Consumed)
				return false;

			foreach (var C in Children) {
				C.OnMouseMove(E);

				if (E.Consumed)
					return false;
			}

			return false;
		}

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

			return false;
		}
	}

}
