using libTech.Models;
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
	public unsafe class Importer_SourceMdl_LibTechModel : Importer<libTechModel> {
		public override bool CanLoadExt(string Extension) {
			switch (Extension) {
				case ".mdl":
					return true;

				default:
					return false;
			}
		}

		public override libTechModel Load(string FilePath) {
			SourceMdl Mdl = SourceMdl.FromFile(FilePath, Engine.VFS);

			if (Mdl == null)
				return null;

			return libTechModel.FromSourceMdl(Mdl);
		}
	}
}
