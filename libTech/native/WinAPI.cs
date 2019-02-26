using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	/*internal static class User32 {
		const string DllName = "User32";
		const CallingConvention CConv = CallingConvention.Cdecl;

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern IntPtr WindowFromDC(IntPtr DC);
	}*/

	internal static class Kernel32 {
		const string DllName = "kernel32";
		const CallingConvention CConv = CallingConvention.Winapi;

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern bool SetDllDirectory(string Path);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern IntPtr LoadLibrary(string Name);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern IntPtr GetModuleHandle(string Name);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern IntPtr GetProcAddress(IntPtr Handle, string Name);
	}
}
