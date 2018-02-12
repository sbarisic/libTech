using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using OpenGL;
using System.Numerics;

//using Matrix4 = System.Numerics.Matrix4x4;

namespace libTech.Graphics {
	public class Camera {
		public Matrix4x4 Projection { get; private set; }

		Matrix4x4 _View;
		public Matrix4x4 View {
			get {
				Update();
				return _View;
			}
			private set {
				_View = value;
			}
		}

		Vector3 _Position;
		public Vector3 Position {
			get {
				return _Position;
			}
			set {
				Dirty = true;
				_Position = value;
			}
		}

		Vector3 _Offset;
		public Vector3 Center {
			get {
				return _Offset;
			}
			set {
				Dirty = true;
				_Offset = value;
			}
		}

		Vector3 _Scale;
		public Vector3 Scale {
			get {
				return _Scale;
			}
			set {
				Dirty = true;
				_Scale = value;
			}
		}

		Quaternion _Rotation;
		public Quaternion Rotation {
			get {
				return _Rotation;
			}
			set {
				Dirty = true;
				_Rotation = value;
			}
		}

		bool Dirty;
		public Vector2 ViewportSize { get; private set; }

		public Camera() {
			View = Matrix4x4.Identity;
			Projection = Matrix4x4.Identity;

			Center = new Vector3(0, 0, 0);
			Position = new Vector3(0, 0, 0);
			Scale = new Vector3(1, 1, 1);
			Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, 0);
		}

		public void SetOrthogonal(float Left, float Bottom, float Right, float Top, float NearPlane = 1, float FarPlane = 10000, bool PreserveCenter = false) {
			Projection = Matrix4x4.CreateOrthographicOffCenter(Left, Right, Bottom, Top, NearPlane, FarPlane);

			float Width = Math.Abs(Left - Right);
			float Height = Math.Abs(Bottom - Top);
			ViewportSize = new Vector2(Width, Height);

			if (!PreserveCenter)
				Center = new Vector3(ViewportSize / 2, 0);
		}

		public void SetPerspective(float Width, float Height, float FoV = 1.5708f, float NearPlane = 1, float FarPlane = 10000, bool PreserveCenter = false) {
			Projection = Matrix4x4.CreatePerspectiveFieldOfView(FoV, Width / Height, NearPlane, FarPlane);
			ViewportSize = new Vector2(Width, Height);

			/*if (!PreserveCenter)
				Center = new Vector3(Width / 2, Height / 2, 0);*/
		}

		public void LookAt(Vector3 Pos, Vector3 UpVector) {
			Matrix4x4.Decompose(Matrix4x4.CreateLookAt(Position - Center, Pos, UpVector), out Vector3 S, out Quaternion LookAtRotation, out Vector3 T);
			Rotation = LookAtRotation;
		}

		public void LookAt(Vector3 Pos) {
			LookAt(Pos, Vector3.UnitY);
		}

		void Update() {
			if (!Dirty)
				return;
			Dirty = false;

			Matrix4x4.Invert(CreateModel(Center, Position, Scale, Rotation), out Matrix4x4 ViewMat);
			View = ViewMat;
		}

		public static Matrix4x4 CreateModel(Vector3 Offset, Vector3 Position, Vector3 Scale, Quaternion Rotation) {
			return Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(-(Offset * Scale)) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
		}
	}
}
