using FishGfx;
using FishGfx.Formats;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using libTech.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace libTech.GUI {
	public class libGUI {
		GfxFont Fnt;
		List<Control> Controls;
		Vector2 MousePos;

		public libGUI(RenderWindow RWind) {
			Fnt = new BMFont("content/fonts/proggy.fnt", 16);
			Fnt.Userdata = Texture.FromFile("content/textures/proggy_0.png");

			Controls = new List<Control>();

			Texture WindowSkin = Texture.FromFile("content/textures/gui_elements/window.png");

			RWind.OnMouseMove += (W, X, Y) => {
				MousePos = new Vector2(X, W.WindowHeight - Y);
				OnMouseMoveEventArgs Evt = new OnMouseMoveEventArgs(this, MousePos);

				foreach (var C in Controls) {
					C.OnMouseMove(Evt);

					if (Evt.Consumed)
						break;
				}
			};

			RWind.OnKey += (W, Key, Scancode, Pressed, Repeat, Mods) => {
				if (Key != Key.MouseLeft)
					return;

				OnKeyEventArgs Evt = new OnKeyEventArgs(this, MousePos, Pressed, Key);

				foreach (var C in Controls) {
					C.OnKey(Evt);

					if (Evt.Consumed)
						break;
				}
			};


			Window Wnd = new Window(WindowSkin);
			Wnd.DebugName = "Window A";
			Wnd.Position = new Vector2(50, 50);
			Wnd.Size = new Vector2(300, 300);
			Controls.Add(Wnd);

			Window Wnd2 = new Window(WindowSkin);
			Wnd2.DebugName = "Window B";
			Wnd2.Position = new Vector2(50, 50);
			Wnd2.Size = new Vector2(100, 100);
			Wnd.AddChild(Wnd2);
		}

		public void Update(float Dt) {
			foreach (var C in Controls)
				C.Update(Dt);
		}

		public void Draw() {
			float FT = Engine.FrameTime.Average();
			float FPS = 1.0f / FT;

			Gfx.DrawText(Fnt, (Texture)Fnt.Userdata, new Vector2(0, Engine.WindowHeight - Fnt.LineHeight), string.Format("{0} ms\n{1} FPS", FT, FPS), Color.White);

			foreach (var C in Controls)
				C.Draw();
		}
	}

	class Window : Control {
		NineSlice Skin;

		public override float BorderLeft { get => Skin.BorderLeft; set => Skin.BorderLeft = value; }
		public override float BorderRight { get => Skin.BorderRight; set => Skin.BorderRight = value; }
		public override float BorderTop { get => Skin.BorderTop; set => Skin.BorderTop = value; }
		public override float BorderBottom { get => Skin.BorderBottom; set => Skin.BorderBottom = value; }

		bool IsBeingDragged;
		int DragSlice;
		Vector2 DragStartPos;
		Vector2 DragStartSize;
		Vector2 DragStartMousePos;

		public Window(Texture Tex) {
			Skin = new NineSlice(Tex, 27, 4, 4, 4);
		}

		public override void Draw() {
			Skin.Position = GlobalPosition;
			Skin.Size = Size;
			Skin.Draw();

			base.Draw();
		}

		public override bool OnMouseMove(OnMouseMoveEventArgs E) {
			base.OnMouseMove(E);

			if (!E.Consumed) {
				if (IsBeingDragged) {
					Vector2 D = E.Pos - DragStartMousePos;

					if (DragSlice == 1 || DragSlice == 2 || DragSlice == 3)
						Position = DragStartPos + D;
					else if (DragSlice == 6)
						Size = new Vector2(DragStartSize.X + D.X, DragStartSize.Y);
					else if (DragSlice == 8) {
						Size = new Vector2(DragStartSize.X, DragStartSize.Y - D.Y);
						Position = DragStartPos + new Vector2(0, D.Y);
					} else if (DragSlice == 9) {
						Size = new Vector2(DragStartSize.X + D.X, DragStartSize.Y - D.Y);
						Position = DragStartPos + new Vector2(0, D.Y);
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

	abstract class Control {
		public bool DebugPaintClientArea;
		public string DebugName;

		public Control Parent;
		public List<Control> Children = new List<Control>();

		public virtual Vector2 Position { get; set; }
		public virtual Vector2 Size { get; set; }

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

	public class libGUIEventArgs : EventArgs {
		public libGUI GUI;
		public bool Consumed;

		public libGUIEventArgs(libGUI GUI) {
			this.GUI = GUI;
			Consumed = false;
		}
	}

	public class OnMouseMoveEventArgs : libGUIEventArgs {
		public Vector2 Pos;

		public OnMouseMoveEventArgs(libGUI GUI, Vector2 Pos) : base(GUI) {
			this.Pos = Pos;
		}
	}

	public class OnKeyEventArgs : libGUIEventArgs {
		public Vector2 MousePos;
		public bool Pressed;
		public Key Key;

		public OnKeyEventArgs(libGUI GUI, Vector2 MousePos, bool Pressed, Key Key) : base(GUI) {
			this.MousePos = MousePos;
			this.Pressed = Pressed;
			this.Key = Key;
		}
	}
}
