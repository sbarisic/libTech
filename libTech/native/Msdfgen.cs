using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	public static class Msdfgen {
		const string DllName = "Msdfgen";
		const CallingConvention CConv = CallingConvention.StdCall;

		public delegate void OnGlyphLoadedFunc(int Unicode, int Width, int Height, IntPtr Pixels);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern IntPtr LoadFont(string Pth);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern void GlyphLoadedCallback(OnGlyphLoadedFunc F);

		[DllImport(DllName, CallingConvention = CConv)]
		public static extern bool LoadGlyph(IntPtr Font, int Unicode,
			int Width = 32, int Height = 32, float AngleThreshold = 3.0f, float Range = 4.0f, float Scale = 1.0f, float TranslateX = 4.0f, float TranslateY = 4.0f);
	}
}
