using FishGfx.Graphics;
using libTech.Materials;
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
	public unsafe class Importer_ValveMaterialType_Material : Importer<Material> {
		public override bool CanLoadExt(string Extension) {
			switch (Extension) {
				case ".vmt":
					return true;

				default:
					return false;
			}
		}

		public override Material Load(string FilePath) {
			Stream S = Engine.VFS.OpenFile(FilePath);
			if (S == null)
				return null;

			return ValveMaterial.CreateMaterial(S, FilePath);
		}
	}
}
