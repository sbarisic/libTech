using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	internal enum RENDERDOC_Version : int {
		eRENDERDOC_API_Version_1_0_0 = 10000,    // RENDERDOC_API_1_0_0 = 1 00 00
		eRENDERDOC_API_Version_1_0_1 = 10001,    // RENDERDOC_API_1_0_1 = 1 00 01
		eRENDERDOC_API_Version_1_0_2 = 10002,    // RENDERDOC_API_1_0_2 = 1 00 02
		eRENDERDOC_API_Version_1_1_0 = 10100,    // RENDERDOC_API_1_1_0 = 1 01 00
		eRENDERDOC_API_Version_1_1_1 = 10101,    // RENDERDOC_API_1_1_1 = 1 01 01
		eRENDERDOC_API_Version_1_1_2 = 10102, // RENDERDOC_API_1_1_2 = 1 01 02
	}

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate int RENDERDOC_GetAPIFunc(RENDERDOC_Version Version, out IntPtr[] APIPointers);

	public static class RenderDoc {
		public static bool Available { get; private set; }

		static IntPtr RenderDocDll;
		static RENDERDOC_GetAPIFunc RENDERDOC_GetAPI;

		internal static void Init() {
			RenderDocDll = Kernel32.LoadLibrary("renderdoc.dll");
			Available = RenderDocDll != IntPtr.Zero;

			if (!Available)
				return;

			if (!Debugger.IsAttached)
				Debugger.Launch();

			RENDERDOC_GetAPI = Marshal.GetDelegateForFunctionPointer<RENDERDOC_GetAPIFunc>(Kernel32.GetProcAddress(RenderDocDll, nameof(RENDERDOC_GetAPI)));
		}

	}
}
