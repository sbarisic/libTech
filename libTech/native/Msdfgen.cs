using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FontCharacter {
		public IntPtr BitmapPtr;

		public int Width;
		public int Height;

		public int BearingX;
		public int BearingY;

		public int AdvanceX;
		public int AdvanceY;

		public int PixelMode;

		public byte[] GetBitmapBytes() {
			int BytesPerPixel = 0;

			if (PixelMode == 2) // Grey
				BytesPerPixel = 1;
			else if (PixelMode == 7) // BGRA
				BytesPerPixel = 4;
			else
				throw new NotImplementedException();

			return BitmapPtr.ReadArray<byte>((uint)(Width * Height * BytesPerPixel));
		}

		public Bitmap GetBitmap() {
			byte[] Data = GetBitmapBytes();
			Bitmap Bmp = new Bitmap(Width, Height);

			for (int Y = 0; Y < Height; Y++)
				for (int X = 0; X < Width; X++) {
					int Idx = Y * Width + X;

					if (PixelMode == 2)
						Bmp.SetPixel(X, Y, Color.FromArgb(Data[Idx], 255, 255, 255));
					else if (PixelMode == 7)
						Bmp.SetPixel(X, Y, Color.FromArgb(Data[Idx], Data[Idx + 1], Data[Idx + 2], Data[Idx + 3]));
					else
						throw new NotImplementedException();
				}

			Bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
			return Bmp;
		}
	}

	public unsafe static class Msdfgen {
		const string DllName = "Msdfgen";
		const CallingConvention CConv = CallingConvention.Cdecl;
		
		public delegate void OnGlyphLoadedFunc(uint Unicode, int Width, int Height, IntPtr Pixels);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern IntPtr LoadFont(string Pth);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern IntPtr LoadFontMemory(IntPtr Memory, int Length);

		public static IntPtr LoadFontMemory(byte[] Bytes) {
			fixed (byte* BytesPtr = Bytes) {
				return LoadFontMemory(new IntPtr(BytesPtr), Bytes.Length);
			}
		}

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern bool SetFontSize(IntPtr Font, int W, int H);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void DestroyFont(IntPtr Fnt);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GlyphLoadedCallback(OnGlyphLoadedFunc F);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern bool GetKerning(IntPtr Font, uint Unicode1, uint Unicode2, out double Out);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern bool LoadGlyph(IntPtr Font, uint Unicode,
			int Width = 32, int Height = 32, float AngleThreshold = 3.0f, float Range = 4.0f, float Scale = 1.0f, float TranslateX = 4.0f, float TranslateY = 4.0f);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern bool LoadGlyphNormal(IntPtr Font, uint Unicode, ref FontCharacter OutChar);
	}
}
