using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using libTech.Materials;
using SourceUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace libTech.Models {
	public class libTechMesh {
		public Material Material;
		public Mesh3D Mesh;
		public Matrix4x4 MeshMatrix;
		public string Name;

		// TODO: Remove?
		Vertex3[] Vertices;

		public libTechMesh() {
			MeshMatrix = Matrix4x4.Identity;
		}

		public libTechMesh(Vertex3[] Verts, Material Material) : this() {
			this.Material = Material;
			SetVertices(Verts);
		}

		public void SetVertices(Vertex3[] Verts) {
			this.Vertices = Verts;

			if (Mesh == null)
				Mesh = new Mesh3D(Vertices);
			else
				Mesh.SetVertices(Vertices);
		}

		public Vertex3[] GetVertices() {
			return Vertices;
		}

		public void Draw() {
			Material.Bind();
			Mesh.Draw();
			Material.Unbind();
		}
	}

	public class libTechModel {
		public List<libTechMesh> Meshes;

		public Vector3 Scale;
		public Vector3 Position;
		public Matrix4x4 Rotation;
		public bool Enabled;

		public libTechModel() {
			Meshes = new List<libTechMesh>();

			Scale = new Vector3(1, 1, 1);
			Position = new Vector3(0, 0, 0);
			Rotation = Matrix4x4.Identity;
			Enabled = true;
		}

		public void AddMesh(libTechMesh Mesh) {
			Meshes.Add(Mesh);
		}

		public void CenterModel() {
			AABB BBox = CalculateAABB();
			Vector3 XYZ = BBox.Position + (BBox.Bounds / 2);

			foreach (var Mesh in Meshes) {
				Vertex3[] Verts = Mesh.GetVertices();

				for (int i = 0; i < Verts.Length; i++)
					Verts[i].Position -= XYZ;

				Mesh.SetVertices(Verts);
			}
		}

		public void ScaleToSize(float MaxSize) {
			Scale = new Vector3(MaxSize / Vector3.Distance(Vector3.Zero, CalculateAABB().Bounds));
		}

		public void DrawOpaque() {
			if (!Enabled)
				return;

			for (int i = 0; i < Meshes.Count; i++) {
				if (Meshes[i].Material.Translucent)
					continue;

				ShaderUniforms.Current.Model = (Matrix4x4.CreateScale(Scale) * Rotation * Matrix4x4.CreateTranslation(Position)) * Meshes[i].MeshMatrix;
				Meshes[i].Draw();
			}

			ShaderUniforms.Current.Model = Matrix4x4.Identity;
		}

		public void DrawTransparent() {
			if (!Enabled)
				return;

			for (int i = 0; i < Meshes.Count; i++) {
				if (!Meshes[i].Material.Translucent)
					continue;

				ShaderUniforms.Current.Model = (Matrix4x4.CreateScale(Scale) * Rotation * Matrix4x4.CreateTranslation(Position)) * Meshes[i].MeshMatrix;
				Meshes[i].Draw();
			}

			ShaderUniforms.Current.Model = Matrix4x4.Identity;
		}

		public void Draw() {
			DrawOpaque();
			DrawTransparent();
		}

		public AABB CalculateAABB() {
			if (Meshes.Count == 0)
				return AABB.Empty;

			return AABB.CalculateAABB(Meshes.SelectMany(M => M.GetVertices().Select(V => V.Position)));
		}

		public static libTechModel FromSourceMdl(SourceMdl Mdl, string ShaderOverride = null) {
			libTechModel Model = new libTechModel();
			string[] MaterialNames = Mdl.GetMaterialNames();
			string[] BodyNames = Mdl.GetBodyNames();

			// BODIES
			for (int BodyPartIdx = 0; BodyPartIdx < Mdl.Mdl.BodyPartCount; BodyPartIdx++) {
				StudioModelFile.StudioModel[] StudioModels = Mdl.Mdl.GetModels(BodyPartIdx).ToArray();

				// MODELS
				for (int ModelIdx = 0; ModelIdx < StudioModels.Length; ModelIdx++) {
					ref StudioModelFile.StudioModel StudioModel = ref StudioModels[ModelIdx];
					StudioModelFile.StudioMesh[] StudioMeshes = Mdl.Mdl.GetMeshes(ref StudioModel).ToArray();

					// MESHES
					for (int MeshIdx = 0; MeshIdx < StudioMeshes.Length; MeshIdx++) {
						ref StudioModelFile.StudioMesh StudioMesh = ref StudioMeshes[MeshIdx];

						StudioVertex[] StudioVerts = new StudioVertex[Mdl.Tris.GetVertexCount(BodyPartIdx, ModelIdx, 0, MeshIdx)];
						Mdl.Tris.GetVertices(BodyPartIdx, ModelIdx, 0, MeshIdx, StudioVerts);

						int[] Indices = new int[Mdl.Tris.GetIndexCount(BodyPartIdx, ModelIdx, 0, MeshIdx)];
						Mdl.Tris.GetIndices(BodyPartIdx, ModelIdx, 0, MeshIdx, Indices);

						List<Vertex3> Vts = new List<Vertex3>();
						for (int i = 0; i < Indices.Length; i++) {
							ref StudioVertex V = ref StudioVerts[Indices[i]];
							Vts.Add(new Vertex3(new Vector3(V.Position.X, V.Position.Y, V.Position.Z), new Vector2(V.TexCoordX, 1.0f - V.TexCoordY), Color.White));
						}

						string MatName = MaterialNames[StudioMesh.Material];
						Material Mat = Engine.GetMaterial(MatName);

						if (Mat == Engine.ErrorMaterial) {
							Mat = ValveMaterial.CreateMaterial(MatName);

							if (Mat != Engine.ErrorMaterial)
								Engine.RegisterMaterial(MatName, Mat);
						}

						libTechMesh Msh = new libTechMesh(Vts.ToArray(), Mat);
						Msh.Name = StudioModel.Name;
						Model.AddMesh(Msh);
					}
				}
			}

			return Model;
		}
	}
}
