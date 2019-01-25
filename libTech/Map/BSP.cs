using FishGfx;
using FishGfx.Formats;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
//using SourceUtils;
//using SourceUtils.ValveBsp;
using libTech.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector3 = System.Numerics.Vector3;

namespace libTech.Map {
	public static class BSP {
		public static libTechModel Load(string Pth) {
			return null;

			libTechModel Model = new libTechModel();
			List<Vertex3> ModelVerts = new List<Vertex3>();
			{


			}
			Model.AddMesh(new libTechMesh(ModelVerts.ToArray(), Engine.GetMaterial("error")));
			return Model;
		}
	}
}