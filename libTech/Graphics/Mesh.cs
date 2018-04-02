using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Quaternion = System.Numerics.Quaternion;
using Matrix4x4 = System.Numerics.Matrix4x4;
using OpenGL;

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
		BufferUsage Usage;

		public Matrix4x4 Matrix;
		public Material Material;
		public bool Wireframe;
		public bool Solid;

		public Vector3[] Vertices { get; private set; }

		public PrimitiveType PrimitiveType {
			get {
				return VAO.PrimitiveType;
			}

			set {
				VAO.PrimitiveType = value;
			}
		}

		public Mesh(BufferUsage Usage = BufferUsage.StaticDraw) {
			VAO = new VertexArray();
			Matrix = Matrix4x4.Identity;
			Material = new Material();

			Wireframe = false;
			Solid = true;
			this.Usage = Usage;
		}

		public void SetVertices(Vector3[] Verts) {
			Vertices = Verts;

			if (VertBuffer == null) {
				VAO.AttribFormat(VERTEX_ATTRIB);
				VAO.AttribBinding(VERTEX_ATTRIB, VAO.BindVertexBuffer(VertBuffer = new BufferObject()));
			}

			VertBuffer.SetData(Verts, Usage: Usage);
			VAO.AttribEnable(VERTEX_ATTRIB, Verts != null);
		}

		public void SetColors(Vector4[] Colors) {
			if (ColorBuffer == null) {
				VAO.AttribFormat(COLOR_ATTRIB, Size: 4);
				VAO.AttribBinding(COLOR_ATTRIB, VAO.BindVertexBuffer(ColorBuffer = new BufferObject(), Stride: 4 * sizeof(float)));
			}

			ColorBuffer.SetData(Colors, Usage: Usage);
			VAO.AttribEnable(COLOR_ATTRIB, Colors != null);
		}

		public void SetUVs(Vector2[] UVs) {
			if (UVBuffer == null) {
				VAO.AttribFormat(UV_ATTRIB, Size: 2);
				VAO.AttribBinding(UV_ATTRIB, VAO.BindVertexBuffer(UVBuffer = new BufferObject(), Stride: 2 * sizeof(float)));
			}

			UVBuffer.SetData(UVs, Usage: Usage);
			VAO.AttribEnable(UV_ATTRIB, UVs != null);
		}

		public void SetElements(uint[] Elements) {
			if (ElementBuffer == null)
				ElementBuffer = new BufferObject();

			if (Elements != null) {
				VAO.BindElementBuffer(ElementBuffer);
				ElementBuffer.SetData(Elements, Usage: Usage);
			} else
				VAO.BindElementBuffer(null);
		}

		public void Draw(Vector3 Position, Vector3 Scale, Quaternion Rotation) {
			if (ColorBuffer == null)
				VertexArray.VertexAttrib(COLOR_ATTRIB, Material.DiffuseColor);

			Material.Bind();
			Material.Shader.SetModelMatrix(Camera.CreateModel(Position, Scale, Rotation) * Matrix);

			if (Wireframe)
				Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

			if (!VAO.HasElementBuffer)
				VAO.Draw(0, VertBuffer.ElementCount);
			else
				VAO.DrawElements(ElementType: DrawElementsType.UnsignedInt);

			if (Wireframe)
				Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

			Material.Unbind();
		}
	}
}
