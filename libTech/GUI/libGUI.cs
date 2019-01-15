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
		List<Control> Controls;
		Vector2 MousePos;

		BMFont AnonPro;
		BMFont DefaultFont;
		BMFont DebugFont;

		public libGUI(RenderWindow RWind) {
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

			DefaultFont = new BMFont("content/fonts/proggy_clean_16.fnt");
			DefaultFont.LoadTextures("content/textures", TextureFilter.Linear);

			DebugFont = new BMFont("content/fonts/proggy_small_16.fnt");
			DebugFont.LoadTextures("content/textures", TextureFilter.Linear);

			AnonPro = new BMFont("content/fonts/anonymous_pro_16.fnt");
			AnonPro.LoadTextures("content/textures", TextureFilter.Linear);

			Window Wnd = new Window(WindowSkin);
			Wnd.DebugName = "Window A";
			Wnd.Position = new Vector2(50, 50);
			Wnd.Size = new Vector2(300, 300);
			//Wnd.SetFont(AnonPro);
			Controls.Add(Wnd);

			Window Wnd2 = new Window(WindowSkin);
			Wnd2.DebugName = "Window B";
			Wnd2.Position = new Vector2(50, 50);
			Wnd2.Size = new Vector2(100, 100);
			//Wnd2.SetFont(DefaultFont);
			Wnd.AddChild(Wnd2);
		}

		public void Update(float Dt) {
			foreach (var C in Controls)
				C.Update(Dt);
		}

		public void Draw() {
			float FT = Engine.FrameTime.Average();
			float FPS = 1.0f / FT;

			Gfx.DrawText(DebugFont, new Vector2(2, Engine.WindowHeight - DebugFont.ScaledLineHeight * 2 - 2), string.Format("{0} ms\n{1} FPS", FT, FPS), Color.White);

			foreach (var C in Controls)
				C.Draw();

		}
	}
}
