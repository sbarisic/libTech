using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Graphics {
	public class NineSlice {
		Texture _Texture;
		public Texture Texture {
			get {
				return _Texture;
			}
			set {
				if (_Texture == value)
					return;

				_Texture = value;
				Dirty = true;
			}
		}

		public Quaternion Rotation;
		public Vector2 Position;

		Vector2 _Size;
		public Vector2 Size {
			get {
				return _Size;
			}
			set {
				if (_Size == value)
					return;

				_Size = value;
				Dirty = true;
			}
		}

		float _BorderTop;
		public float BorderTop {
			get {
				return _BorderTop;
			}
			set {
				if (_BorderTop == value)
					return;

				_BorderTop = value;
				Dirty = true;
			}
		}

		float _BorderLeft;
		public float BorderLeft {
			get {
				return _BorderLeft;
			}
			set {
				if (_BorderLeft == value)
					return;

				_BorderLeft = value;
				Dirty = true;
			}
		}

		float _BorderBottom;
		public float BorderBottom {
			get {
				return _BorderBottom;
			}
			set {
				if (_BorderBottom == value)
					return;

				_BorderBottom = value;
				Dirty = true;
			}
		}

		float _BorderRight;
		public float BorderRight {
			get {
				return _BorderRight;
			}
			set {
				if (_BorderRight == value)
					return;

				_BorderRight = value;
				Dirty = true;
			}
		}

		float _BorderLeftScale;
		public float BorderLeftScale {
			get {
				return _BorderLeftScale;
			}
			set {
				if (_BorderLeftScale == value)
					return;

				_BorderLeftScale = value;
				Dirty = true;
			}
		}

		float _BorderRightScale;
		public float BorderRightScale {
			get {
				return _BorderRightScale;
			}
			set {
				if (_BorderRightScale == value)
					return;

				_BorderRightScale = value;
				Dirty = true;
			}
		}

		float _BorderTopScale;
		public float BorderTopScale {
			get {
				return _BorderTopScale;
			}
			set {
				if (_BorderTopScale == value)
					return;

				_BorderTopScale = value;
				Dirty = true;
			}
		}

		float _BorderBottomScale;
		public float BorderBottomScale {
			get {
				return _BorderBottomScale;
			}
			set {
				if (_BorderBottomScale == value)
					return;

				_BorderBottomScale = value;
				Dirty = true;
			}
		}

		Color _Color;
		public Color Color {
			get {
				return _Color;
			}
			set {
				_Color = value;
				Dirty = true;
			}
		}


		Mesh2D Mesh;
		bool Dirty;

		List<Tuple<int, AABB>> Boxes;

		public NineSlice(Texture NineSlice, float BorderTop, float BorderBottom, float BorderLeft, float BorderRight) {
			this.BorderTop = BorderTop;
			this.BorderBottom = BorderBottom;
			this.BorderLeft = BorderLeft;
			this.BorderRight = BorderRight;

			BorderTopScale = BorderBottomScale = BorderLeftScale = BorderRightScale = 1;

			Color = Color.White;
			Rotation = Quaternion.Identity;
			Position = Vector2.Zero;
			Size = new Vector2(50, 50);

			Texture = NineSlice;
			Mesh = new Mesh2D();

			Boxes = new List<Tuple<int, AABB>>();
		}

		public NineSlice(Texture NineSlice, float Border) : this(NineSlice, Border, Border, Border, Border) {
		}

		public int Collides(Vector2 Pos) {
			foreach (var Box in Boxes) {
				if (Box.Item2.IsInside(Pos))
					return Box.Item1;
			}

			return 0;
		}

		/*IEnumerable<Vertex2> EmitQuad(int ID, Vector2 Pos, Vector2 Size, Vector2 UV, Vector2 UVSize) {
			//Console.WriteLine("{0} - {1} .. {2}", ID, Pos, Pos + Size);

			Boxes.Add(new Tuple<int, AABB>(ID, new AABB(Pos, Size)));
			return Vertex2.CreateQuad(Pos, Size, UV, UVSize, Color);
		}*/

		IEnumerable<Vertex2> EmitQuad2(int ID, Vector2 Pos, Vector2 Size, Vector2 UVPos, Vector2 UVSize) {
			Boxes.Add(new Tuple<int, AABB>(ID, new AABB(Pos, Size)));
			return Vertex2.CreateQuad(Pos, Size, UVPos / Texture.Size, UVSize / Texture.Size);
		}

		/*Vector2 ToUV(float X, float Y) {
			return new Vector2(X, Y) / Texture.Size;
		}*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void CalcPositions(Vector2 Size, bool PerformScale, out Vector2 A, out Vector2 B, out Vector2 C, out Vector2 D, out Vector2 E, out Vector2 F, out Vector2 G, out Vector2 H, out Vector2 I) {
			float BL = BorderLeft;
			float BR = BorderRight;
			float BT = BorderTop;
			float BB = BorderBottom;

			if (PerformScale) {
				BL *= BorderLeftScale;
				BR *= BorderRightScale;
				BT *= BorderTopScale;
				BB *= BorderBottomScale;
			}

			float WX = Size.X - BL - BR;

			A = new Vector2(0, Size.Y) - new Vector2(0, BT);
			B = A + new Vector2(BL, 0);
			C = B + new Vector2(WX, 0);
			D = new Vector2(0, BB);
			E = D + new Vector2(BL, 0);
			F = E + new Vector2(WX, 0);
			G = Vector2.Zero;
			H = G + new Vector2(BL, 0);
			I = H + new Vector2(WX, 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void CalcSizes(Vector2 Size, bool PerformScale, out Vector2 SA, out Vector2 SB, out Vector2 SC, out Vector2 SD, out Vector2 SE, out Vector2 SF, out Vector2 SG, out Vector2 SH, out Vector2 SI) {
			float BL = BorderLeft;
			float BR = BorderRight;
			float BT = BorderTop;
			float BB = BorderBottom;

			if (PerformScale) {
				BL *= BorderLeftScale;
				BR *= BorderRightScale;
				BT *= BorderTopScale;
				BB *= BorderBottomScale;
			}

			float WX = Size.X - BL - BR;
			float WY = Size.Y - BT - BB;

			SA = new Vector2(BL, BT);
			SB = new Vector2(WX, BT);
			SC = new Vector2(BR, BT);
			SD = new Vector2(BL, WY);
			SE = new Vector2(WX, WY);
			SF = new Vector2(BR, WY);
			SG = new Vector2(BL, BB);
			SH = new Vector2(WX, BB);
			SI = new Vector2(BR, BB);
		}

		void Refresh() {
			List<Vertex2> Verts = new List<Vertex2>();

			Vector2 A, B, C, D, E, F, G, H, I;
			CalcPositions(Size, true, out A, out B, out C, out D, out E, out F, out G, out H, out I);

			Vector2 SA, SB, SC, SD, SE, SF, SG, SH, SI;
			CalcSizes(Size, true, out SA, out SB, out SC, out SD, out SE, out SF, out SG, out SH, out SI);

			Vector2 UV_A, UV_B, UV_C, UV_D, UV_E, UV_F, UV_G, UV_H, UV_I;
			CalcPositions(Texture.Size, false, out UV_A, out UV_B, out UV_C, out UV_D, out UV_E, out UV_F, out UV_G, out UV_H, out UV_I);

			Vector2 UV_SA, UV_SB, UV_SC, UV_SD, UV_SE, UV_SF, UV_SG, UV_SH, UV_SI;
			CalcSizes(Texture.Size, false, out UV_SA, out UV_SB, out UV_SC, out UV_SD, out UV_SE, out UV_SF, out UV_SG, out UV_SH, out UV_SI);

			Boxes.Clear();
			Verts.AddRange(EmitQuad2(1, A, SA, UV_A, UV_SA));
			Verts.AddRange(EmitQuad2(2, B, SB, UV_B, UV_SB));
			Verts.AddRange(EmitQuad2(3, C, SC, UV_C, UV_SC));
			Verts.AddRange(EmitQuad2(4, D, SD, UV_D, UV_SD));
			Verts.AddRange(EmitQuad2(5, E, SE, UV_E, UV_SE));
			Verts.AddRange(EmitQuad2(6, F, SF, UV_F, UV_SF));
			Verts.AddRange(EmitQuad2(7, G, SG, UV_G, UV_SG));
			Verts.AddRange(EmitQuad2(8, H, SH, UV_H, UV_SH));
			Verts.AddRange(EmitQuad2(9, I, SI, UV_I, UV_SI));

			Mesh.SetVertices(Verts.ToArray());
		}

		public void Draw() {
			if (Dirty) {
				Dirty = false;
				Refresh();
			}

			//ShaderUniforms.
			Matrix4x4 OldModel = ShaderUniforms.Current.Model;
			ShaderUniforms.Current.Model = Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(new Vector3(Position, 0).Round());

			DefaultShaders.DefaultTextureColor2D.Bind(ShaderUniforms.Current);
			Texture.BindTextureUnit();
			Mesh.Draw();
			Texture.UnbindTextureUnit();
			DefaultShaders.DefaultTextureColor2D.Unbind();

			ShaderUniforms.Current.Model = OldModel;
		}
	}
}
