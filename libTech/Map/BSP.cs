using FishGfx;
using FishGfx.Formats;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using LibBSP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Map {
	public static class BSP {
		public static GenericMesh Load(string Pth) {
			LibBSP.BSP Map = new LibBSP.BSP(Pth);

			Vertex[] Vert = Map.vertices.ToArray();
			Face[] Faces = Map.faces.ToArray();


			List<Vertex3> Verts = new List<Vertex3>();
			for (int i = 0; i < Faces.Length; i++) {
				Face FF = Faces[i];

				for (int j = 0; j < FF.numVertices; j++) {
					Vertex Vtx = Vert[FF.firstVertex + j];

					Vertex3 V3 = new Vertex3(new Vector3((float)Vtx.position.x, (float)Vtx.position.y, (float)Vtx.position.z), new Vector2((float)Vtx.uv0.x, (float)Vtx.uv0.y), Vtx.color);
					Verts.Add(V3);
				}
			}

			/*for (int i = 0; i < Vert.Length; i++) {
				Vertex Vtx = Vert[i];

				Vertex3 V3 = new Vertex3(new Vector3((float)Vtx.position.x, (float)Vtx.position.y, (float)Vtx.position.z), new Vector2((float)Vtx.uv0.x, (float)Vtx.uv0.y), Vtx.color);
				Verts.Add(V3);
			}*/

			return new GenericMesh(Verts.ToArray());
		}
	}
}