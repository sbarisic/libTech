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
	public static class GConsole {
		public static bool Open = false;
		public static Color Color = Color.White;
		public static bool Echo = true;

		//static Window ConsoleWindow;
		//static Label Output;
		//static InputBox Input;

		static StringBuilder TempBuffer = new StringBuilder();
		static Dictionary<string, string> Aliases = new Dictionary<string, string>();

		internal static void Init() {
			/*ConsoleWindow = new Window();
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
			}//*/

			ConVar<float> ConHeight = ConVar.Register("con_height", 0.4f, ConVarType.Archive);
			//const float Padding = 10;

			//ConsoleWindow.Position = new Vector2(Padding, (int)(Engine.WindowHeight * ConHeight) + Padding);
			//ConsoleWindow.Size = new Vector2(Engine.WindowWidth - Padding * 2, (int)(Engine.WindowHeight * (1.0f - ConHeight)) - Padding * 2);


			ConCmd.Register("clear", (Argv) => {
				Clear();
			});

			ConCmd.Register("rainbow", (Argv) => {
				string Rnd = Utils.RandomString(40, 60);

				Color Clr = Color;
				foreach (var Char in Rnd) {
					Color = Utils.RandomColor();
					Write(Char.ToString());
				}
				Write("\n");

				Color = Clr;
			});

			ConCmd.Register("echo", (Argv) => {
				for (int i = 1; i < Argv.Length; i++) {
					string Arg = Argv[i];

					/*if (Arg.StartsWith("$")) {
						if (ConVar.TryFind(Arg.Substring(1), out ConVar Var))
							Write(Var.ObjectValue);
						else
							Write("null");
					} else*/
					Write(Arg);
				}
				Write("\n");
			});

			ConCmd.Register("alias", (Argv) => {
				if (Argv.Length == 1) {
					foreach (var Alias in GetAliases())
						WriteLine("{0} -> {1}", Alias.Item1, Alias.Item2);
				} else if (Argv.Length == 2) {
					foreach (var Alias in GetAliases()) {
						if (Alias.Item1.StartsWith(Argv[1]))
							WriteLine("{0} -> {1}", Alias.Item1, Alias.Item2);
					}
				} else if (Argv.Length == 3) {
					WriteLine("{0} -> {1}", Argv[1], Argv[2]);
					RegisterAlias(Argv[1], Argv[2]);
				} else
					Error("alias\nalias <command_alias>\nalias <command_alias> <command>");
			});

			ConCmd.Register("var", (Argv) => {
				if (Argv.Length != 2) {
					Error("var <variable_name>");
					return;
				}

				ConVar.Register(Argv[1], 0);
			});

			ConCmd.Register("inc", (Argv) => {
				if (Argv.Length != 2) {
					Error("inc <variable_name>");
					return;
				}

				if (ConVar.TryFind(Argv[1], out ConVar Var))
					((ConVar<int>)Var).Value++;
			});

			ConCmd.Register("dec", (Argv) => {
				if (Argv.Length != 2) {
					Error("dec <variable_name>");
					return;
				}

				if (ConVar.TryFind(Argv[1], out ConVar Var))
					((ConVar<int>)Var).Value--;
			});

			ConCmd.Register("toggle", (Argv) => {
				if (Argv.Length != 2) {
					Error("toggle <variable_name>");
					return;
				}

				if (ConVar.TryFind(Argv[1], out ConVar Var)) {
					ConVar<int> IntVar = (ConVar<int>)Var;

					if (IntVar.Value == 0)
						IntVar.Value = 1;
					else
						IntVar.Value = 0;
				}
			});

			ConCmd.Register("cvarlist", (Argv) => {
				foreach (var CVar in ConVar.GetAll())
					WriteLine(CVar);
			});

			ConCmd.Register("cmdlist", (Argv) => {
				foreach (var Cmd in ConCmd.GetAll())
					WriteLine(Cmd);
			});

			ConCmd.Register("eval", (Argv) => {
				SendInputQuiet(string.Join("", Argv.Skip(1)));
			});

			ConCmd.Register("mousepos", (Argv) => {
				GConsole.WriteLine((Engine.Window.WindowSize.GetHeight() - Engine.Window.MousePos).Abs());
			});

			ConCmd.Register("crash", (Argv) => {
				throw new InvalidOperationException("Crashing the program!");
			});
		}

		public static void RegisterAlias(string Alias, string Command) {
			Alias = Alias.Trim();

			if (ConItems.TryFind(Alias, out ConItem Itm)) {
				Error("Cannot shadow a command/variable with an alias");
				return;
			}

			if (Aliases.ContainsKey(Alias))
				Aliases.Remove(Alias);

			Aliases.Add(Alias, Command);
		}

		public static IEnumerable<Tuple<string, string>> GetAliases() {
			return Aliases.Select(KV => new Tuple<string, string>(KV.Key, KV.Value));
		}

		internal static void Update() {
			// TODO
		}

		public static void Clear() {
			// TODO
		}

		public static void Write(string Msg) {
			Console.Write(Msg);

			// TODO
		}

		public static void Write(object Val) {
			Write((Val ?? "null").ToString());
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

		public static void Error(string Msg) {
			Color OldClr = Color;
			Color = new Color(255, 60, 60);
			WriteLine(Msg);
			Color = OldClr;
		}

		public static void Error(string Fmt, params object[] Args) {
			Error(string.Format(Fmt, Args));
		}

		public static void SendInputQuiet(string Line) {
			bool OldEcho = Echo;
			Echo = false;
			SendInput(Line);
			Echo = OldEcho;
		}

		public static void SendInput(string Line) {
			// Ignore empty
			Line = Line.Trim();
			if (Line.Length == 0 || Line.StartsWith("//"))
				return;

			Color = Color.White;
			if (Echo)
				WriteLine("> {0}", Line);
			string[] ArgvLine = ParseLine(Line).ToArray();

			foreach (var Argv in ArgvLine.Split(";")) {
				ConVar Var;

				for (int i = 0; i < Argv.Length; i++) {
					if (Argv[i].StartsWith("$") && ConVar.TryFind(Argv[i].Substring(1), out Var))
						Argv[i] = Var.StringValue;
				}

				if (Argv.Length == 1 && Aliases.ContainsKey(Argv[0])) {
					bool OldEcho = Echo;
					Echo = false;
					SendInput(Aliases[Argv[0]]);
					Echo = OldEcho;
					return;
				}

				try {
					if (Argv.Length == 2 && (Argv[0] == "+" || Argv[0] == "-")) {
						if (ConVar.TryFind(Argv[1], out Var)) {
							if (Var is ConVar<int>)
								((ConVar<int>)Var).Value = Argv[0] == "+" ? 1 : 0;
							else if (Var is ConVar<bool>)
								((ConVar<bool>)Var).Value = Argv[0] == "+" ? true : false;
							else
								Error("Unknown variable type '{0}'", Var.ObjectValue.GetType().Name);
						} else
							Error("Cannot toggle non existing variable '{0}'", Argv[1]);
					} else if ((Argv.Length == 1 || Argv.Length == 2) && ConVar.TryFind(Argv[0], out Var)) {
						if (Argv.Length == 2)
							Var.StringValue = Argv[1];

						if (Echo)
							WriteLine(Var);
					} else if (Argv.Length == 3 && Argv[1] == "=" && ConVar.TryFind(Argv[0], out Var)) {
						Var.StringValue = Argv[2];

						if (Echo && Argv.Length != 3)
							WriteLine(Var);
					} else if (Argv.Length > 0 && ConCmd.TryFind(Argv[0], out ConCmd Cmd)) {
						Cmd.Command(Argv);
					} else
						Error("Unknown command/variable '{0}'", Argv[0]);
				} catch (Exception E) {
					Engine.LogFatal(E);

					Error("{0}: {1}", E.GetType().Name, E.Message);
				}
			}
		}


		static StringBuilder ParseBuilder = new StringBuilder();

		static IEnumerable<string> ParseLine(string Line) {
			ParseBuilder.Length = 0;
			//Line = Line.Replace("=", " = ").Replace(";", " ; ").Replace("\r", "").Replace("\n", "");
			bool InsideQuote = false;

			const string Symbols = "+-=;";

			char LastChr = (char)0;
			foreach (var Chr in Line) {
				if (!InsideQuote && Chr == ' ') {
					if (ParseBuilder.Length > 0) {
						yield return ParseBuilder.ToString();
						ParseBuilder.Length = 0;
					}
				} else if (!InsideQuote && Symbols.Contains(Chr)) {
					if (ParseBuilder.Length > 0) {
						yield return ParseBuilder.ToString();
						ParseBuilder.Length = 0;
					}

					yield return Chr.ToString();
				} else if (Chr == '"' && LastChr != '\\')
					InsideQuote = !InsideQuote;
				else if (Chr == '"' && LastChr == '\\') {
					ParseBuilder.Length--;
					ParseBuilder.Append('"');
				} else
					ParseBuilder.Append(Chr);

				LastChr = Chr;
			}

			if (ParseBuilder.Length > 0)
				yield return ParseBuilder.ToString();
		}
	}
}
