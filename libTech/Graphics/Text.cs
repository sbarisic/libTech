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
	struct ColorEntry {
		public int Index;
		public Color Color;
	}

	public class Text {
		bool Dirty;
		Mesh2D Mesh;

		bool EmptyString;

		string _String;
		public string String {
			get {
				return _String;
			}
			set {
				_String = value;
				EmptyString = _String.Length == 0;
				Dirty = true;
			}
		}

		Vector2 Offset;
		public Vector2 Position;
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

		List<ColorEntry> Colors = new List<ColorEntry>();

		Vector2 _StringSize;
		public Vector2 StringSize {
			get {
				if (EmptyString)
					return Vector2.Zero;

				Refresh();
				return _StringSize;
			}

			private set {
				_StringSize = value;
			}
		}

		public Text(FreetypeFont Font, string Text, Vector2 Position, Quaternion Rotation, Color Color) {
			this.Font = Font;
			this.String = Text;
			this.Position = Position;
			this.Rotation = Rotation;
			this.Color = Color;

			Mesh = new Mesh2D();
		}

		public Text(FreetypeFont Font, string Text) : this(Font, Text, Vector2.Zero, Quaternion.Identity, Color.White) {
		}

		public Text(FreetypeFont Font) : this(Font, "") {
		}

		public void SetColor(Color Clr) {
			Colors.Add(new ColorEntry() { Index = String.Length, Color = Clr });
		}

		public void ClearColors() {
			Colors.Clear();
		}

		/*public Vector2 MeasureString() {
			return Font.MeasureString(String);
		}*/

		public void Refresh() {
			if (!Dirty)
				return;
			Dirty = false;

			if (EmptyString)
				return;

			List<Vertex2> Verts = new List<Vertex2>();
			StringSize = Font.MeasureString(String, out Offset, out Vector2 StringMax);
			Color CurrentColor = Color;

			Font.GetGlyphs(String, Vector2.Zero, (GlyphIdx, Char, Glyph, Pos) => {
				if (Glyph.Bitmap == null)
					return;

				Glyph.GetUV(Font.TextureAtlas, out float U, out float V, out float W, out float H);

				foreach (var ColorIndex in Colors) {
					if (ColorIndex.Index == GlyphIdx)
						CurrentColor = ColorIndex.Color;
				}

				Verts.Add(new Vertex2(Pos, new Vector2(U, V), CurrentColor));
				Verts.Add(new Vertex2(Pos + new Vector2(Glyph.Bitmap.Width, Glyph.Bitmap.Height), new Vector2(U + W, V + H), CurrentColor));
				Verts.Add(new Vertex2(Pos + new Vector2(Glyph.Bitmap.Width, 0), new Vector2(U + W, V), CurrentColor));

				Verts.Add(new Vertex2(Pos, new Vector2(U, V), CurrentColor));
				Verts.Add(new Vertex2(Pos + new Vector2(0, Glyph.Bitmap.Height), new Vector2(U, V + H), CurrentColor));
				Verts.Add(new Vertex2(Pos + new Vector2(Glyph.Bitmap.Width, Glyph.Bitmap.Height), new Vector2(U + W, V + H), CurrentColor));
			});

			Mesh.SetVertices(Verts.ToArray());
		}

		public void Draw() {
			Refresh();

			if (EmptyString)
				return;

			ShaderUniforms.Model = Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(new Vector3(Position - Offset, 0).Round());

			DefaultShaders.DefaultTextureColor2D.Bind();
			Font.TextureAtlas.BindTextureUnit();
			Mesh.Draw();
			Font.TextureAtlas.UnbindTextureUnit();
			DefaultShaders.DefaultTextureColor2D.Unbind();
		}
	}
}
