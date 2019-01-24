using ImageMagick;
using SourceUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	internal static class DDS {
		[Flags]
		public enum DdsHeaderFlags : uint {
			CAPS = 0x1,
			HEIGHT = 0x2,
			WIDTH = 0x4,
			PITCH = 0x8,
			PIXELFORMAT = 0x1000,
			MIPMAPCOUNT = 0x20000,
			LINEARSIZE = 0x80000,
			DEPTH = 0x800000
		}

		[Flags]
		public enum DdsCaps : uint {
			COMPLEX = 0x8,
			MIPMAP = 0x400000,
			TEXTURE = 0x1000
		}

		[Flags]
		public enum DdsPixelFormatFlags {
			ALPHAPIXELS = 0x1,
			ALPHA = 0x2,
			FOURCC = 0x4,
			RGB = 0x40,
			YUV = 0x200,
			LUMINANCE = 0x20000
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct DdsPixelFormat {
			public uint dwSize;
			public DdsPixelFormatFlags dwFlags;
			public uint dwFourCC;
			public uint dwRGBBitCount;
			public uint dwRBitMask;
			public uint dwGBitMask;
			public uint dwBBitMask;
			public uint dwABitMask;
		}

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct DdsHeader {
			public uint dwSize;
			public DdsHeaderFlags dwFlags;
			public uint dwHeight;
			public uint dwWidth;
			public uint dwPitchOrLinearSize;
			public uint dwDepth;
			public uint dwMipMapCount;
			public fixed uint dwReserved1[11];
			public DdsPixelFormat ddspf;
			public DdsCaps dwCaps;
			public uint dwCaps2;
			public uint dwCaps3;
			public uint dwCaps4;
			public uint dwReserved2;
		}

		public static unsafe int WriteDdsHeader(ValveTextureFile VTF, int Mip, byte[] Buffer) {
			DdsHeader Header = new DdsHeader();

			int BlockSize;
			uint FourCC;

			switch (VTF.Header.HiResFormat) {
				case TextureFormat.DXT1:
					BlockSize = 8;
					FourCC = 0x31545844;
					break;
				case TextureFormat.DXT3:
					BlockSize = 16;
					FourCC = 0x33545844;
					break;
				case TextureFormat.DXT5:
					BlockSize = 16;
					FourCC = 0x35545844;
					break;
				default:
					throw new NotImplementedException();
			}

			Header.dwWidth = (uint)Math.Max(1, VTF.Header.Width >> Mip);
			Header.dwHeight = (uint)Math.Max(1, VTF.Header.Height >> Mip);

			Header.dwSize = (uint)Marshal.SizeOf(typeof(DdsHeader));
			Header.dwFlags = DdsHeaderFlags.CAPS | DdsHeaderFlags.HEIGHT | DdsHeaderFlags.WIDTH | DdsHeaderFlags.PIXELFORMAT;
			Header.dwPitchOrLinearSize = (uint)(Math.Max(1, (VTF.Header.Width + 3) / 4) * BlockSize);
			Header.dwDepth = 1;
			Header.dwMipMapCount = 1;
			Header.dwCaps = DdsCaps.TEXTURE;
			Header.ddspf.dwSize = (uint)Marshal.SizeOf(typeof(DdsPixelFormat));
			Header.ddspf.dwFlags = DdsPixelFormatFlags.FOURCC;
			Header.ddspf.dwFourCC = FourCC;

			fixed (byte* bufferPtr = Buffer) {
				uint* MagicPtr = (uint*)bufferPtr;
				DdsHeader* HeaderPtr = (DdsHeader*)(bufferPtr + sizeof(uint));

				*MagicPtr = 0x20534444;
				*HeaderPtr = Header;
			}

			return (int)Header.dwSize + sizeof(uint);
		}
	}
}
