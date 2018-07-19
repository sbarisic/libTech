using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using libTech.Graphics;
using libTech.GUI;
using FishGfx.Graphics;
using FishGfx;

namespace libTech {
	// TODO

	public static class GConsole {
		public static bool Open = false;
		public static Color Color = Color.White;

		static Window ConsoleWindow;

		static Label Output;
		static InputBox Input;

		static StringBuilder TempBuffer = new StringBuilder();

		internal static void Init() {
			ConsoleWindow = new Window();
			ConsoleWindow.ResizableHorizontal = false;
			ConsoleWindow.Movable = false;
			ConsoleWindow.Color = new Color(255, 255, 255, 200);
			ConsoleWindow.MinimumSize = new Vector2(0, 100);

			Input = ConsoleWindow.AddChild(new InputBox(DefaultFonts.ConsoleFont));
			Input.OnTextEntered += (In, Txt) => {
				In.String = "";
				SendInput(Txt);
			};

			Output = ConsoleWindow.AddChild(new Label(DefaultFonts.ConsoleFont));
			Output.Multiline = true;
			ConsoleWindow.OnResize += (Wnd, Sz) => {
				float SpacingOffset = 1.5f;
				Output.Position = new Vector2(0, DefaultFonts.ConsoleFont.LineSpacing * SpacingOffset);
				Output.DrawRegion = new AABB(new Vector2(Sz.X, Sz.Y - DefaultFonts.ConsoleFont.LineSpacing * SpacingOffset));

				Input.DrawRegion = new AABB(new Vector2(Sz.X, DefaultFonts.ConsoleFont.LineSpacing));
			};

			if (TempBuffer.Length > 0) {
				Output.AppendString(TempBuffer.ToString());
				TempBuffer.Clear();
			}

			const float HRatio = 0.75f;
			const float Padding = 15;

			ConsoleWindow.Position = new Vector2(Padding, (int)(Engine.WindowHeight * HRatio) + Padding);
			ConsoleWindow.Size = new Vector2(Engine.WindowWidth - Padding * 2, (int)(Engine.WindowHeight * (1.0f - HRatio)) - Padding * 2);
		}

		internal static void Update() {
			if (Open && ConsoleWindow.Parent == null)
				Engine.GUI.AddChild(ConsoleWindow);
			else if (!Open && ConsoleWindow.Parent != null)
				Engine.GUI.RemoveChild(ConsoleWindow);
		}

		public static void Clear() {
			if (Output == null)
				return;

			Output.Clear();
			ConsoleWindow.Size = ConsoleWindow.Size;
		}

		public static void Write(string Msg) {
			Console.Write(Msg);

			if (Output == null) {
				TempBuffer.Append(Msg);
				return;
			}

			Output.AppendString(Msg, Color);
		}

		public static void WriteLine(string Msg) {
			Write(Msg + "\n");
		}

		public static void WriteLine(object Obj) {
			WriteLine(Obj.ToString());
		}

		public static void WriteLine(string Fmt, params object[] Args) {
			WriteLine(string.Format(Fmt, Args));
		}

		public static void SendInput(string Line) {
			Line = Line.Trim();
			if (Line.Length == 0)
				return;

			Color = Color.White;
			WriteLine("> {0}", Line);

			string[] Argv = ParseLine(Line);

			if (Argv[0] == "christmas") {
				string RndStr = Utils.RandomString(30, 60);

				foreach (var Chr in RndStr) {
					Color = Utils.RandomColor();
					Write(Chr.ToString());
				}

				Write("\n");

			} else {
				Color = new Color(255, 60, 60);
				WriteLine("Unknown command '{0}'", Argv[0]);
			}

			Color = Color.White;
		}

		static string[] ParseLine(string Line) {
			return Line.Split(' ');
		}
	}
}
