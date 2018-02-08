using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace libTech.Graphics {
	public class VertexArray : GraphicsObject {
		public PrimitiveType PrimitiveType;

		List<BufferObject> BufferObjects;
		int FreeBindingIndex = 0;

		public VertexArray() {
			ID = Gl.CreateVertexArray();

			PrimitiveType = PrimitiveType.Triangles;
			BufferObjects = new List<BufferObject>();
		}

		public override void Bind() {
			Gl.BindVertexArray(ID);
		}

		public override void Unbind() {
			Gl.BindVertexArray(0);
		}

		public void Draw(int First, int Count) {
			Bind();

			// TODO: Elements

			Gl.DrawArrays(PrimitiveType, First, Count);
			Unbind();
		}

		public uint BindVertexBuffer(BufferObject Obj, int BindingIndex = -1, int Offset = 0, int Stride = 3 * sizeof(float)) {
			if (!BufferObjects.Contains(Obj))
				BufferObjects.Add(Obj);

			if (BindingIndex == -1)
				BindingIndex = FreeBindingIndex++;

			if (Obj != null)
				Gl.VertexArrayVertexBuffer(ID, (uint)BindingIndex, Obj.ID, (IntPtr)Offset, Stride);

			return (uint)BindingIndex;
		}

		public void BindElementBuffer(BufferObject Obj) {
			BufferObjects.Add(Obj);

			if (Obj != null)
				Gl.VertexArrayElementBuffer(ID, Obj.ID);
		}

		public void AttribEnable(uint AttribIdx, bool Enable = true) {
			if (Enable)
				Gl.EnableVertexArrayAttrib(ID, AttribIdx);
			else
				Gl.DisableVertexArrayAttrib(ID, AttribIdx);
		}

		public void AttribFormat(uint AttribIdx, int Size = 3, VertexAttribType AttribType = VertexAttribType.Float, bool Normalized = false, uint RelativeOffset = 0) {
			Gl.VertexArrayAttribFormat(ID, AttribIdx, Size, AttribType, Normalized, RelativeOffset);
		}

		public void AttribBinding(uint AttribIdx, uint BindingIdx) {
			AttribEnable(AttribIdx);
			Gl.VertexArrayAttribBinding(ID, AttribIdx, BindingIdx);
		}

		public override void GraphicsDispose() {
			Gl.DeleteVertexArrays(new uint[] { ID });
		}
	}
}
