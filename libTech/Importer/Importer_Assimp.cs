using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libTech.Graphics;
using System.Drawing;
using System.IO;
using System.Numerics;
using Assimp;
using Assimp.Configs;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Mesh = libTech.Graphics.Mesh;
using Material = libTech.Graphics.Material;

using AssimpMesh = Assimp.Mesh;
using AssimpMaterial = Assimp.Material;

namespace libTech.Importer {
	public unsafe class Importer_Assimp : Importer<Model> {
		AssimpContext AssimpCtx;

		public Importer_Assimp() {
			AssimpCtx = new AssimpContext();
			AssimpCtx.SetConfig(new FBXImportLightsConfig(false));
		}

		public override bool CanLoadExt(string Extension) {
			bool Can = AssimpCtx.IsImportFormatSupported(Extension);
			return Can;
		}

		Mesh ConvertMesh(AssimpMesh M, Material[] Materials) {
			Mesh Msh = new Mesh();

			Vector3[] Verts = M.Vertices.Select((V) => new Vector3(V.X, V.Y, V.Z)).ToArray();
			Msh.SetVertices(Verts);

			Vector2[] UVs = M.TextureCoordinateChannels[0].Select((V) => new Vector2(V.X, V.Y)).ToArray();
			if (UVs.Length > 0)
				Msh.SetUVs(UVs);

			Vector4[] Colors = M.VertexColorChannels[0].Select((C) => new Vector4(C.R, C.G, C.B, C.A)).ToArray();
			if (Colors.Length > 0)
				Msh.SetColors(Colors);

			uint[] Indices = M.GetUnsignedIndices();
			if (Indices.Length > 0)
				Msh.SetElements(Indices);

			if (Materials != null)
				Msh.Material = Materials[M.MaterialIndex];

			return Msh;
		}

		Material ConvertMaterial(AssimpMaterial M) {
			Material Mat = new Material();

			if (M.HasColorDiffuse)
				Mat.DiffuseColor = new Vector4(M.ColorDiffuse.R, M.ColorDiffuse.G, M.ColorDiffuse.B, M.ColorDiffuse.A);

			if (M.HasTextureDiffuse) {
				string TexName = "content/textures/" + Path.GetFileNameWithoutExtension(M.TextureDiffuse.FilePath) + ".png";
				if (File.Exists(TexName))
					Mat.Diffuse = new Texture(Image.FromFile(TexName));
			}

			return Mat;
		}

		public override Model Load(string FilePath) {
			Model Mdl = new Model();

			Scene S = AssimpCtx.ImportFile(FilePath);

			Material[] Materials = null;
			if (S.HasMaterials)
				Materials = S.Materials.Select((M) => ConvertMaterial(M)).ToArray();

			foreach (var AssimpMsh in S.Meshes)
				Mdl.AddMesh(ConvertMesh(AssimpMsh, Materials));

			return Mdl;
		}
	}
}
