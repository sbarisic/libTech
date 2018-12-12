using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Diagnostics;
using libTech;
using libTech.Entities;
using libTech.Importer;
using libTech.libNative;
using libTech.GUI;
using libTech.Graphics;

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

		Window MainMenuWindow;

		void SpawnMainMenu(FreetypeFont MenuFont) {
			float Padding = 5;
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
			MainMenuWindow.Center(Engine.Window.WindowSize / 2);
		}

		public override void Load() {
			SpawnMainMenu(DefaultFonts.MainMenuMedium);

		}
	}
}