using libTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech.libNative {
	internal struct Vertex {
		public Vector4 Position;
		public byte A, B, G, R;

		public Vertex(byte R, byte G, byte B) {
			this.R = R;
			this.G = G;
			this.B = B;
			A = 255;

			Position = Vector4.Zero;
		}

		public Vector4 Color {
			get {
				return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
			}
		}
	}

	internal enum Im3dPrimitiveType : int {
		Triangles, Lines, Points
	}

	unsafe delegate void RenderFunc(Im3dPrimitiveType PrimitiveType, IntPtr Vertices, uint VertexCount);

	internal static unsafe class TinyGizmo {
		const string DllName = "libNative";
		//const CallingConvention CConv = CallingConvention.Cdecl;
		const CallingConvention CConv = CallingConvention.StdCall;

		public static Action<Im3dPrimitiveType, Vertex[]> OnRender = null;

		static void Render(Im3dPrimitiveType PrimitiveType, IntPtr Vertices, uint VertexCount) {
			OnRender?.Invoke(PrimitiveType, new Vertex[VertexCount].Fill(Vertices));
		}

		public static void GizmoInit() {
			RenderFunc F = Render;
			GCHandle.Alloc(F);
			GizmoInit(F);
		}

		[DllImport(DllName, CallingConvention = CConv)]
		static extern void GizmoInit(RenderFunc F);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GizmoBegin(float Dt, Vector2 ViewSize, float ClipNear, float ClipFar, float FOV,
			Vector3 CamPos, Vector3 CamDir, float ScreenSpaceScale, Vector3 Snapping);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GizmoInput(Vector3 RayDir, bool Left, bool Translate, bool Rotate, bool Scale, bool Local, bool Ctrl);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern bool Gizmo(string Name, ref Vector3 Position, ref Matrix3x3 Rotation, ref Vector3 Scale);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GizmoEnd();
	}
}
