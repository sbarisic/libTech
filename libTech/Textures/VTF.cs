using FishGfx.Graphics;
using ImageMagick;
using SourceUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Textures {
	public static class VTF {
		public static Texture ToTexture(ValveTextureFile VTF, int Mip = 0, int Frame = 0, int Face = 0, int ZSlice = 0) {
			int DataLen = VTF.GetHiResPixelDataLength(Mip);
			int TotalLen = DataLen + 128;
			byte[] PixelBuffer = null;

			if (PixelBuffer == null || PixelBuffer.Length < TotalLen) {
				int PowerOfTwo = 256;

				while (PowerOfTwo < TotalLen)
					PowerOfTwo <<= 1;

				PixelBuffer = new byte[PowerOfTwo];
			}

			bool RemoveAlpha = false;
			Image Img = null;
			int Offset = 0;
			int Width = Math.Max(1, VTF.Header.Width >> Mip);
			int Height = Math.Max(1, VTF.Header.Height >> Mip);
			MagickReadSettings ReadSettings = new MagickReadSettings { Width = Width, Height = Height };

			switch (VTF.Header.HiResFormat) {
				case TextureFormat.DXT1:
				case TextureFormat.DXT3:
				case TextureFormat.DXT5: {
						if (VTF.Header.HiResFormat == TextureFormat.DXT1)
							RemoveAlpha = true;

						ReadSettings.Format = MagickFormat.Dds;
						Offset = DDS.WriteDdsHeader(VTF, Mip, PixelBuffer);
						break;
					}

				case TextureFormat.I8:
					ReadSettings.Format = MagickFormat.Gray;
					RemoveAlpha = true;
					break;

				case TextureFormat.IA88:
					ReadSettings.Format = MagickFormat.Graya;
					break;

				case TextureFormat.BGRA8888:
					ReadSettings.Format = MagickFormat.Bgra;
					break;

				case TextureFormat.BGR888:
					ReadSettings.Format = MagickFormat.Bgr;
					RemoveAlpha = true;
					break;

				default:
					throw new NotImplementedException();
			}


			if (Img == null) {
				VTF.GetHiResPixelData(Mip, Frame, Face, ZSlice, PixelBuffer, Offset);
				MagickImage MImg = new MagickImage(PixelBuffer, ReadSettings);

				if (MImg.Width != Width || MImg.Height != Height)
					MImg.Resize(new MagickGeometry(Width, Height) { IgnoreAspectRatio = true });

				Bitmap Bmp = MImg.ToBitmap();

				if (RemoveAlpha)
					Bmp.RemoveAlpha();

				Img = Bmp;
			}

			TextureWrap WrapU = TextureWrap.Repeat;
			TextureWrap WrapV = TextureWrap.Repeat;

			if ((VTF.Header.Flags & TextureFlags.CLAMPS) != 0)
				WrapU = TextureWrap.ClampToEdge;

			if ((VTF.Header.Flags & TextureFlags.CLAMPT) != 0)
				WrapV = TextureWrap.ClampToEdge;

			if ((VTF.Header.Flags & TextureFlags.CLAMPS) != 0)
				WrapU = WrapV = TextureWrap.ClampToBorder;

			Texture T = Texture.FromImage(Img);
			T.SetFilter((VTF.Header.Flags & TextureFlags.POINTSAMPLE) != 0 ? TextureFilter.Nearest : TextureFilter.Linear);
			T.SetWrap(WrapU, WrapV);
			return T;
		}
	}
}
