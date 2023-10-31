using FishGfx;
using FishGfx.Formats;
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

		public libTechMesh(Material Material) : this() {
			this.Material = Material;
		}

		public libTechMesh(Vertex3[] Verts, Material Material) : this(Material) {
			SetVertices(Verts);
		}

		public libTechMesh(libTechMesh Clone) : this(Clone.Vertices, Clone.Material) {
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

		public void SetLabel(string Label) {
			Mesh.VAO.SetLabel(OpenGL.ObjectIdentifier.VertexArray, Label);
		}

		public void SetWireframe(bool Wireframe) {
			if (Wireframe)
				Mesh.PolygonMode = PolygonMode.Line;
			else
				Mesh.PolygonMode = PolygonMode.Fill;
		}

		public void Draw() {
			Material.DrawMesh(Mesh);
		}
	}

	public class libTechModel {
		public Vector3 Scale;
		public Vector3 Position;
		public Quaternion Rotation;
		public bool Enabled;

		public Dictionary<string, libTechBone> Bones;

		public BoundSphere BoundingSphere {
			get; private set;
		}
		public AABB BoundingBox {
			get; private set;
		}

		List<libTechMesh> Meshes;

		public libTechModel() {
			Bones = new Dictionary<string, libTechBone>();
			Meshes = new List<libTechMesh>();

			Scale = new Vector3(1, 1, 1);
			Position = new Vector3(0, 0, 0);
			Rotation = Quaternion.Identity;
			Enabled = true;
		}

		public libTechModel(libTechModel Clone) : this() {
			foreach (var B in Clone.Bones)
				Bones.Add(B.Key, B.Value);

			foreach (var M in Clone.GetMeshes())
				AddMesh(new libTechMesh(M));

			Scale = Clone.Scale;
			Position = Clone.Position;
			Rotation = Clone.Rotation;
			Enabled = true;
		}

		public libTechModel(GenericMesh[] Meshes, Material Mat) : this() {
			for (int i = 0; i < Meshes.Length; i++) {
				libTechMesh Msh = new libTechMesh(Meshes[i].Vertices.ToArray(), Mat);
				AddMesh(Msh);
			}
		}

		void CalcBounds() {
			if (Meshes.Count == 0) {
				BoundingBox = AABB.Empty;
				BoundingSphere = BoundSphere.Empty;
			}

			BoundingBox = AABB.CalculateAABB(Meshes.SelectMany(M => M.GetVertices().Select(V => V.Position)));
			BoundingSphere = BoundSphere.FromAABB(BoundingBox);
		}

		public void AddMesh(libTechMesh Mesh) {
			Meshes.Add(Mesh);
			CalcBounds();
		}

		public void RemoveMesh(libTechMesh Mesh) {
			Meshes.Remove(Mesh);
			CalcBounds();
		}

		public IEnumerable<libTechMesh> GetMeshes() {
			return Meshes.ToArray();
		}

		public void CenterModel() {
			CalcBounds();
			Vector3 XYZ = BoundingBox.Position + (BoundingBox.Bounds / 2);

			foreach (var Mesh in Meshes) {
				Vertex3[] Verts = Mesh.GetVertices();

				for (int i = 0; i < Verts.Length; i++)
					Verts[i].Position -= XYZ;

				Mesh.SetVertices(Verts);
			}

			CalcBounds();
		}

		public void SetWireframe(bool Wireframe) {
			foreach (var Msh in GetMeshes())
				Msh.SetWireframe(Wireframe);
		}

		public libTechBone? GetBone(string Name) {
			if (Bones.ContainsKey(Name))
				return Bones[Name];

			return null;
		}

		public void ScaleToSize(float MaxSize) {
			Scale = new Vector3(MaxSize / Vector3.Distance(Vector3.Zero, BoundingBox.Bounds));
		}

		public void SetLabel(string Label) {
			for (int i = 0; i < Meshes.Count; i++) {
				if (Meshes[i].Material.Translucent)
					Meshes[i].SetLabel(string.Format("{0} Transparent", Label));
				else
					Meshes[i].SetLabel(string.Format("{0} Opaque", Label));
			}
		}

		public void DrawOpaque() {
			if (!Enabled)
				return;

			for (int i = 0; i < Meshes.Count; i++) {
				if (Meshes[i].Material.Translucent)
					continue;

				ShaderUniforms.Current.Model = (Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position)) * Meshes[i].MeshMatrix;
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

				ShaderUniforms.Current.Model = (Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position)) * Meshes[i].MeshMatrix;
				Meshes[i].Draw();
			}

			ShaderUniforms.Current.Model = Matrix4x4.Identity;
		}

		public void DrawShadowVolume(Material ShadowVolumeMat) {
			for (int i = 0; i < Meshes.Count; i++) {
				if (Meshes[i].Material.Translucent)
					continue;

				ShaderUniforms.Current.Model = (Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position)) * Meshes[i].MeshMatrix;

				Material Old = Meshes[i].Material;
				Meshes[i].Material = ShadowVolumeMat;
				Meshes[i].Draw();
				Meshes[i].Material = Old;
			}
		}

		public static libTechModel FromSourceMdl(SourceMdl Mdl, string ShaderOverride = null) {
			libTechModel Model = new libTechModel();
			string[] MaterialNames = Mdl.GetMaterialNames();
			string[] BodyNames = Mdl.GetBodyNames();

			StudioModelFile.StudioBone[] Bones = Mdl.Mdl.GetBones();
			string[] BoneNames = Mdl.Mdl.GetBoneNames();

			for (int i = 0; i < Bones.Length; i++)
				Model.Bones.Add(BoneNames[i], new libTechBone(BoneNames[i], Bones[i]));


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

						if (Mat == Engine.GetMaterial("error")) {
							Mat = ValveMaterial.CreateMaterial(MatName);

							if (Mat != Engine.GetMaterial("error"))
								Engine.RegisterMaterial(Mat);
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
