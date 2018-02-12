using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace libTech.Graphics {
	public class Mesh {
		const int VERTEX_ATTRIB = 0;
		const int COLOR_ATTRIB = 1;
		const int UV_ATTRIB = 2;

		VertexArray VAO;
		BufferObject VertBuffer;
		BufferObject ColorBuffer;
		BufferObject UVBuffer;
		BufferObject ElementBuffer;

		public Matrix4x4 Matrix;
		public Material Material;

		public Mesh() {
			VAO = new VertexArray();
			Matrix = Matrix4x4.Identity;
		}

		public void SetVertices(Vector3[] Verts) {
			VertBuffer = new BufferObject();
			VertBuffer.SetData(Verts);

			VAO.AttribFormat(VERTEX_ATTRIB);
			VAO.AttribBinding(VERTEX_ATTRIB, VAO.BindVertexBuffer(VertBuffer));
		}

		public void SetColors(Vector4[] Colors) {
			ColorBuffer = new BufferObject();
			ColorBuffer.SetData(Colors);

			VAO.AttribFormat(COLOR_ATTRIB, Size: 4);
			VAO.AttribBinding(COLOR_ATTRIB, VAO.BindVertexBuffer(ColorBuffer, Stride: 4 * sizeof(float)));
		}

		public void SetUVs(Vector2[] UVs) {
			UVBuffer = new BufferObject();
			UVBuffer.SetData(UVs);

			VAO.AttribFormat(UV_ATTRIB, Size: 2);
			VAO.AttribBinding(UV_ATTRIB, VAO.BindVertexBuffer(UVBuffer, Stride: 2 * sizeof(float)));
		}

		public void SetElements(uint[] Elements) {
			ElementBuffer = new BufferObject();
			ElementBuffer.SetData(Elements);

			VAO.BindElementBuffer(ElementBuffer);
		}

		public void Draw() {
			if (ColorBuffer == null)
				VertexArray.VertexAttrib(COLOR_ATTRIB, Material.DiffuseColor);

			Material.Bind();

			if (ElementBuffer == null)
				VAO.Draw(0, VertBuffer.ElementCount);
			else
				VAO.DrawElements(ElementType: OpenGL.DrawElementsType.UnsignedInt);
			
			Material.Unbind();
		}
	}
}
