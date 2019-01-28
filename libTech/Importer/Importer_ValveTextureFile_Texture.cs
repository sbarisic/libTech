using FishGfx.Graphics;
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
	public unsafe class Importer_ValveTextureFile_Texture : Importer<Texture> {
		public override bool CanLoadExt(string Extension) {
			switch (Extension) {
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

			return VTF.ToTexture(new ValveTextureFile(S));
		}
	}
}
