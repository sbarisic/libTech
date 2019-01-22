using LibBSP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LBSP = LibBSP.BSP;

namespace libTech.Map {
	public static class BSP {
		public static void Load(string Pth) {
			LBSP Map = new LBSP(Pth);

			List<Vertex> Vert = Map.vertices;
		}
	}
}
