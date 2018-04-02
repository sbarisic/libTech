using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;

namespace CARP {
	public static class ArgumentParser {
		static bool Parsed = false;
		static string CmdLine;
		static string ExePath;
		static Dictionary<string, List<string>> Switches;
		static string CurrentSwitch;

		static void Parse() {
			if (Parsed)
				return;
			Parsed = true;

			CmdLine = Environment.CommandLine;
			Switches = new Dictionary<string, List<string>>();

			bool InQuote = false;
			StringBuilder CurTok = new StringBuilder();

			/*Console.WriteLine(CmdLine);
			Console.WriteLine();*/

			for (int i = 0; i < CmdLine.Length; i++) {
				char C = CmdLine[i];
				char PC = i > 0 ? CmdLine[i - 1] : (char)0;
				char PPC = i > 1 ? CmdLine[i - 2] : (char)0;

				if (C == '\"') {
					if (!InQuote) {
						InQuote = true;
						if (CurTok.Length > 0)
							EmitToken(CurTok.ToString());
						CurTok.Clear();
						//CurTok.Append(C);
					} else if (PC != '\\' || (PC == '\\' && PPC == '\\')) {
						InQuote = false;
						//CurTok.Append(C);
						EmitToken(CurTok.ToString());
						CurTok.Clear();
					} else
						CurTok.Append(C);
					continue;
				}

				if (!InQuote && char.IsWhiteSpace(C) && CurTok.Length > 0) {
					EmitToken(CurTok.ToString());
					CurTok.Clear();
				} else if (!char.IsWhiteSpace(C) || InQuote)
					CurTok.Append(C);
			}

			if (CurTok.Length > 0)
				EmitToken(CurTok.ToString());

			CmdLine = CmdLine.Substring(CmdLine.IndexOf(' ') + 1);
		}

		static void EmitToken(string Tok) {
			Tok = Tok.Replace("\\\\", "\\").Replace("\\\"", "\"");

			if (Tok.StartsWith("--")) {
				EmitSwitch(Tok.Substring(2));
				return;
			} else if (Tok.StartsWith("-")) {
				EmitSwitch(Tok.Substring(1, 1));
				if (Tok.Length > 2)
					EmitToken(Tok.Substring(2));
				return;
			}

			if (CurrentSwitch == null)
				ExePath = Tok;
			else
				Switches[CurrentSwitch].Add(Tok);
		}

		static void EmitSwitch(string S) {
			CurrentSwitch = S;
			if (!Switches.ContainsKey(CurrentSwitch))
				Switches.Add(CurrentSwitch, new List<string>());
		}

		static T ParseToT<T>(string Src) {
			Type TT = typeof(T);

			// If no value found, return default
			if (string.IsNullOrEmpty(Src))
				return default(T);

			// If it's a string, return it
			if (TT == typeof(string))
				return (T)(object)Src;

			// If it contains a public string constructor
			if (TT.GetConstructor(new Type[] { typeof(string) }) != null)
				return (T)Activator.CreateInstance(TT, Src);

			// If it has a Type.Parse static method
			MethodInfo ParseMethod;
			if ((ParseMethod = TT.GetMethod("Parse", new Type[] { typeof(string), typeof(IFormatProvider) })) != null)
				return (T)ParseMethod.Invoke(null, new object[] { Src, CultureInfo.InvariantCulture });
			else if ((ParseMethod = TT.GetMethod("Parse", new Type[] { typeof(string) })) != null)
				return (T)ParseMethod.Invoke(null, new object[] { Src });

			throw new NotImplementedException("Could not parse type " + TT);
		}

		////////////////////////////////////////////////////////////////////////////// Public functions

		/// <summary>
		/// Get all argument names and values passed to the program
		/// </summary>
		public static IEnumerable<KeyValuePair<string, string[]>> All {
			get {
				Parse();

				foreach (var V in Switches)
					yield return new KeyValuePair<string, string[]>(V.Key, V.Value.ToArray());
			}
		}

		/// <summary>
		/// Get exe path from command line
		/// </summary>
		public static string ExecutablePath {
			get {
				Parse();
				return ExePath;
			}
		}

		/// <summary>
		/// Get raw command line without the exe path
		/// </summary>
		public static string CommandLine {
			get {
				Parse();
				return CmdLine;
			}
		}

		/// <summary>
		/// Get defined values as string array for argument
		/// </summary>
		/// <param name="Name">Name of the argument</param>
		/// <returns>Value array</returns>
		public static string[] Get(string Name) {
			Parse();

			if (Switches.ContainsKey(Name))
				return Switches[Name].ToArray();
			return null;
		}

		/// <summary>
		/// Get defined values as array for argument parsed to type T
		/// </summary>
		/// <typeparam name="T">Return type, has to have either a constructor that takes a string or a static Parse method</typeparam>
		/// <param name="Name">Name of the argument</param>
		/// <returns>Value array</returns>
		public static T[] Get<T>(string Name = null) {
			string[] SrcArr = Get(Name);
			T[] Val = new T[SrcArr.Length];

			for (int i = 0; i < SrcArr.Length; i++)
				Val[i] = ParseToT<T>(SrcArr[i]);
			return Val;
		}

		/// <summary>
		/// Get last defined value for argument as string
		/// </summary>
		/// <param name="Name">Name of the argument</param>
		/// <returns>Value</returns>
		public static string GetSingle(string Name) {
			string[] Values = Get(Name);
			if (Values != null && Values.Length > 0)
				return Values[Values.Length - 1];
			if (Values != null && Values.Length == 0)
				return "";
			return null;
		}

		/// <summary>
		/// Get last defined value for argument parsed to type T
		/// </summary>
		/// <typeparam name="T">Return type, has to have either a constructor that takes a string or a static Parse method</typeparam>
		/// <param name="Name">Name of the argument</param>
		/// <returns>Value</returns>
		public static T GetSingle<T>(string Name = null) {
			// If no value name found, assume same as type name except lower case
			if (string.IsNullOrEmpty(Name))
				Name = typeof(T).Name.ToLower();

			return ParseToT<T>(GetSingle(Name));
		}

		/// <summary>
		/// Check if argument was defined
		/// </summary>
		/// <param name="Name">Name of argument</param>
		/// <returns>True when defined, False when not defined</returns>
		public static bool Defined(string Name) {
			return Get(Name) != null;
		}
	}
}
