using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Quaternion = System.Numerics.Quaternion;
using OpenGL;

namespace libTech.Graphics {
	public class Model {
		List<Mesh> MeshList;

		public Vector3 Position;
		public Vector3 Scale;
		public Quaternion Rotation;

		public Mesh[] Meshes {
			get {
				return MeshList.ToArray();
			}
		}

		public Model() {
			MeshList = new List<Mesh>();

			Position = Vector3.Zero;
			Scale = Vector3.One;
			Rotation = Quaternion.Identity;
		}

		public void AddMesh(Mesh M) {
			MeshList.Add(M);
		}

		public void Draw() {
			foreach (var Msh in MeshList)
				if (!Msh.Material.IsTransparent)
					Msh.Draw(Position, Scale, Rotation);
		}

		public void DrawTransparent() {
			foreach (var Msh in MeshList)
				if (Msh.Material.IsTransparent)
					Msh.Draw(Position, Scale, Rotation);
		}
	}
}
