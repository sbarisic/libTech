using FishGfx;
using FishGfx.Formats;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using libTech.Graphics;
using libTech.GUI.Controls;
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
		Dictionary<Key, Control> MouseHeldControls;
		List<Control> Controls;

		Vector2 MousePos;

		BMFont AnonPro16;
		BMFont AnonPro32;

		public GfxFont DefaultFont;
		public GfxFont DebugFont;

		public Texture WindowSkin;

		public Texture ButtonSkin;
		public Texture ButtonHoverSkin;
		public Texture ButtonDisabledSkin;
		public Texture ButtonClickSkin;

		public Texture ButtonCloseSkin;
		public Texture ButtonCloseHoverSkin;
		public Texture ButtonCloseClickSkin;

		public libGUI(RenderWindow RWind) {
			Controls = new List<Control>();
			MouseHeldControls = new Dictionary<Key, Control>();

			WindowSkin = Texture.FromFile("content/textures/gui_elements/window.png");

			ButtonSkin = Texture.FromFile("content/textures/gui_elements/button.png");
			ButtonHoverSkin = Texture.FromFile("content/textures/gui_elements/button_hover.png");
			ButtonDisabledSkin = Texture.FromFile("content/textures/gui_elements/button_disabled.png");
			ButtonClickSkin = Texture.FromFile("content/textures/gui_elements/button_click.png");

			ButtonCloseSkin = Texture.FromFile("content/textures/gui_elements/button_close.png");
			ButtonCloseHoverSkin = Texture.FromFile("content/textures/gui_elements/button_close_hover.png");
			ButtonCloseClickSkin = Texture.FromFile("content/textures/gui_elements/button_close_click.png");

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
				bool IsMouse = Key >= Key.MouseButton1 && Key <= Key.MouseButton8;
				OnKeyEventArgs Evt = new OnKeyEventArgs(this, MousePos, Pressed, Key);

				if (IsMouse) {
					bool ClickedOnAny = false;

					foreach (var C in Controls)
						if (C.IsInside(MousePos)) {
							Control Ctrl = C.GetRecursiveClientAreaChildAt(MousePos) ?? C;
							ClickControl(Ctrl, Key, Pressed);
							ClickedOnAny = true;
						}

					if (!ClickedOnAny)
						ClickControl(null, Key, Pressed);
				}

				foreach (var C in Controls) {
					C.OnKey(Evt);

					if (Evt.Consumed)
						break;
				}
			};

			DefaultFont = new BMFont("content/fonts/proggy_clean_16.fnt");
			((BMFont)DefaultFont).LoadTextures("content/textures", TextureFilter.Linear);

			DebugFont = new BMFont("content/fonts/proggy_small_16.fnt");
			((BMFont)DebugFont).LoadTextures("content/textures", TextureFilter.Linear);

			AnonPro16 = new BMFont("content/fonts/anonymous_pro_16.fnt");
			AnonPro16.LoadTextures("content/textures", TextureFilter.Linear);

			AnonPro32 = new BMFont("content/fonts/anonymous_pro_32.fnt");
			AnonPro32.LoadTextures("content/textures", TextureFilter.Linear);
		}

		void ClickControl(Control C, Key MouseKey, bool Pressed) {
			if (!MouseHeldControls.ContainsKey(MouseKey))
				MouseHeldControls.Add(MouseKey, null);

			if (Pressed) {
				Control PressedOn = null;
				if ((PressedOn = MouseHeldControls[MouseKey]) == null) {
					if (C != null) {
						MouseHeldControls[MouseKey] = C;
						C.OnBeginHold(new OnKeyEventArgs(this, MousePos, Pressed, MouseKey));
					}
				} else {
					ClickControl(PressedOn, MouseKey, false);
					ClickControl(C, MouseKey, true);
				}
			} else {
				Control ReleasedOn = null;
				if ((ReleasedOn = MouseHeldControls[MouseKey]) != null) {
					MouseHeldControls[MouseKey] = null;
					ReleasedOn.OnEndHold(new OnKeyEventArgs(this, MousePos, Pressed, MouseKey));
				}
			}
		}

		public Control GetHeldControl(Key MouseKey) {
			if (MouseHeldControls.ContainsKey(MouseKey))
				return MouseHeldControls[MouseKey];

			return null;
		}

		public void Update(float Dt) {
			foreach (var C in Controls)
				C.Update(Dt);
		}

		public void AddControl(Control C) {
			Controls.Add(C);
		}

		public void RemoveControl(Control C) {
			Controls.Remove(C);
		}

		public void Draw() {
			float FT = Engine.FrameTime.Average();
			float FPS = 1.0f / FT;

			RenderState RS = Gfx.PeekRenderState();
			RS.EnableScissorTest = true;
			RS.ScissorRegion = new AABB(new Vector2(Engine.WindowWidth, Engine.WindowHeight));
			Gfx.PushRenderState(RS);

			Gfx.DrawText(DebugFont, new Vector2(2, Engine.WindowHeight - DebugFont.ScaledLineHeight * 2 - 2), string.Format("{0} ms\n{1} FPS", FT, FPS), Color.White);

			/*Color[] Colors = new Color[] { Color.White, Color.Black, Color.Yellow, Color.Cyan, Color.Red };

			for (int i = 0; i < Colors.Length; i++) {
				Gfx.DrawText(AnonPro32, new Vector2(10, Engine.WindowHeight - 50 - AnonPro32.ScaledLineHeight * i), "Hello Font World!", Colors[i]);
			}*/

			foreach (var C in Controls)
				C.Draw();

			Gfx.PopRenderState();
		}
	}
}
