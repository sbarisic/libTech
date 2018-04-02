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

		public ShaderProgram Shader;
		public bool IsTransparent;
		public string Name;

		public Material() {
			Name = "";
			DiffuseColor = Vector4.One;
			IsTransparent = false;
			Shader = ShaderProgram.Default;
		}

		public void Bind() {
			Shader.Bind();
			Diffuse?.BindTextureUnit();
		}

		public void Unbind() {
			Diffuse?.UnbindTextureUnit();
			Shader.Unbind();
		}

		public override string ToString() {
			if (!string.IsNullOrEmpty(Name))
				return Name;

			return base.ToString();
		}
	}
}
