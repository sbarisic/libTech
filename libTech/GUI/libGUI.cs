using FishGfx;
using FishGfx.Graphics;
using FishGfx_Nuklear;
using FishMarkupLanguage;
using libTech.Graphics;
using NuklearDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.GUI {
	public class libGUI {
		FishGfxDevice Dev;
		RenderWindow RWind;

		List<FMLDocument> Docs;

		public void Init(RenderWindow RWind, ShaderProgram GUIShader) {
			this.RWind = RWind;
			Docs = new List<FMLDocument>();

			Dev = new FishGfxDevice(RWind.WindowSize, GUIShader);
			Dev.RegisterEvents(RWind);

			NuklearAPI.Init(Dev);
		}

		public void OnMouseMove(RenderWindow RWind, float X, float Y) {

		}

		public void OnKey(RenderWindow Wnd, Key Key, int Scancode, bool Pressed, bool Repeat, KeyMods Mods) {

		}

		public void OnChar(RenderWindow Wnd, string Char, uint Unicode) {

		}

		public void DrawDocument(FMLDocument Doc) {
			Docs.Add(Doc);
		}

		void PaintTags(IEnumerable<FMLTag> Tags) {
			foreach (var T in Tags)
				PaintTag(T);
		}

		void PaintTag(FMLTag Tag) {
			FMLAttributes Attrib = Tag.Attributes;

			switch (Tag.TagName) {
				case "root": {
						PaintTags(Tag.Children);
						break;
					}

				case "window": {
						NkPanelFlags Flags = NkPanelFlags.BorderTitle;

						if (Attrib.GetAttribute("minimizable", true))
							Flags |= NkPanelFlags.Minimizable;

						if (Attrib.GetAttribute("movable", false))
							Flags |= NkPanelFlags.Movable;

						if (Attrib.GetAttribute("resizable", false))
							Flags |= NkPanelFlags.Scalable;

						float X = Attrib.GetAttribute("x", 0.0f);
						float Y = Attrib.GetAttribute("y", 0.0f);
						float W = Attrib.GetAttribute("width", 50.0f);
						float H = Attrib.GetAttribute("height", 50.0f);

						NuklearAPI.Window(Attrib.GetAttribute("title", "Window"), X, Y, W, H, Flags, () => {
							NuklearAPI.LayoutRowDynamic(35);

							PaintTags(Tag.Children);
						});
						break;
					}

				case "button": {
						if (NuklearAPI.ButtonLabel(Attrib.GetAttribute("text", "Button"))) {
						}

						break;
					}

				default:
					throw new InvalidOperationException("Unknown tag name " + Tag.TagName);
			}
		}

		public void Draw() {
			Dev.WindowSize = RWind.WindowSize;

			NuklearAPI.Frame(() => {
				foreach (var D in Docs)
					PaintTags(D.Tags);

				/*const NkPanelFlags Flags = NkPanelFlags.BorderTitle | NkPanelFlags.MovableScalable | NkPanelFlags.Minimizable | NkPanelFlags.ScrollAutoHide;

				NuklearAPI.Window("Test Window", 100, 100, 200, 200, Flags, () => {
					NuklearAPI.LayoutRowDynamic(35);

					for (int i = 0; i < 10; i++) {
						if (NuklearAPI.ButtonLabel("Some Button #" + i))
							Console.WriteLine("You pressed Some Button #" + i);
					}
				});*/

			});

			Docs.Clear();
		}
	}
}
