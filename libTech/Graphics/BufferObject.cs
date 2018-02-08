using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using System.Numerics;

namespace libTech.Graphics {
	public unsafe class BufferObject : GraphicsObject {
		public int Size { get; private set; }
		public int ElementCount { get; private set; }

		public BufferObject() {
			ID = Gl.CreateBuffer();
		}

		public void SetData(uint Size, IntPtr Data, BufferUsage Usage = BufferUsage.DynamicDraw) {
			this.Size = (int)Size;

			Gl.NamedBufferData(ID, Size, Data, Usage);
		}

		public void SetData(Vector3[] Data, BufferUsage Usage = BufferUsage.DynamicDraw) {
			ElementCount = Data.Length;

			fixed (Vector3* DataPtr = Data)
				SetData((uint)(sizeof(Vector3) * Data.Length), (IntPtr)DataPtr, Usage);
		}

		public override void GraphicsDispose() {
			Gl.UnmapNamedBuffer(ID);
			Gl.DeleteBuffers(new uint[] { ID });
		}
	}
}
