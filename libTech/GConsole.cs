using NuklearDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	public static class GConsole {
		const int ConsoleBufferSize = 256;

		struct GConsoleEntry {
			public NkColor Clr;
			public string Msg;

			public override string ToString() {
				if (Msg != null)
					return string.Format("{0} {1}", Clr, Msg);

				return "";
			}
		}

		public static bool Open;

		static StringBuilder InBuffer = new StringBuilder();
		static GConsoleEntry[] OutBuffer = new GConsoleEntry[ConsoleBufferSize];
		static Stack<NkColor> LineColors = new Stack<NkColor>();
		static int MaxLineLen;

		static Dictionary<string, Action> Commands;

		static GConsole() {
			Commands = new Dictionary<string, Action>();
			Clear(true);
		}

		static void PushLine(string Line) {
			MaxLineLen = Line.Length;

			for (int i = 1; i < OutBuffer.Length; i++) {
				OutBuffer[i - 1] = OutBuffer[i];
				MaxLineLen = Math.Max(MaxLineLen, (OutBuffer[i].Msg ?? "").Length);
			}

			OutBuffer[OutBuffer.Length - 1] = new GConsoleEntry() { Clr = LineColors.Peek(), Msg = Line };
		}

		static void PushColor(byte R, byte G, byte B, byte A = 255) {
			LineColors.Push(new NkColor() { R = R, G = G, B = B, A = A });
		}

		static NkColor PopColor() {
			return LineColors.Pop();
		}

		static void PushText(string Msg) {
			string[] Lines = Msg.Split('\n');
			for (int i = 0; i < Lines.Length; i++)
				PushLine(Lines[i]);
		}

		public static void Clear(bool ClearColors = false) {
			if (ClearColors) {
				LineColors.Clear();
				PushColor(255, 255, 255);
			}

			for (int i = 0; i < OutBuffer.Length; i++)
				OutBuffer[i] = new GConsoleEntry();
			MaxLineLen = 0;
		}

		/*public static void Write(string Msg) {
			Console.Write(Msg);
			PushText(Msg);
		}

		public static void Write(object Obj) {
			Write(Obj.ToString());
		}

		public static void Write(string Fmt, params object[] Args) {
			Write(string.Format(Fmt, Args));
		}*/

		public static void WriteLine(string Msg) {
			//Write(Msg + "\n");
			Console.WriteLine(Msg);
			PushText(Msg);
		}

		public static void WriteLine(object Obj) {
			WriteLine(Obj.ToString());
		}

		public static void WriteLine(string Fmt, params object[] Args) {
			WriteLine(string.Format(Fmt, Args));
		}

		public static void RegisterCommand(string Name, Action A) {
			Commands.Add(Name, A);
		}

		public static void SendInput(string Line) {
			Line = Line.Trim();
			if (Line.Length <= 0)
				return;

			WriteLine(">> " + Line);

			if (Line.ToLower() == "clear") {
				Clear();
				return;
			} else if (Line.ToLower() == "terminate")
				Environment.Exit(0);
			else if (Commands.ContainsKey(Line.ToLower())) {
				Commands[Line.ToLower()]();
				return;
			}

			PushColor(255, 127, 127);
			WriteLine("Unknown command '{0}'", Line);
			PopColor();
		}

		public static void NuklearDraw(int X, int Y) {
			const NkPanelFlags Flags = NkPanelFlags.BorderTitle | NkPanelFlags.MovableScalable | NkPanelFlags.ClosableMinimizable;
			const string ConWindowName = "GConsole";

			if (Open) {
				NuklearAPI.Window(ConWindowName, X, Y, 600, 400, Flags, () => {
					NkRect Bounds = NuklearAPI.WindowGetBounds();
					if (Bounds.H > 85) {
						NuklearAPI.LayoutRowDynamic(Bounds.H - 85);

						NuklearAPI.Group("Console_OutBufferGroup", 0, () => {
							NuklearAPI.LayoutRowDynamic(12);

							for (int i = 0; i < OutBuffer.Length; i++) {
								GConsoleEntry E = OutBuffer[i];
								if (E.Msg != null)
									NuklearAPI.LabelColored(E.Msg, E.Clr);
							}
						});
					}

					NuklearAPI.LayoutRowDynamic();
					if (NuklearAPI.EditString(NkEditTypes.Field, InBuffer).HasFlag(NkEditEvents.Active) && NuklearAPI.IsKeyPressed(NkKeys.Enter)) {
						SendInput(InBuffer.ToString());
						InBuffer.Clear();
					}
				});

				if (NuklearAPI.WindowIsHidden(ConWindowName)) {
					NuklearAPI.WindowClose(ConWindowName);
					Open = false;
				}
			}
		}
	}
}
