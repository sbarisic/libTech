using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace libTech.Graphics {
	public abstract class GraphicsObject {
		public uint ID;

		public virtual void Bind() {
			throw new InvalidOperationException("Implemented function call");
		}

		public virtual void Unbind() {
			throw new InvalidOperationException("Implemented function call");
		}

		public void SetLabel(ObjectIdentifier ObjID, string Lbl) {
#if DEBUG
			Gl.ObjectLabel(ObjID, ID, Lbl.Length, Lbl);
#endif
		}

		public abstract void GraphicsDispose();
	}
}
