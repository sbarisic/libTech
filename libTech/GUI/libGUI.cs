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
		Window Wnd;

		public libGUI() {
			Fnt = new BMFont("content/fonts/proggy.fnt", 16);
			Fnt.Userdata = Texture.FromFile("content/textures/proggy_0.png");

			Texture WindowSkin = Texture.FromFile("content/textures/gui_elements/nineslice_30.png");
			Wnd = new Window(WindowSkin, 30);
		}

		public void Draw(float Dt) {
			float FT = Engine.FrameTime.Average();
			float FPS = 1.0f / FT;

			Gfx.DrawText(Fnt, (Texture)Fnt.Userdata, new Vector2(0, Engine.WindowHeight - Fnt.LineHeight), string.Format("{0} ms\n{1} FPS", FT, FPS), Color.White);

			Wnd.Draw();
		}
	}

	class Window {
		NineSlice Skin;

		public Window(Texture Tex, float Border) {
			Skin = new NineSlice(Tex, Border);

			Skin.Position = new Vector2(100, 100);
			Skin.Size = new Vector2(400, 400);
		}

		public void Draw() {
			Skin.Draw();
		}
	}
}
