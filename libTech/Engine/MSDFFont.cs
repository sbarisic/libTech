using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenGL;
using libTech.Graphics;
using System.Runtime.InteropServices;

namespace libTech {
	[StructLayout(LayoutKind.Sequential)]
	struct FloatRGB {
		public float R, G, B;
	}

	public unsafe class MSDFFont {
		static object FontLock = new object();
		IntPtr Fnt;

		public int CharWidth { get; private set; }
		public int CharHeight { get; private set; }
		public float AngleThreshold { get; private set; }
		public float Range { get; private set; }
		public float Scale { get; private set; }
		public float OffsetX { get; private set; }
		public float OffsetY { get; private set; }

		Dictionary<char, Texture> GlyphTexture;

		public MSDFFont(string Pth) {
			GlyphTexture = new Dictionary<char, Texture>();

			CharWidth = 32;
			CharHeight = 36;

			AngleThreshold = 3;
			Range = 4;
			Scale = 1;
			OffsetX = 6;
			OffsetY = 10;

			Pth = Path.GetFullPath(Pth).NormalizeFilePath();
			if (!File.Exists(Pth))
				throw new Exception("File does not exist " + Pth);

			string Ext = Path.GetExtension(Pth).ToLower();
			switch (Ext) {
				case ".ttf":
					lock (FontLock) {

						Fnt = Msdfgen.LoadFont(Pth);
					}
					break;

				default:
					throw new Exception("Unnown font file format " + Ext);
			}

			LoadGlyphRange('A', 'Z');
			LoadGlyphRange('a', 'z');
			LoadGlyphRange('0', '9');
		}

		public Texture GetGlyphTexture(char C, bool Load = true) {
			if (!GlyphTexture.ContainsKey(C))
				if (Load)
					LoadGlyph(C);
				else
					return null;

			if (GlyphTexture.ContainsKey(C))
				return GlyphTexture[C];
			return null;
		}

		public void GetGlyphsForString(string Str, Action<Texture, float, float> A) {
			float X = -OffsetX;
			float Y = 0;

			for (int i = 0; i < Str.Length; i++) {
				A(GetGlyphTexture(Str[i]), X, Y);
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
				if (!GlyphTexture.ContainsKey(C)) {
					Texture Tex = Texture.FromImage(Bmp);
					Tex.SetFilter(Gl.LINEAR, Gl.LINEAR);
					GlyphTexture.Add(C, Tex);
				}
			}
		}
	}
}
