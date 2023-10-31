using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	public enum RENDERDOC_Version {
		eRENDERDOC_API_Version_1_0_0 = 10000,    // RENDERDOC_API_1_0_0 = 1 00 00
		eRENDERDOC_API_Version_1_0_1 = 10001,    // RENDERDOC_API_1_0_1 = 1 00 01
		eRENDERDOC_API_Version_1_0_2 = 10002,    // RENDERDOC_API_1_0_2 = 1 00 02
		eRENDERDOC_API_Version_1_1_0 = 10100,    // RENDERDOC_API_1_1_0 = 1 01 00
		eRENDERDOC_API_Version_1_1_1 = 10101,    // RENDERDOC_API_1_1_1 = 1 01 01
		eRENDERDOC_API_Version_1_1_2 = 10102,    // RENDERDOC_API_1_1_2 = 1 01 02
		eRENDERDOC_API_Version_1_2_0 = 10200,    // RENDERDOC_API_1_2_0 = 1 02 00
		eRENDERDOC_API_Version_1_3_0 = 10300,    // RENDERDOC_API_1_3_0 = 1 03 00
		eRENDERDOC_API_Version_1_4_0 = 10400, // RENDERDOC_API_1_4_0 = 1 04 00
	}

	[StructLayout(LayoutKind.Sequential)]
	struct RENDERDOC_API {
		public IntPtr GetAPIVersionFunc;

		public IntPtr SetCaptureOptionU32Func;
		public IntPtr SetCaptureOptionF32Func;

		public IntPtr GetCaptureOptionU32Func;
		public IntPtr GetCaptureOptionF32Func;

		public IntPtr SetFocusToggleKeysFunc;
		public IntPtr SetCaptureKeysFunc;

		public IntPtr GetOverlayBitsFunc;
		public IntPtr MaskOverlayBitsFunc;

		public IntPtr ShutdownFunc;
		public IntPtr UnloadCrashHandlerFunc;

		// Get/SetLogFilePathTemplate was renamed to Get/SetCaptureFilePathTemplate in 1.1.2.
		public IntPtr SetCaptureFilePathTemplateFunc;

		public IntPtr GetCaptureFilePathTemplateFunc;

		public IntPtr GetNumCapturesFunc;
		public IntPtr GetCaptureFunc;

		public IntPtr TriggerCaptureFunc;

		// IsRemoteAccessConnected was renamed to IsTargetControlConnected in 1.1.1.
		public IntPtr IsTargetControlConnectedFunc;

		public IntPtr LaunchReplayUIFunc;

		public IntPtr SetActiveWindowFunc;

		public IntPtr StartFrameCaptureFunc;
		public IntPtr IsFrameCapturingFunc;
		public IntPtr EndFrameCaptureFunc;

		// new function in 1.1.0
		public IntPtr TriggerMultiFrameCaptureFunc;

		// new function in 1.2.0
		public IntPtr SetCaptureFileCommentsFunc;

		// new function in 1.4.0
		public IntPtr DiscardFrameCaptureFunc;
	}

	[UnmanagedFunctionPointer(RenderDoc.CConv)]
	internal unsafe delegate int RENDERDOC_GetAPIFunc(RENDERDOC_Version Version, RENDERDOC_API** API);

	[UnmanagedFunctionPointer(RenderDoc.CConv)]
	internal unsafe delegate void GetAPIVersionFunc(out int Major, out int Minor, out int Patch);

	[UnmanagedFunctionPointer(RenderDoc.CConv)]
	internal unsafe delegate void StartFrameCaptureFunc(void* Device, void* WindHandle);

	[UnmanagedFunctionPointer(RenderDoc.CConv)]
	internal unsafe delegate bool IsFrameCapturingFunc();

	[UnmanagedFunctionPointer(RenderDoc.CConv)]
	internal unsafe delegate bool EndFrameCaptureFunc(void* Device, void* WindHandle);

	public unsafe static class RenderDoc {
		internal const CallingConvention CConv = CallingConvention.Cdecl;

		public static bool Available { get; private set; }
		static RENDERDOC_API API;

		static int Major, Minor, Patch;

		[RenderDocAPIFunc]
		static GetAPIVersionFunc GetAPIVersion;

		[RenderDocAPIFunc]
		static StartFrameCaptureFunc StartFrameCapture;

		[RenderDocAPIFunc]
		static IsFrameCapturingFunc IsFrameCapturing;

		[RenderDocAPIFunc]
		static EndFrameCaptureFunc EndFrameCapture;

		static bool GetAPI() {
			IntPtr RenderDocDll = Kernel32.GetModuleHandle("renderdoc.dll");
			Available = RenderDocDll != IntPtr.Zero;

			if (!Available)
				return Available;

			RENDERDOC_GetAPIFunc RENDERDOC_GetAPI = Marshal.GetDelegateForFunctionPointer<RENDERDOC_GetAPIFunc>(Kernel32.GetProcAddress(RenderDocDll, nameof(RENDERDOC_GetAPI)));

			RENDERDOC_API* APIPtr = null;
			if (RENDERDOC_GetAPI(RENDERDOC_Version.eRENDERDOC_API_Version_1_4_0, &APIPtr) != 1) {
				Available = false;
				return Available;
			}

			API = *APIPtr;

			return Available;
		}

		internal static void Init() {
#if DEBUG
			/*if (!Debugger.IsAttached)
					Debugger.Launch();*/
#endif

			if (!GetAPI())
				return;

			Type APIType = API.GetType();
			FieldInfo[] RenderDocFuncs = typeof(RenderDoc).GetFields(BindingFlags.Static | BindingFlags.NonPublic).Where(F => F.GetCustomAttribute<RenderDocAPIFuncAttribute>() != null).ToArray();

			foreach (var Func in RenderDocFuncs) {
				IntPtr FuncPtr = (IntPtr)APIType.GetField(Func.FieldType.Name).GetValue(API);

				if (FuncPtr == IntPtr.Zero)
					Func.SetValue(null, null);
				else
					Func.SetValue(null, Marshal.GetDelegateForFunctionPointer(FuncPtr, Func.FieldType));
			}

			GetAPIVersion(out Major, out Minor, out Patch);
		}

		static bool IsInCapture;
		static bool CaptureQueued;
		static int CaptureFrames;

		public static void CaptureFrame(int Frames = 1) {
			CaptureFrames = Frames;
			CaptureQueued = true;
		}

		public static void StartCapture() {
			if (!Available)
				return;

			if (IsInCapture)
				throw new Exception("Capture in progress");

			StartFrameCapture(null, null);
			IsInCapture = true;
		}

		public static bool IsCapturing() {
			return IsInCapture;
		}

		public static void EndCapture() {
			if (!Available)
				return;

			if (!IsInCapture)
				throw new Exception("No capture in progress");

			EndFrameCapture(null, null);
			IsInCapture = false;
		}

		internal static void StartFrame() {
			if (CaptureQueued) {
				CaptureQueued = false;

				StartCapture();
			}
		}

		internal static void EndFrame() {
			if (IsInCapture) {

				CaptureFrames--;
				if (CaptureFrames <= 0) {
					EndCapture();


				}
			}
		}
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	sealed class RenderDocAPIFuncAttribute : Attribute {
	}
}
