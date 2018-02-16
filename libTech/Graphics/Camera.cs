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
		public static float VerticalFOVFromHorizontal(float FOV, float Width, float Height) {
			//return 2 * atanf(tanf(XFov / 2) * (ViewSize.Y / ViewSize.X));
			return 2 * (float)Math.Atan(Math.Tan(FOV / 2) * (Height / Width));
		}

		public static Camera ActiveCamera;

		public float Near { get; private set; }
		public float Far { get; private set; }
		public float VerticalFOV { get; private set; }
		public float HorizontalFOV { get; private set; }

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

		Matrix4x4 _World;
		public Matrix4x4 World {
			get {
				Update();
				return _World;
			}
			private set {
				_World = value;
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

		/*Vector3 _Offset;
		public Vector3 Center {
			get {
				return _Offset;
			}
			set {
				Dirty = true;
				_Offset = value;
			}
		}*/

		/*Vector3 _Scale;
		public Vector3 Scale {
			get {
				return _Scale;
			}
			set {
				Dirty = true;
				_Scale = value;
			}
		}*/

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

		public Vector3 ForwardNormal { get { return -Vector3.UnitZ; } }
		public Vector3 RightNormal { get { return Vector3.UnitX; } }
		public Vector3 UpNormal { get { return Vector3.UnitY; } }

		public Vector3 WorldForwardNormal { get { return Vector3.Normalize(Vector4.Transform(new Vector4(ForwardNormal, 0), World).XYZ()); } }
		public Vector3 WorldRightNormal { get { return Vector3.Normalize(Vector4.Transform(new Vector4(RightNormal, 0), World).XYZ()); } }
		public Vector3 WorldUpNormal { get { return Vector3.Normalize(Vector4.Transform(new Vector4(UpNormal, 0), World).XYZ()); } }

		public Camera() {
			View = Matrix4x4.Identity;
			Projection = Matrix4x4.Identity;

			//Center = new Vector3(0, 0, 0);
			Position = new Vector3(0, 0, 0);
			//Scale = new Vector3(1, 1, 1);
			Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, 0);
		}

		public void SetOrthogonal(float Left, float Bottom, float Right, float Top, float NearPlane = 1, float FarPlane = 10000, bool PreserveCenter = false) {
			Projection = Matrix4x4.CreateOrthographicOffCenter(Left, Right, Bottom, Top, NearPlane, FarPlane);

			float Width = Math.Abs(Left - Right);
			float Height = Math.Abs(Bottom - Top);
			ViewportSize = new Vector2(Width, Height);

			this.Near = NearPlane;
			this.Far = FarPlane;

			/*if (!PreserveCenter)
				Center = new Vector3(ViewportSize / 2, 0);*/
		}

		public void SetPerspective(float Width, float Height, float HFOV = 1.5708f, float NearPlane = 1, float FarPlane = 7500, bool PreserveCenter = false) {
			HorizontalFOV = HFOV;
			Projection = Matrix4x4.CreatePerspectiveFieldOfView(VerticalFOV = VerticalFOVFromHorizontal(HFOV, Width, Height), Width / Height, NearPlane, FarPlane);
			//Projection = Matrix4x4.CreatePerspective(Width, Height, NearPlane, FarPlane);

			ViewportSize = new Vector2(Width, Height);

			this.Near = NearPlane;
			this.Far = FarPlane;

			/*if (!PreserveCenter)
				Center = new Vector3(Width / 2, Height / 2, 0);*/
		}

		/*public void LookAt(Vector3 Pos, Vector3 UpVector) {
			Matrix4x4.Decompose(Matrix4x4.CreateLookAt(Position, Pos, UpVector), out Vector3 S, out Quaternion LookAtRotation, out Vector3 T);
			Rotation = LookAtRotation;
		}

		public void LookAt(Vector3 Pos) {
			LookAt(Pos, Vector3.UnitY);
		}*/

		void Update() {
			if (!Dirty)
				return;
			Dirty = false;

			World = CreateModel(Position, Vector3.One, Rotation);

			Matrix4x4.Invert(World, out Matrix4x4 ViewMat);
			View = ViewMat;
		}

		public static Matrix4x4 CreateModel(Vector3 Position, Vector3 Scale, Quaternion Rotation) {
			return Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
		}
	}
}
