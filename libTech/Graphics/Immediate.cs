using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using libTech.libNative;
using PrimitiveType = OpenGL.PrimitiveType;

namespace libTech.Graphics {
	public static partial class Immediate {
		static Mesh M;

		public static ShaderProgram TriangleShader;
		public static ShaderProgram PointShader;
		public static ShaderProgram LineShader;

		public static Matrix4x4 Matrix = Matrix4x4.Identity;

		static Immediate() {
			TinyGizmo.OnRender = (PrimType, Verts) => {
				Vector3[] Positions = Verts.Select((V) => V.Position.XYZ()).ToArray();
				Vector4[] Colors = Verts.Select((V) => V.Color).ToArray();

				if (PrimType == Im3dPrimitiveType.Triangles)
					Triangles(Positions, Colors);
				else if (PrimType == Im3dPrimitiveType.Lines)
					Lines(Positions, Colors, Verts.Select((V) => V.Position.W).ToArray());
				else
					Points(Positions, Colors, Verts.Select((V) => V.Position.W).ToArray());
			};

			TinyGizmo.GizmoInit();
		}

		static void UpdateMesh(Vector3[] Position, Vector4[] Color, Vector2[] UV, uint[] Indice, Texture Tex = null) {
			if (M == null)
				M = new Mesh(OpenGL.BufferUsage.DynamicDraw) {
					Material = new Material()
				};

			M.Material.Diffuse = Tex;
			M.SetVertices(Position);
			M.SetColors(Color);
			M.SetUVs(UV);
			M.SetElements(Indice);
		}

		static void DrawMesh(PrimitiveType PType, ShaderProgram Shader) {
			if (Shader == null)
				throw new Exception("No shader bound for immediate mode drawing");

			Shader.Bind();
			Shader.SetModelMatrix(Matrix);
			M.PrimitiveType = PType;
			M.Draw();
			Shader.Unbind();
		}

		public static void Points(Vector3[] Position, Vector4[] Color = null, float[] Size = null, uint[] Indice = null) {
			if (Size == null) {
				Size = new float[Position.Length];
				for (int i = 0; i < Size.Length; i++)
					Size[i] = 2;
			}

			UpdateMesh(Position, Color, Size.Select((S) => new Vector2(S)).ToArray(), Indice);
			DrawMesh(PrimitiveType.Points, PointShader);
		}

		public static void Lines(Vector3[] Position, Vector4[] Color = null, float[] Size = null, uint[] Indice = null) {
			if (Size == null) {
				Size = new float[Position.Length];
				for (int i = 0; i < Size.Length; i++)
					Size[i] = 2;
			}

			UpdateMesh(Position, Color, Size.Select((S) => new Vector2(S)).ToArray(), Indice);
			DrawMesh(PrimitiveType.Lines, LineShader);
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

		public static void Triangles(Vector3[] Position, Vector4[] Color = null, Vector2[] UV = null, uint[] Indice = null, Texture Tex = null) {
			UpdateMesh(Position, Color, UV, Indice, Tex);
			DrawMesh(PrimitiveType.Triangles, TriangleShader);
		}

		public static void GizmoInput(Vector2 Cursor, bool Left, bool Translate, bool Rotate, bool Scale, bool Local, bool Ctrl) {
			Cursor = ((Cursor / Camera.ActiveCamera.ViewportSize) * new Vector2(2) - Vector2.One) * new Vector2(1, -1);

			Vector3 RayDir = new Vector3(Cursor.X / Camera.ActiveCamera.Projection.M11, Cursor.Y / Camera.ActiveCamera.Projection.M22, -1);
			RayDir = Vector3.Normalize(RayDir);
			RayDir = Vector4.Transform(new Vector4(RayDir, 0), Camera.ActiveCamera.World).XYZ();

			TinyGizmo.GizmoInput(RayDir, Left, Translate, Rotate, Scale, Local, Ctrl);
		}

		public static bool Gizmo(/*string Name,*/float Dt, ref Vector3 Pos, ref Quaternion Rot, ref Vector3 Scale, Vector3? Snapping = null, float ScreenScale = 25) {
			Camera Cam = Camera.ActiveCamera;
			TinyGizmo.GizmoBegin(Dt, Engine.WindowSize, Cam.Near, Cam.Far, Cam.VerticalFOV, Cam.Position, Cam.WorldForwardNormal, ScreenScale, Snapping ?? Vector3.Zero);

			Matrix3x3 RotMat = Matrix4x4.CreateFromQuaternion(Rot);
			bool Ret = TinyGizmo.Gizmo("GizmoName", ref Pos, ref RotMat, ref Scale);
			Rot = Quaternion.CreateFromRotationMatrix(RotMat);

			TinyGizmo.GizmoEnd();
			return Ret;
		}

		public static void UseShaders(Action A) {
			ShaderProgram OldTriangle = TriangleShader;
			ShaderProgram OldPoint = PointShader;
			ShaderProgram OldLine = LineShader;

			A();

			TriangleShader = OldTriangle;
			PointShader = OldPoint;
			LineShader = OldLine;
		}
	}

	public static partial class Immediate {
		public static void Texture2D(Vector2 Pos, Texture Tex, bool PositionIsCenter = false, Vector2? Scale = null) {
			if (Scale == null)
				Scale = Vector2.One;

			if (PositionIsCenter)
				Pos -= new Vector2(Tex.Width / 2, Tex.Height / 2);

			Triangles(new Vector3[] {
				new Vector3(Pos.X, Pos.Y, 0),
				new Vector3(Pos.X, Pos.Y + Tex.Height * Scale.Value.Y, 0),
				new Vector3(Pos.X + Tex.Width * Scale.Value.X, Pos.Y + Tex.Height * Scale.Value.Y, 0),
				new Vector3(Pos.X + Tex.Width * Scale.Value.X, Pos.Y, 0),
			}, UV: new Vector2[] {
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(1, 1),
				new Vector2(1, 0)
			}, Indice: new uint[] {
				0, 1, 2, 0, 2, 3
			}, Tex: Tex);
		}
	}
}
