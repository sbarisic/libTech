using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

using GLPixelFormat = OpenGL.PixelFormat;
using IPixFormat = System.Drawing.Imaging.PixelFormat;

namespace libTech.Graphics {
	public class Texture : GraphicsObject {
		public Texture(int W, int H, TextureTarget Target = TextureTarget.Texture2d) {
			ID = Gl.CreateTexture(Target);

			SetWrap();
			SetFilter();
			Storage2D(W, H);
		}

		public Texture(Image FromImage) : this(FromImage.Width, FromImage.Height) {
			SubImage2D(FromImage);
		}

		public void TextureParam(TextureParameterName ParamName, int Val) {
			Gl.TextureParameter(ID, ParamName, Val);
		}

		public void SetWrap(int Val = Gl.CLAMP_TO_EDGE) {
			TextureParam(TextureParameterName.TextureWrapS, Val);
			TextureParam(TextureParameterName.TextureWrapT, Val);
			TextureParam(TextureParameterName.TextureWrapR, Val);
		}

		public void SetFilter(int Min = Gl.NEAREST, int Mag = Gl.NEAREST) {
			TextureParam(TextureParameterName.TextureMinFilter, Min);
			TextureParam(TextureParameterName.TextureMagFilter, Mag);
		}

		public void Storage2D(int W, int H, int Levels = 1, InternalFormat IntFormat = InternalFormat.Rgba8) {
			Gl.TextureStorage2D(ID, Levels, IntFormat, W, H);
		}

		public void SubImage2D(IntPtr Pixels, int X, int Y, int W, int H, GLPixelFormat PFormat = GLPixelFormat.Rgba, PixelType PType = PixelType.UnsignedByte, int Level = 0) {
			Gl.TextureSubImage2D(ID, Level, X, Y, W, H, PFormat, PType, Pixels);
		}

		public void SubImage2D(Image Img, int X = 0, int Y = 0, int W = -1, int H = -1) {
			if (W == -1 || H == -1) {
				W = Img.Width;
				H = Img.Height;
			}

			using (Bitmap Bmp = new Bitmap(Img)) {
				Bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

				BitmapData Data = Bmp.LockBits(new Rectangle(X, Y, W, H), ImageLockMode.ReadOnly, IPixFormat.Format32bppArgb);
				SubImage2D(Data.Scan0, X, Y, W, H, GLPixelFormat.Bgra);
				Bmp.UnlockBits(Data);
			}
		}

		public void BindTextureUnit(uint Unit = 0) {
			Gl.BindTextureUnit(Unit, ID);
		}

		public void UnbindTextureUnit(uint Unit = 0) {
			Gl.BindTextureUnit(Unit, 0);
		}

		public void GenerateMipmap() {
			Gl.GenerateTextureMipmap(ID);
		}

		public override void GraphicsDispose() {
			Gl.DeleteTextures(new uint[] { ID });
		}
	}
}
