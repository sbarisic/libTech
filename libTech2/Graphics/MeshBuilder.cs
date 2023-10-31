using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FishGfx;

namespace libTech.Graphics {
	class MeshBuilder {
		List<Vertex3> Verts;

		Vector3 PosOffset;
		Vector2 UVPos;
		Vector2 UVSize;

		public MeshBuilder() {
			Verts = new List<Vertex3>();
			SetPositionOffset(Vector3.Zero);
			SetUVOffsetSize(Vector2.Zero, Vector2.One);
		}

		public void SetPositionOffset(Vector3 Pos) {
			PosOffset = Pos;
		}

		public void SetUVOffsetSize(Vector2 UVPos, Vector2 UVSize) {
			this.UVPos = UVPos;
			this.UVSize = UVSize;
		}

		public void Add(Vertex3 Vert) {
			Verts.Add(Vert);
		}

		public void Add(Vector3 Pos) {
			Add(new Vertex3(Pos + PosOffset, Vector2.Zero));
		}

		public void Add(Vector3 Pos, Vector2 UV) {
			Add(new Vertex3(Pos + PosOffset, UVPos + UV * UVSize));
		}

		public void Add(Vector3 Pos, Vector2 UV, Color Clr) {
			Add(new Vertex3(Pos + PosOffset, UVPos + UV * UVSize, Clr));
		}

		/*public void Add(Vector3 Pos, Vector2 UV, Color Clr) {
			Add(new Vertex3(Pos + PosOffset, UVPos + UV * UVSize, Clr));
		}*/

		public Vertex3[] ToArray() {
			return Verts.ToArray();
		}
	}
}
