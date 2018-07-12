using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Numerics;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using CARP;

using libTech.GUI;
using libTech.Reflection;
using libTech.Importer;

using FishGfx.Graphics;
using FishGfx.System;
using System.ComponentModel;

namespace libTech {
	public unsafe static class Program {
		static RenderWindow Window;
		static LibTechGame Game;

		static void Main(string[] args) {
			string DllDirectory = "native/x86";

			if (IntPtr.Size == 8)
				DllDirectory = "native/x64";

			if (!Kernel32.SetDllDirectory(DllDirectory))
				throw new Win32Exception();

			AppDomain.CurrentDomain.AssemblyResolve += (S, E) => TryLoadAssembly(E.Name, DllDirectory);

			RunGame();
		}

		static void ParseVariables() {
			CVar.InitMode = true;
			CVar.Register("game", "basegame", CVarType.Replicated | CVarType.Init, (This, Old, New) => This.Value = Path.GetFullPath((string)New));

			CVar.Register("width", 1366, CVarType.Archive);
			CVar.Register("height", 768, CVarType.Archive);
			CVar.Register("borderless", false, CVarType.Archive);
			CVar.Register("resizable", false, CVarType.Archive);

			// Parse all arguments and set CVars
			foreach (var Arg in ArgumentParser.All) {
				switch (Arg.Key) {
					case "console":
						GConsole.Open = true;
						break;

					default: {
							CVar CVar = CVar.Find(Arg.Key);

							if (CVar != null)
								CVar.Value = Arg.Value.LastOrDefault();
							else
								CVar.Register(Arg.Key, Arg.Value.LastOrDefault());

							break;
						}
				}
			}

			CVar.InitMode = false;
			foreach (var CVar in CVar.GetAll())
				GConsole.WriteLine(CVar);
		}

		static Assembly TryLoadAssembly(string AssemblyName, string BasePath) {
			AssemblyName Name = new AssemblyName(AssemblyName);
			string ExpectedPath = Path.Combine(Path.GetFullPath(BasePath), Name.Name + ".dll");

			if (File.Exists(ExpectedPath))
				return Assembly.LoadFile(ExpectedPath);

			GConsole.WriteLine("Could not find " + Path.GetFileName(ExpectedPath));
			return null;
		}

		static void LoadGameDll(string BasePath) {
			string GameFolder = CVar.GetString("game");
			BasePath = Path.Combine(GameFolder, "Game.dll");

			Assembly GameAssembly = Reflect.LoadAssembly(BasePath);
			Type[] GameImplementations = Reflect.GetAllImplementationsOf(GameAssembly, typeof(LibTechGame)).ToArray();

			if (GameImplementations.Length == 0) {
				GConsole.WriteLine("No game implementations found in " + BasePath);
				Environment.Exit(0);
			} else if (GameImplementations.Length > 1) {
				GConsole.WriteLine("Too many game implementations in " + BasePath);
				Environment.Exit(0);
			}

			AppDomain.CurrentDomain.AssemblyResolve += (S, E) => TryLoadAssembly(E.Name, BasePath);
			Importers.RegisterAll(GameAssembly);

			Game = (LibTechGame)Activator.CreateInstance(GameImplementations[0]);
			Game.Load();
		}

		static void RunGame() {
			ParseVariables();

			FileWatcher.Init("content");
			Importers.RegisterAll(Reflect.GetExeAssembly());

			Window = new RenderWindow(CVar.GetInt("width", 800), CVar.GetInt("height", 600), "libTech", CVar.GetBool("resizable"));
			LoadGameDll(CVar.GetString("game"));

			//FreetypeText.Init();
			//FreetypeFont Font = FreetypeText.LoadFont("content/fonts/Hack.ttf");
			//FreetypeFont Fnt = new FreetypeFont("content/fonts/gt-pressura-mono-light.ttf");

			FreetypeFont Fnt = new FreetypeFont("content/fonts/Hack.ttf");
			Fnt.FontSize = 24;

			using (Bitmap Bmp = new Bitmap(1512, 200))
			using (Graphics Gfx = Graphics.FromImage(Bmp)) {
				Gfx.Clear(Color.Black);
				Gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

				string Str = "The quick brown fox jumps over the lazy dog! 1234567890";

				Fnt.GetGlyphs(Str, new Vector2(10, 150), (Char, Glyph, Pos) => {
					if (Glyph.Bitmap != null)
						Gfx.DrawImage(Glyph.Bitmap, Pos.X, Pos.Y);
				});

				Vector2 Size = Fnt.MeasureString(Str);
				Gfx.DrawRectangle(Pens.Blue, 10, 80, Size.X, Size.Y);

				Fnt.GetGlyphs(Str, new Vector2(10, 80), (Char, Glyph, Pos) => {
					if (Glyph.Bitmap != null) {
						Gfx.DrawRectangle(Pens.Red, Pos.X, Pos.Y, Glyph.Size.X, Glyph.Size.Y);
						Gfx.DrawImage(Glyph.Bitmap, Pos.X, Pos.Y);
					}
				});

				Gfx.Flush();
				Bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
				Bmp.Save("out.png");
			}

			//Bitmap Bmp = Fnt.GetGlyphImage('H');
			//Bmp.Save("h.png");

			//libGUI GUI = new libGUI();

			while (!Window.ShouldClose) {
				Gfx.Clear();

				Window.SwapBuffers();
				Events.Poll();
			}
		}

	}
}
