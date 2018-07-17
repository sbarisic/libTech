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
		libGUI GUI;

		static string[] QuitPrompts = new string[] {
				"Are you this stupid?",
				"     Really?",
				"Whyyyyyyyyy?",
				"Are you sure you really want to quit?",
				"Do you really want to quit?",
				"Quit? Quitting is for losers.",
				"Your mum didn't quit ME last night.",
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

		public override void Load() {
			FreetypeFont MenuFont = DefaultFonts.MainMenuMedium;
			float ButtonHeight = MenuFont.LineHeight * 2.2f;
			float Padding = 5;

			GUI = Engine.CreateGUI();

			Window MainMenuWindow = GUI.AddChild(new Window(new Vector2(100, 100), Vector2.Zero));
			MainMenuWindow.Resizable = false;

			TextButton NewGame = MainMenuWindow.AddChild(new TextButton(MenuFont, "New Game"));
			NewGame.Position = new Vector2(Padding, Padding + ButtonHeight * 3);

			TextButton JoinGame = MainMenuWindow.AddChild(new TextButton(MenuFont, "Join Game"));
			JoinGame.Position = new Vector2(Padding, Padding + ButtonHeight * 2);

			TextButton Settings = MainMenuWindow.AddChild(new TextButton(MenuFont, "Settings"));
			Settings.Position = new Vector2(Padding, Padding + ButtonHeight * 1);

			TextButton Quit = MainMenuWindow.AddChild(new TextButton(MenuFont, "Quit"));
			Quit.Position = new Vector2(Padding, Padding + ButtonHeight * 0);

			Quit.OnClick += (K, P) => {
				Engine.CreateYesNoPrompt(GUI, QuitPrompts.Random(), () => Environment.Exit(0)).Center((Engine.Window.GetWindowSizeVec() / 2).RandomAround(200));
			};

			MainMenuWindow.AutoResize(new Vector2(Padding));
		}
	}
}