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
	public class Text {
		bool Dirty;
		Mesh2D Mesh;

		string _String;
		public string String {
			get {
				return _String;
			}
			set {
				if (_String == value)
					return;

				_String = value;
				Dirty = true;
			}
		}

		public Vector3 Position;
		public Quaternion Rotation;

		FreetypeFont _Font;
		public FreetypeFont Font {
			get {
				return _Font;
			}
			set {
				if (_Font == value)
					return;

				_Font = value;
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

		public Vector2 StringSize {
			get;
			private set;
		}

		public Text(FreetypeFont Font, string Text, Vector3 Position, Quaternion Rotation, Color Color) {
			this.Font = Font;
			this.String = Text;
			this.Position = Position;
			this.Rotation = Rotation;
			this.Color = Color;

			Mesh = new Mesh2D();
		}

		public Text(FreetypeFont Font, string Text) : this(Font, Text, Vector3.Zero, Quaternion.Identity, Color.White) {
		}

		/*public Vector2 MeasureString() {
			return Font.MeasureString(String);
		}*/

		public void Refresh() {
			if (!Dirty)
				return;
			Dirty = false;

			List<Vertex2> Verts = new List<Vertex2>();

			Font.GetGlyphs(String, Vector2.Zero, (Char, Glyph, Pos) => {
				if (Glyph.Bitmap == null)
					return;

				Glyph.GetUV(Font.TextureAtlas, out float U, out float V, out float W, out float H);

				Verts.Add(new Vertex2(Pos + new Vector2(Glyph.Bitmap.Width, Glyph.Bitmap.Height), new Vector2(U + W, V + H), Color));
				Verts.Add(new Vertex2(Pos + new Vector2(Glyph.Bitmap.Width, 0), new Vector2(U + W, V), Color));
				Verts.Add(new Vertex2(Pos, new Vector2(U, V), Color));

				Verts.Add(new Vertex2(Pos + new Vector2(0, Glyph.Bitmap.Height), new Vector2(U, V + H), Color));
				Verts.Add(new Vertex2(Pos + new Vector2(Glyph.Bitmap.Width, Glyph.Bitmap.Height), new Vector2(U + W, V + H), Color));
				Verts.Add(new Vertex2(Pos, new Vector2(U, V), Color));
			});

			Mesh.SetVertices(Verts.ToArray());

			StringSize = Font.MeasureString(String);
		}

		public void Draw() {
			Refresh();
			ShaderUniforms.Model = Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);

			DefaultShaders.Text2D.Bind();
			Font.TextureAtlas.BindTextureUnit();
			Mesh.Draw();
			Font.TextureAtlas.UnbindTextureUnit();
			DefaultShaders.Text2D.Unbind();
		}
	}
}
