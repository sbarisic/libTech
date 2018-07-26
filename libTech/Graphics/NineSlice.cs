using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;

namespace libTech.Graphics {
	public class NineSlice {
		public Texture Texture;

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

		float _Border;
		public float Border {
			get {
				return _Border;
			}
			set {
				if (_Border == value)
					return;

				_Border = value;
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

		public NineSlice(Texture NineSlice, float Border) {
			this.Border = Border;

			Color = Color.White;
			Rotation = Quaternion.Identity;
			Position = Vector2.Zero;
			Size = new Vector2(50, 50);

			Texture = NineSlice;
			Mesh = new Mesh2D();

			Boxes = new List<Tuple<int, AABB>>();
		}

		public int Collides(Vector2 Pos) {
			foreach (var Box in Boxes) {
				if (Box.Item2.IsInside(Pos))
					return Box.Item1;
			}

			return 0;
		}

		IEnumerable<Vertex2> EmitQuad(int ID, Vector2 Pos, Vector2 Size, Vector2 UV, Vector2 UVSize) {
			//Console.WriteLine("{0} - {1} .. {2}", ID, Pos, Pos + Size);

			Boxes.Add(new Tuple<int, AABB>(ID, new AABB(Pos, Size)));
			return Vertex2.CreateQuad(Pos, Size, UV, UVSize, Color);
		}

		void Refresh() {
			List<Vertex2> Verts = new List<Vertex2>();

			Vector2 Pos = Vector2.Zero;
			Vector2 CornerSize = new Vector2(Border, Border);
			Vector2 CornerSizeN = CornerSize / Texture.Size;

			Vector2 HBarSize = new Vector2(Size.X, Border);
			Vector2 HBarSizeN = new Vector2(Texture.Width - CornerSize.X * 2, Border) / Texture.Size;

			Vector2 VBarSize = new Vector2(Border, Size.Y);
			Vector2 VBarSizeN = new Vector2(Border, Texture.Height - CornerSize.X * 2) / Texture.Size;

			Vector2 MiddleSize = Size;
			Vector2 MiddleSizeN = (Texture.Size - CornerSize * 2) / Texture.Size;

			Boxes.Clear();
			Verts.AddRange(EmitQuad(1, Pos + Size.GetHeight() - CornerSize.GetWidth(), CornerSize, new Vector2(CornerSizeN.X, 1) - CornerSizeN, CornerSizeN));
			Verts.AddRange(EmitQuad(2, Pos + Size.GetHeight(), HBarSize, new Vector2(CornerSizeN.X, 1 - CornerSizeN.Y), HBarSizeN));
			Verts.AddRange(EmitQuad(3, Pos + Size, CornerSize, Vector2.One - CornerSizeN, CornerSizeN));

			Verts.AddRange(EmitQuad(4, Pos - CornerSize.GetWidth(), VBarSize, CornerSizeN.GetHeight(), VBarSizeN));
			Verts.AddRange(EmitQuad(5, Pos, MiddleSize, CornerSizeN, MiddleSizeN));
			Verts.AddRange(EmitQuad(6, Pos + Size - VBarSize.GetHeight(), VBarSize, CornerSizeN + HBarSizeN.GetWidth(), VBarSizeN));

			Verts.AddRange(EmitQuad(7, Pos - CornerSize, CornerSize, Vector2.Zero, CornerSizeN));
			Verts.AddRange(EmitQuad(8, Pos - CornerSize.GetHeight(), HBarSize, CornerSizeN.GetWidth(), HBarSizeN));
			Verts.AddRange(EmitQuad(9, Pos + Size.GetWidth() - CornerSize.GetHeight(), CornerSize, new Vector2(1, 0) - CornerSizeN.GetWidth(), CornerSizeN));

			Mesh.SetVertices(Verts.ToArray());
		}

		public void Draw() {
			if (Dirty) {
				Dirty = false;
				Refresh();
			}

			ShaderUniforms.Model = Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(new Vector3(Position, 0).Round());

			DefaultShaders.DefaultTextureColor2D.Bind();
			Texture.BindTextureUnit();
			Mesh.Draw();
			Texture.UnbindTextureUnit();
			DefaultShaders.DefaultTextureColor2D.Unbind();
		}
	}
}
