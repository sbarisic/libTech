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

			internal Glyph(FontCharacter FontChar, Bitmap Bitmap) {
				FontChar.BitmapPtr = IntPtr.Zero;
				Userdata = null;

				this.FontChar = FontChar;
				this.Bitmap = Bitmap;
			}

		}

		IntPtr Fnt;

		/*public int CharWidth { get; private set; }
		public int CharHeight { get; private set; }
		public float AngleThreshold { get; private set; }
		public float Range { get; private set; }
		public float Scale { get; private set; }
		public float OffsetX { get; private set; }
		public float OffsetY { get; private set; }

		Dictionary<int, Bitmap> GlyphMsdfTexture;*/
		Dictionary<uint, Glyph> GlyphTexture;

		int _FontSize;
		public int FontSize {
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

		public FreetypeFont(string Pth, int Size = 12) : this(File.ReadAllBytes(Pth), Size) {
		}

		public FreetypeFont(byte[] FontFile, int Size = 12) {
			//GlyphMsdfTexture = new Dictionary<int, Bitmap>();
			GlyphTexture = new Dictionary<uint, Glyph>();

			/*CharWidth = 32;
			CharHeight = 36;

			AngleThreshold = 3;
			Range = 4;
			Scale = 1;
			OffsetX = 6;
			OffsetY = 10;*/

			Fnt = Msdfgen.LoadFontMemory(FontFile);
			FontSize = Size;
			//LoadGlyphRange(33, 126);
		}

		/*public Bitmap GetGlyphImage(char C, bool Load = true) {
			int Unicode = C;

			if (!GlyphMsdfTexture.ContainsKey(Unicode))
				if (Load)
					LoadGlyph(Unicode);
				else
					return null;

			if (GlyphMsdfTexture.ContainsKey(Unicode))
				return GlyphMsdfTexture[Unicode];
			return null;
		}*/

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

				GlyphTexture.Add(Unicode, new Glyph(FChar, FChar.GetBitmap()));
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

		/*public void GetMsdfGlyphsForString(string Str, OnGlyphAction OnGlyph) {
			float X = -OffsetX;
			float Y = 0;

			for (int i = 0; i < Str.Length; i++) {
				OnGlyph(GetGlyphImage(Str[i]), X, Y);
				X += CharWidth - OffsetX - Range; // TODO: Is that right? I don't know.
			}
		}

		public void LoadGlyphRange(int UnicodeMin, int UnicodeMax, Func<int, bool> IsValid) {
			lock (FontLock) {
				Msdfgen.GlyphLoadedCallback(OnGlyph);

				for (int i = UnicodeMin; i < UnicodeMax + 1; i++)
					if (IsValid(i))
						Msdfgen.LoadGlyph(Fnt, i, CharWidth, CharHeight, AngleThreshold, Range, Scale, OffsetX, OffsetY);
			}
		}

		public void LoadGlyphRange(int UnicodeMin, int UnicodeMax) {
			LoadGlyphRange(UnicodeMin, UnicodeMax, ValidGlyph);
		}

		public void LoadGlyph(int Unicode) {
			LoadGlyphRange(Unicode, Unicode);
		}

		bool ValidGlyph(int Unicode) {
			return !(char.IsControl((char)Unicode) || char.IsWhiteSpace((char)Unicode));
		}

		void OnGlyph(int Unicode, int Width, int Height, IntPtr Data) {
			using (Bitmap Bmp = new Bitmap(Width, Height)) {
				FloatRGB* DataPtr = (FloatRGB*)Data;

				for (int i = 0; i < Width * Height; i++) {
					FloatRGB Clr = DataPtr[i];

					int R = (int)(Clr.R * 256).Clamp(0, 255);
					int G = (int)(Clr.G * 256).Clamp(0, 255);
					int B = (int)(Clr.B * 256).Clamp(0, 255);
					Bmp.SetPixel(i % Width, i / Width, Color.FromArgb(255, R, G, B));
				}

				Bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
				//Bmp.Save("TEMP/" + (char)Unicode + ".png");

				char C = (char)Unicode;
				if (!GlyphMsdfTexture.ContainsKey(C)) {
					//Texture Tex = Texture.FromImage(Bmp);
					//Tex.SetFilterSmooth();
					//Tex.SetFilter(Gl.LINEAR, Gl.LINEAR);

					GlyphMsdfTexture.Add(C, BmpToTex(Bmp));
				}
			}
		}

		Texture BmpToTex(Bitmap Bmp) {
			return (Texture)Bmp.Clone();
		}*/

		uint ToUnicode(char C) {
			return C;
		}
	}
}
