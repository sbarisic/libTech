using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using libTech.libNative;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Graphics {
	public static class DbgDraw {
		static int FreeLineIdx;
		static Vertex3[] LineVerts;

		static int FreeDepthTestedLineIdx;
		static Vertex3[] DepthTestedLineVerts;

		static ShaderProgram DebugDrawShader;
		static Mesh3D DebugMesh;

		public static bool Enabled;

		internal static void Init() {
			FreeLineIdx = FreeDepthTestedLineIdx = 0;
			LineVerts = new Vertex3[128];
			DepthTestedLineVerts = new Vertex3[128];

			DebugDrawShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/debug_draw.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/debug_draw.frag"));
			DebugMesh = new Mesh3D(BufferUsage.DynamicDraw);
			DebugMesh.PrimitiveType = PrimitiveType.Lines;
			DebugMesh.VAO.SetLabel(OpenGL.ObjectIdentifier.VertexArray, "DebugDraw vertex buffer");

			debug_draw.Init((Start, End, DepthEnabled) => AppendLine(Start, End, DepthEnabled));
		}

		static void AppendLine(Vertex3 Start, Vertex3 End, bool DepthEnabled) {
			ref int FreeIdx = ref FreeLineIdx;
			ref Vertex3[] Lines = ref LineVerts;

			if (DepthEnabled) {
				FreeIdx = ref FreeDepthTestedLineIdx;
				Lines = ref DepthTestedLineVerts;
			}

			if (FreeIdx >= Lines.Length - 1)
				Lines = new Vertex3[Lines.Length * 2];

			Lines[FreeIdx++] = Start;
			Lines[FreeIdx++] = End;
		}

		public static void DrawAxisTriad(Matrix4x4 Mat, float Size = 5, float Length = 30, bool DepthEnabled = true, int Time = 0) {
			if (!Enabled)
				return;

			debug_draw.debug_draw_axis_triad(Mat, Size, Length, DepthEnabled, Time);
		}

		public static void DrawAxisTriad(Vector3 Position, Quaternion Rotation, float Size = 5, float Length = 30, bool DepthEnabled = true, int Time = 0) {
			DrawAxisTriad(Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position), Size, Length, DepthEnabled, Time);
		}

		public static void DrawAABB(AABB Box, Matrix4x4? Transform = null, bool DepthEnabled = true) {
			if (!Enabled)
				return;

			throw new NotImplementedException();
		}

		public static void DrawCross(Vector3 Pos, float Length = 30, bool DepthEnabled = true, int Time = 0) {
			if (!Enabled)
				return;

			debug_draw.debug_draw_cross(Pos, Length, DepthEnabled, Time);
		}

		public static void DrawSphere(Vector3 Pos, float Radius, Color? Clr = null, bool DepthEnabled = true, int Time = 0) {
			if (!Enabled)
				return;

			debug_draw.debug_draw_sphere(Pos, Clr ?? Color.White, Radius, DepthEnabled, Time);
		}

		public static void DrawArrow(Vector3 From, Vector3 To, Color? Clr = null, float Size = 30, bool DepthEnabled = true, int Time = 0) {
			if (!Enabled)
				return;

			debug_draw.debug_draw_arrow(From, To, Clr ?? Color.White, Size, DepthEnabled, Time);
		}

		public static void DrawLine(Vector3 From, Vector3 To, Color? Clr = null, bool DepthEnabled = true, int Time = 0) {
			if (!Enabled)
				return;

			debug_draw.debug_draw_line(From, To, Clr ?? Color.White, DepthEnabled, Time);
		}

		public static void DrawCircle(Vector3 Center, Vector3 Normal, Color? Clr = null, float Radius = 20, int Steps = 14, bool DepthEnabled = true, int Time = 0) {
			if (!Enabled)
				return;

			debug_draw.debug_draw_circle(Center, Normal, Clr ?? Color.White, Radius, Steps, DepthEnabled, Time);
		}

		public static void DrawFrustum(Camera Cam, Color? Clr = null, bool DepthEnabled = true, int Time = 0) {
			if (!Enabled)
				return;

			Matrix4x4.Invert(Cam.View * Cam.Projection, out Matrix4x4 InverseClip);
			debug_draw.debug_draw_frustum(InverseClip, Clr ?? Color.White, DepthEnabled, Time);
		}

		internal static void FinalizeDraw(long TimeMS) {
			if (!Enabled)
				return;

			RenderAPI.DbgPushGroup("DbgDraw Finalize");

			debug_draw.debug_draw_flush(TimeMS);
			DebugDrawShader.Bind(ShaderUniforms.Current);
			RenderState RS = Gfx.PeekRenderState();

			// No depth test
			{
				RS.EnableDepthTest = false;
				Gfx.PushRenderState(RS);

				DebugMesh.SetVertices(LineVerts, FreeLineIdx, HasUVs: false);
				DebugMesh.Draw();

				Gfx.PopRenderState();
			}

			// Depth test
			{
				RS.EnableDepthTest = true;
				Gfx.PushRenderState(RS);

				DebugMesh.SetVertices(DepthTestedLineVerts, FreeDepthTestedLineIdx, HasUVs: false);
				DebugMesh.Draw();

				Gfx.PopRenderState();
			}

			DebugDrawShader.Unbind();
			FreeLineIdx = FreeDepthTestedLineIdx = 0;

			RenderAPI.DbgPopGroup();
		}
	}
}
