using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech_Bootstrapper {
	public static class Bootstrapper {
		[DllExport(CallingConvention.Cdecl)]
		public static int Entry() {
			libTech.Program.Main(Environment.GetCommandLineArgs().Skip(1).ToArray());
			return 0;
		}
	}
}
