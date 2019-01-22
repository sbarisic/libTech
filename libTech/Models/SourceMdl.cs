using FishGfx;
using FishGfx.Formats;
using SourceUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace libTech.Models {
	public static class SourceMdl {
		public static GenericMesh Load(string Pth) {
			List<Vertex3> Verts = new List<Vertex3>();
			StudioVertex[] Vtx = LoadMdl(Pth, Engine.VFS).ToArray()[0];

			for (int i = 0; i < Vtx.Length; i++) {
				ref StudioVertex V = ref Vtx[i];
				Verts.Add(new Vertex3(new Vector3(V.Position.X, V.Position.Y, V.Position.Z), new Vector2(V.TexCoordX, 1.0f - V.TexCoordY), Color.White));
			}

			return new GenericMesh(Verts.ToArray());
		}


		static IEnumerable<StudioVertex[]> LoadMdl(string FilePath, IResourceProvider Res) {
			FilePath = FilePath.Substring(0, FilePath.Length - Path.GetExtension(FilePath).Length);

			StudioModelFile Mdl = StudioModelFile.FromProvider(FilePath + ".mdl", Res);
			ValveVertexFile Verts = ValveVertexFile.FromProvider(FilePath + ".vvd", Res);
			ValveTriangleFile Tris = ValveTriangleFile.FromProvider(FilePath + ".dx90.vtx", Mdl, Verts, Res);

			for (int BodyPartIdx = 0; BodyPartIdx < Mdl.BodyPartCount; BodyPartIdx++) {
				StudioModelFile.StudioModel[] Models = Mdl.GetModels(BodyPartIdx).ToArray();

				for (int ModelIndex = 0; ModelIndex < Models.Length; ModelIndex++) {
					StudioModelFile.StudioModel Model = Models[ModelIndex];
					StudioModelFile.StudioMesh[] Meshes = Mdl.GetMeshes(ref Model).ToArray();

					for (int MeshIndex = 0; MeshIndex < Meshes.Length; MeshIndex++) {
						StudioModelFile.StudioMesh Mesh = Meshes[MeshIndex];

						StudioVertex[] StudioVerts = new StudioVertex[Tris.GetVertexCount(BodyPartIdx, ModelIndex, 0, MeshIndex)];
						Tris.GetVertices(BodyPartIdx, ModelIndex, 0, MeshIndex, StudioVerts);

						int[] Indices = new int[Tris.GetIndexCount(BodyPartIdx, ModelIndex, 0, MeshIndex)];
						Tris.GetIndices(BodyPartIdx, ModelIndex, 0, MeshIndex, Indices);

						List<StudioVertex> Vts = new List<StudioVertex>();
						for (int i = 0; i < Indices.Length; i++)
							Vts.Add(StudioVerts[Indices[i]]);


						yield return Vts.ToArray();
					}
				}
			}
		}
	}
}