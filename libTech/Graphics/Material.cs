using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace libTech.Graphics {
	public class Material {
		public Vector4 DiffuseColor;

		public Texture Diffuse;

		public bool IsTransparent;

		public Material() {
			DiffuseColor = Vector4.One;
			IsTransparent = false;
		}

		public void Bind() {
			Diffuse?.BindTextureUnit();
		}

		public void Unbind() {
			Diffuse?.UnbindTextureUnit();
		}
	}
}
