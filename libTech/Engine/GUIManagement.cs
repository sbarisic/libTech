using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FishGfx.Graphics;
using libTech.Entities;
using libTech.Graphics;
using libTech.GUI;

namespace libTech {
	public unsafe static partial class Engine {
		static List<libGUI> GUIs = new List<libGUI>();

		public static libGUI CreateGUI() {
			libGUI GUI = new libGUI();
			GUIs.Add(GUI);
			return GUI;
		}

		public static Window CreateYesNoPrompt(libGUI GUI, string Question, Action OnYes = null, Action OnNo = null) {
			float Padding = 10;

			Label TextLabel = new Label(DefaultFonts.MainMenuMedium, Question);
			Window Wnd = new Window(Vector2.Zero, new Vector2(TextLabel.Size.X + Padding * 2, (Question.Count(c => c == '\n') + 2) * DefaultFonts.MainMenuMedium.LineHeight * 2));
			Wnd.Resizable = false;

			Wnd.AddChild(TextLabel);
			TextLabel.Position = new Vector2(Padding, Wnd.Size.Y - DefaultFonts.MainMenuMedium.LineHeight);

			TextButton NoBtn = Wnd.AddChild(new TextButton(DefaultFonts.MainMenuMedium, "No"));
			NoBtn.Position = new Vector2(Wnd.Size.X - NoBtn.Size.X - Padding, Padding);

			NoBtn.OnClick += (K, P) => {
				if (K == Key.MouseLeft) {
					OnNo?.Invoke();
					GUI.RemoveChild(Wnd);
				}
			};

			TextButton YesBtn = Wnd.AddChild(new TextButton(DefaultFonts.MainMenuMedium, "Yes"));
			YesBtn.Position = new Vector2(Wnd.Size.X - NoBtn.Size.X - YesBtn.Size.X - Padding * 3, Padding);

			YesBtn.OnClick += (K, P) => {
				if (K == Key.MouseLeft) {
					OnYes?.Invoke();
					GUI.RemoveChild(Wnd);
				}
			};

			//Wnd.AutoResize(new Vector2(Padding));

			//Wnd.Center(Engine.Window.GetWindowSizeVec() / 2);
			Wnd.Center(Engine.Window.GetWindowSizeVec() / 2);
			GUI.AddChild(Wnd);
			return Wnd;
		}

		internal static void DrawAllGUI() {
			foreach (var GUI in GUIs)
				GUI.Draw();
		}

		internal static void OnMouseMove(Vector2 Pos) {
			foreach (var GUI in GUIs)
				GUI.OnMouseMove(Pos);
		}

		internal static void OnKey(Key Key, int Scancode, bool Pressed, bool Repeat, KeyMods Mods) {
			foreach (var GUI in GUIs)
				GUI.OnKey(Key, Scancode, Pressed, Repeat, Mods);
		}
	}
}
