using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	public static class MagickImg {
		public static Bitmap ToImage(Stream S) {
			MagickReadSettings RS = new MagickReadSettings();
			RS.Format = MagickFormat.Tga;

			using (MagickImage Img = new MagickImage(S, RS)) {
				return Img.ToBitmap();
			}
		}
	}
}
