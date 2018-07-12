using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	// TODO

	public static class GConsole {
		public static bool Open;

		static GConsole() {

		}

		public static void Clear() {
		}

		public static void WriteLine(string Msg) {
			Console.WriteLine(Msg);
		}

		public static void WriteLine(object Obj) {
			WriteLine(Obj.ToString());
		}

		public static void WriteLine(string Fmt, params object[] Args) {
			WriteLine(string.Format(Fmt, Args));
		}

		public static void SendInput(string Line) {

		}
	}
}
