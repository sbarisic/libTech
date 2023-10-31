using FishGfx.Graphics;

using ImageMagick;

using libTech.Models;
using libTech.Textures;

using SourceUtils;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Importer {
	public unsafe class Importer_Texture : Importer<Texture> {
		public override bool CanLoadExt(string Extension) {
			switch (Extension) {
				case ".bmp":
				case ".gif":
				case ".exif":
				case ".jpg":
				case ".png":
				case ".tiff":
				case ".tga":
				case ".vtf":
					return true;

				default:
					return false;
			}
		}

		public override Texture Load(string FilePath) {
			Stream S = Engine.VFS.OpenFile(FilePath);

			if (S == null)
				throw new NotImplementedException("Wut?");

			switch (Path.GetExtension(FilePath)) {
				case ".vtf":
					return VTF.ToTexture(new ValveTextureFile(S));

				case ".tga": {
						MagickReadSettings RS = new MagickReadSettings();
						RS.Format = MagickFormat.Tga;

						using (MagickImage Img = new MagickImage(S, RS)) {
							return Texture.FromImage(Img.ToBitmap());
						}

					}

				default:
					return Texture.FromImage(Image.FromStream(S));
			}
		}
	}
}