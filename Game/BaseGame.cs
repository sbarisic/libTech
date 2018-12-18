using FishGfx;
using FishGfx.Graphics;
using libTech;
using libTech.Entities;
using libTech.Graphics;
using libTech.GUI;
using libTech.Importer;
using libTech.libNative;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Color = FishGfx.Color;

namespace Game {
	public unsafe class Game : LibTechGame {
		static string[] QuitPrompts = new string[] {
				"     Really?",
				"Whyyyyyyyyy?",
				"Are you sure you really want to quit?",
				"Do you really want to quit?",
				"Are you not sure you do not want to not quit the game?",
				"(\"&#(!% (\"!/$!)\\ =!)\")",
				"Environment.Exit(0);",
				"The \"yes\" and \"no\" buttons on this prompt are reversed.",
				"Delete all your save files?",
				"Delete system32?",
				"      Yes. No.",
				"      No. Yes.",
				"         What?",
			};

		//Window MainMenuWindow;

		void SpawnMainMenu(FreetypeFont MenuFont) {
			/*float Padding = 5;
			float ButtonHeight = 40;

			MainMenuWindow = Engine.GUI.AddChild(new Window());
			MainMenuWindow.Resizable = false;
			MainMenuWindow.Movable = false;

			TextButton NewGame = MainMenuWindow.AddChild(new TextButton(MenuFont, "New Game", ButtonHeight));
			NewGame.Position = new Vector2(Padding, ButtonHeight * 3 + Padding * 4);

			TextButton JoinGame = MainMenuWindow.AddChild(new TextButton(MenuFont, "Join Game", ButtonHeight));
			JoinGame.Position = new Vector2(Padding, ButtonHeight * 2 + Padding * 3);

			TextButton Settings = MainMenuWindow.AddChild(new TextButton(MenuFont, "Settings", ButtonHeight));
			Settings.Position = new Vector2(Padding, ButtonHeight * 1 + Padding * 2);

			TextButton Quit = MainMenuWindow.AddChild(new TextButton(MenuFont, "Quit", ButtonHeight));
			Quit.Position = new Vector2(Padding, ButtonHeight * 0 + Padding);
			Quit.OnMouseClick += (K, P) => {
				Engine.CreateYesNoPrompt(QuitPrompts.Random(), () => Environment.Exit(0)).Center((Engine.Window.WindowSize / 2).RandomAround(200));
			};

			MainMenuWindow.AutoResize(new Vector2(Padding));
			MainMenuWindow.Center(Engine.Window.WindowSize / 2);*/


		}

		Text Txt;

		public override void Load() {
			SpawnMainMenu(DefaultFonts.MainMenuMedium);
			Txt = new Text(DefaultFonts.MainMenuSmall, "");

			for (int i = 0; i < 30; i++) {
				Txt.SetColor(new Color(255, 255, 255));
				Txt.String += string.Format("{0}. - ", i);

				for (int j = 0; j < (i * i) / 8 + 1; j++) {
					Txt.SetColor(new[] {
						new Color(20, 12, 28), new Color(68, 36, 52), new Color(48, 52, 109),
						new Color(78, 74, 78), new Color(133, 76, 48), new Color(52, 101, 36),
						new Color(208, 70, 72), new Color(117, 113, 97), new Color(89, 125, 206),
						new Color(210, 125, 44), new Color(133, 149, 161), new Color(109, 170, 44),
						new Color(210, 170, 153), new Color(109, 194, 202), new Color(218, 212, 94),
						new Color(222, 238, 214) }.Random());

					Txt.String += ("QWERTZUIOPASDFGHJKLYXCVBNMqwertzuiopasdfghjklyxcvbnm1234567890".Random());
				}

				Txt.String += "\n";
			}

			Txt.Position = new Vector2(0, Engine.WindowHeight);
			//Txt.Position = Vector2.Zero;
		}
	
		public override void DrawGUI(float Dt) {
			base.DrawGUI(Dt);
			
			//Gfx.Clear(new Color(100, 100, 100, 0));
			Gfx.Line(new Vertex2(new Vector2(0, 0), Color.Red), new Vertex2(new Vector2(100, -100), Color.Red), 10);

			Txt.Draw();
		}
	}
}