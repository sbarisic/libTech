using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Numerics;

//using Texture = System.Drawing.Bitmap;
using System.Drawing.Imaging;
using FishGfx.Graphics;

// TODO: Port to FishGfx

namespace libTech {
	[StructLayout(LayoutKind.Sequential)]
	struct FloatRGB {
		public float R, G, B;
	}

	//public delegate void OnGlyphAction(Bitmap Img, float X, float Y);
	public delegate void OnGlyphAction(uint Unicode, FreetypeFont.Glyph G, Vector2 Position);


	public unsafe class FreetypeFont {
		public struct Glyph {
			FontCharacter FontChar;

			public Bitmap Bitmap;
			public object Userdata;

			public int X;
			public int Y;

			public Vector2 Size {
				get {
					return new Vector2(FontChar.Width, FontChar.Height);
				}
			}

			public Vector2 Advance {
				get {
					return new Vector2(FontChar.AdvanceX, FontChar.AdvanceY) / 64;
				}
			}

			public Vector2 Bearing {
				get {
					return new Vector2(FontChar.BearingX, FontChar.BearingY);
				}
			}

			public void GetUV(float RootW, float RootH, out float U, out float V, out float W, out float H) {
				W = Bitmap.Width / RootW;
				H = Bitmap.Height / RootH;
				U = X / RootW;
				V = Y / RootH;
			}

			public void GetUV(Texture Root, out float U, out float V, out float W, out float H) {
				GetUV(Root.Width, Root.Height, out U, out V, out W, out H);
			}

			internal Glyph(FontCharacter FontChar, Bitmap Bitmap) {
				FontChar.BitmapPtr = IntPtr.Zero;
				Userdata = null;

				X = 0;
				Y = 0;
				this.FontChar = FontChar;
				this.Bitmap = Bitmap;
			}

		}

		IntPtr Fnt;
		Dictionary<uint, Glyph> GlyphTexture;
		RectanglePacker Packer;

		public Texture TextureAtlas;

		public int LineHeight {
			get {
				return FontSize;
			}
		}

		int _FontSize;
		int FontSize {
			get {
				return _FontSize;
			}

			set {
				if (_FontSize != value) {
					_FontSize = value;
					Msdfgen.SetFontSize(Fnt, 0, _FontSize);
				}
			}
		}

		public FreetypeFont(string Pth, int Size = 12) : this(File.ReadAllBytes(Pth), Size, Size * 16, Size * 16) {
		}

		public FreetypeFont(byte[] FontFile, int Size = 12, int AtlasW = 512, int AtlasH = 512) {
			GlyphTexture = new Dictionary<uint, Glyph>();
			Fnt = Msdfgen.LoadFontMemory(FontFile);
			FontSize = Size;

			TextureAtlas = Texture.Empty(AtlasW, AtlasH);
			Packer = new RectanglePacker(AtlasW, AtlasH);
		}

		public double GetKerning(char CharA, char CharB) {
			return GetKerning(ToUnicode(CharA), ToUnicode(CharB));
		}

		public double GetKerning(uint UnicodeA, uint UnicodeB) {
			if (Msdfgen.GetKerning(Fnt, UnicodeA, UnicodeB, out double K))
				return K;

			return 0;
		}

		public Glyph? GetGlyph(char C) {
			return GetGlyph(ToUnicode(C));
		}

		public Glyph? GetGlyph(uint Unicode) {
			if (GlyphTexture.ContainsKey(Unicode))
				return GlyphTexture[Unicode];

			FontCharacter FChar = new FontCharacter();
			if (Msdfgen.LoadGlyphNormal(Fnt, Unicode, ref FChar)) {
				if (FChar.Width == 0 || FChar.Height == 0) {
					if (!GlyphTexture.ContainsKey(Unicode))
						GlyphTexture.Add(Unicode, new Glyph(FChar, null));

					return GetGlyph(Unicode);
				}

				Glyph G = new Glyph(FChar, FChar.GetBitmap());
				if (!Packer.Pack(G.Bitmap.Width, G.Bitmap.Height, out G.X, out G.Y))
					throw new NotImplementedException("Cannot pack glyph");

				G.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				TextureAtlas.SubRect2D(G.Bitmap, G.X, G.Y);
				G.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

				GlyphTexture.Add(Unicode, G);
				return GetGlyph(Unicode);
			}

			return null;
		}

		public void GetGlyphs(string Str, Vector2 StartPos, OnGlyphAction OnGlyph) {
			uint[] Unicodes = Str.ToUTF8CodePoints();

			foreach (var Unicode in Unicodes) {
				Glyph G = GetGlyph(Unicode).Value;
				OnGlyph(Unicode, G, StartPos + new Vector2(G.Bearing.X, -(G.Size.Y - G.Bearing.Y)));
				StartPos += G.Advance;
				// TODO: Kerning value?	
			}
		}

		public Vector2 MeasureString(string Str) {
			Vector2 Max = Vector2.Zero;
			Vector2 StartPos = Vector2.Zero;

			for (int i = 0; i < Str.Length; i++) {
				Glyph G = GetGlyph(Str[i]).Value;
				Max = Max.Max(StartPos + new Vector2(G.Bearing.X, -(G.Size.Y - G.Bearing.Y)) + G.Size);
				StartPos += G.Advance;
			}

			return Max;
		}

		uint ToUnicode(char C) {
			return C;
		}
	}
}
