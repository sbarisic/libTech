using System;
using System.Collections.Generic;
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

		public byte[] GetBitmapBytes() {
			return BitmapPtr.ReadArray<byte>((uint)(Width * Height));
		}
	}

	public unsafe static class Msdfgen {
		const string DllName = "Msdfgen";
		const CallingConvention CConv = CallingConvention.Cdecl;

		
		public delegate void OnGlyphLoadedFunc(int Unicode, int Width, int Height, IntPtr Pixels);

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
		public static extern bool GetKerning(IntPtr Font, int Unicode1, int Unicode2, out double Out);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern bool LoadGlyph(IntPtr Font, int Unicode,
			int Width = 32, int Height = 32, float AngleThreshold = 3.0f, float Range = 4.0f, float Scale = 1.0f, float TranslateX = 4.0f, float TranslateY = 4.0f);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern bool LoadGlyphNormal(IntPtr Font, int Unicode, ref FontCharacter OutChar);
	}
}
