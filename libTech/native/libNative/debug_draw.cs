using FishGfx;
using libTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech.libNative {
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct DrawVertex {
		public Vector3 Position;
		public Vector3 Color;
		public float Size;

		public static implicit operator Vertex3(DrawVertex DV) {
			return new Vertex3(DV.Position, DV.Color);
		}
	}

	internal delegate void draw_line_func(DrawVertex Start, DrawVertex End, bool DepthEnabled);

	internal static class debug_draw {
		const string DllName = "libNative";
		//const CallingConvention CConv = CallingConvention.Cdecl;
		const CallingConvention CConv = CallingConvention.StdCall;

		[DllImport(DllName, CallingConvention = CConv)]
		static extern void debug_draw_init(draw_line_func DrawLine);

		public static void Init(draw_line_func DrawLine) {
			GCHandle.Alloc(DrawLine);
			debug_draw_init(DrawLine);
		}

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void debug_draw_flush(long TimeMS);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void debug_draw_axis_triad(Matrix4x4 Mat, float Size, float Length, bool DepthEnabled, int Time);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void debug_draw_aabb(Vector3 Mins, Vector3 Maxs, Vector3 Color, bool DepthEnabled, int Time);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void debug_draw_cross(Vector3 Pos, float Length, bool DepthEnabled, int Time);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void debug_draw_sphere(Vector3 Pos, Vector3 Color, float Radius, bool DepthEnabled, int Time);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void debug_draw_arrow(Vector3 From, Vector3 To, Vector3 Color, float Size, bool DepthEnabled, int Time);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void debug_draw_line(Vector3 From, Vector3 To, Vector3 Color, bool DepthEnabled, int Time);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void debug_draw_circle(Vector3 Center, Vector3 Normal, Vector3 Color, float Radius, int Steps, bool DepthEnabled, int Time);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void debug_draw_frustum(Matrix4x4 invClipMatrix, Vector3 Color, bool DepthEnabled, int Time);
	}
}
