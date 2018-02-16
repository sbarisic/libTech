using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using libTech;

namespace libTech.libNative {
	/*[StructLayout(LayoutKind.Sequential)]
	struct GizmoApplicationState {
		public bool mouse_left;
		public bool hotkey_translate;
		public bool hotkey_rotate;
		public bool hotkey_scale;
		public bool hotkey_local;
		public bool hotkey_ctrl;
		public float screenspace_scale;     // If > 0.f, the gizmos are drawn scale-invariant with a screenspace value defined here
		public float snap_translation;      // World-scale units used for snapping translation
		public float snap_scale;            // World-scale units used for snapping scale
		public float snap_rotation;         // Radians used for snapping rotation quaternions (i.e. PI/8 or PI/16)
		public Vector2 viewport_size;       // 3d viewport used to render the view
		public Vector2 cursor;              // Current cursor location in window coordinates
		public CameraParams cam;              // Used for constructing inverse view projection for raycasting onto gizmo geometry
	}

	[StructLayout(LayoutKind.Sequential)]
	struct CameraParams {
		public float yfov, near_clip, far_clip;
		public Vector3 position;
		public Quaternion orientation;
	}*/

	internal struct GizmoVertex {
		public Vector3 Position, Normal;
		public Vector4 Color;
	}

	unsafe delegate void RenderFunc(IntPtr Vertices, uint VertexCount, IntPtr Indices, uint IndexCount);

	internal static unsafe class TinyGizmo {
		const string DllName = "libNative";
		//const CallingConvention CConv = CallingConvention.Cdecl;
		const CallingConvention CConv = CallingConvention.StdCall;

		public static Action<GizmoVertex[], uint[]> OnRender;

		static void Render(IntPtr Vertices, uint VertexCount, IntPtr Indices, uint IndexCount) {
			OnRender?.Invoke(new GizmoVertex[VertexCount].Fill(Vertices), new uint[IndexCount].Fill(Indices));
		}

		public static void GizmoInit() {
			RenderFunc F = Render;
			GCHandle.Alloc(F);
			GizmoInit(F);
		}

		[DllImport(DllName, CallingConvention = CConv)]
		static extern void GizmoInit(RenderFunc F);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GizmoBegin(Vector2 ViewSize, float ClipNear, float ClipFar, float FOV,
			Vector3 CamPos, Quaternion CamRot, float ScreenSpaceScale, Vector3 Snapping);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GizmoInput(Vector2 Cursor, bool Left, bool Translate, bool Rotate, bool Scale, bool Local, bool Ctrl);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern bool Gizmo(string Name, ref Vector3 Position, ref Quaternion Rotation, ref Vector3 Scale);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GizmoEnd();
	}
}
