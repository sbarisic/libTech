﻿using FishGfx.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Materials {
	public class TexturedShaderMaterial : Material {
		public Texture Texture;

		public TexturedShaderMaterial(string ShaderName, Texture Texture) : base(Engine.GetShader(ShaderName), ShaderName) {
			if (Texture == null)
				throw new ArgumentException("Texture can not be null");

			this.Texture = Texture;
		}

		public override void Bind() {
			base.Bind();
			Texture.BindTextureUnit();
		}

		public override void Unbind() {
			Texture.UnbindTextureUnit();
			base.Unbind();
		}
	}
}
