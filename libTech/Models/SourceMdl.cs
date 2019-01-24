using FishGfx;
using FishGfx.Formats;
using FishGfx.Graphics;
using libTech.Textures;
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
	public unsafe class SourceMdl {
		public StudioModelFile Mdl;
		public ValveVertexFile Verts;
		public ValveTriangleFile Tris;

		/*public GenericMesh Load(string Pth) {
			List<Vertex3> Verts = new List<Vertex3>();
			StudioVertex[] Vtx = LoadMdl(Pth, Engine.VFS).ToArray()[0];

			for (int i = 0; i < Vtx.Length; i++) {
				ref StudioVertex V = ref Vtx[i];
				Verts.Add(new Vertex3(new Vector3(V.Position.X, V.Position.Y, V.Position.Z), new Vector2(V.TexCoordX, 1.0f - V.TexCoordY), Color.White));
			}

			return new GenericMesh(Verts.ToArray());
		}*/

		public Dictionary<string, Texture> GetTextures() {
			Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();

			foreach (var TexName in GetMaterialNames())
				Textures.Add(TexName, Engine.Load<Texture>(TexName));

			return Textures;
		}

		public string[] GetMaterialNames() {
			string[] MatNames = new string[Mdl.MaterialCount];

			for (int i = 0; i < MatNames.Length; i++)
				MatNames[i] = Mdl.GetMaterialName(i, Engine.VFS);

			return MatNames;
		}

		public string[] GetBodyNames() {
			string[] ModNames = new string[Mdl.BodyPartCount];

			for (int i = 0; i < ModNames.Length; i++)
				ModNames[i] = Mdl.GetBodyPartName(i);


			return ModNames;
		}

		IEnumerable<StudioVertex[]> LoadMdl(IResourceProvider Res) {
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

		public static SourceMdl FromFile(string FilePath, IResourceProvider Res) {
			FilePath = FilePath.Substring(0, FilePath.Length - Path.GetExtension(FilePath).Length);

			SourceMdl Model = new SourceMdl();
			Model.Mdl = StudioModelFile.FromProvider(FilePath + ".mdl", Res);
			Model.Verts = ValveVertexFile.FromProvider(FilePath + ".vvd", Res);
			Model.Tris = ValveTriangleFile.FromProvider(FilePath + ".dx90.vtx", Model.Mdl, Model.Verts, Res);

			return Model;
		}
	}
}