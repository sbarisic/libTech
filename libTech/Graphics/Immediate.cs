using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using libTech.libNative;
using PrimitiveType = OpenGL.PrimitiveType;

namespace libTech.Graphics {
	public static class Immediate {
		static Mesh M;

		public static ShaderProgram Shader;
		public static Matrix4x4 Matrix = Matrix4x4.Identity;

		static Immediate() {
			TinyGizmo.OnRender = (Verts, Inds) => Triangles(Verts.Select((V) => V.Position).ToArray(), Verts.Select((V) => V.Color).ToArray(), Indice: Inds);
			TinyGizmo.GizmoInit();
		}

		static void UpdateMesh(Vector3[] Position, Vector4[] Color, Vector2[] UV, uint[] Indice) {
			if (M == null) {
				M = new Mesh(OpenGL.BufferUsage.DynamicDraw);
				M.Material = new Material();
			}

			M.SetVertices(Position);
			M.SetColors(Color);
			M.SetUVs(UV);
			M.SetElements(Indice);
		}

		static void DrawMesh(PrimitiveType PType) {
			if (Shader == null)
				throw new Exception("No shader bound for immediate mode drawing");

			Shader.SetModelMatrix(Matrix);

			M.PrimitiveType = PType;
			Shader.Bind();
			M.Draw();
			Shader.Unbind();
		}

		public static void Lines(Vector3[] Position, Vector4[] Color = null, Vector2[] UV = null, uint[] Indice = null) {
			UpdateMesh(Position, Color, UV, Indice);
			DrawMesh(PrimitiveType.Lines);
		}

		public static void Line(Vector3 A, Vector3 B, Vector4 Color) {
			Lines(new Vector3[] { A, B }, new Vector4[] { Color, Color });
		}

		public static void Axes(Vector3 Origin, float Scale = 1) {
			Lines(new Vector3[] {
				Origin, Origin + new Vector3(Scale, 0, 0),
				Origin, Origin + new Vector3(0, Scale, 0),
				Origin, Origin + new Vector3(0, 0, Scale),
			}, new Vector4[] {
				new Vector4(1, 0, 0, 1), new Vector4(1, 0, 0, 1),
				new Vector4(0, 1, 0, 1), new Vector4(0, 1, 0, 1),
				new Vector4(0, 0, 1, 1), new Vector4(0, 0, 1, 1),
			});
		}

		public static void Triangles(Vector3[] Position, Vector4[] Color = null, Vector2[] UV = null, uint[] Indice = null) {
			UpdateMesh(Position, Color, UV, Indice);
			DrawMesh(PrimitiveType.Triangles);
		}

		public static void GizmoInput(Vector2 Cursor, bool Left, bool Translate, bool Rotate, bool Scale, bool Local, bool Ctrl) {
			TinyGizmo.GizmoInput(Cursor, Left, Translate, Rotate, Scale, Local, Ctrl);
		}

		/*public static void GizmoBegin(Camera DefaultCam, Vector3 Snapping, float ScreenScale = 25) {
			TinyGizmo.GizmoBegin(Engine.WindowSize, DefaultCam.Near, DefaultCam.Far, DefaultCam.VerticalFOV, DefaultCam.Position, DefaultCam.Rotation, ScreenScale, Snapping);
		}

		public static bool Gizmo(string Name, ref Vector3 Pos, ref Quaternion Rot, ref Vector3 Scale) {
			return TinyGizmo.Gizmo(Name, ref Pos, ref Rot, ref Scale);
		}

		public static void GizmoEnd() {
			TinyGizmo.GizmoEnd();
		}*/

		public static bool Gizmo(/*string Name,*/ ref Vector3 Pos, ref Quaternion Rot, ref Vector3 Scale, Camera Cam, Vector3? Snapping = null, float ScreenScale = 25) {
			TinyGizmo.GizmoBegin(Engine.WindowSize, Cam.Near, Cam.Far, Cam.VerticalFOV, Cam.Position, Cam.Rotation, ScreenScale, Snapping ?? Vector3.Zero);
			bool Ret = TinyGizmo.Gizmo("GizmoName", ref Pos, ref Rot, ref Scale);
			TinyGizmo.GizmoEnd();
			return Ret;
		}
	}
}
