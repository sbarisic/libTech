using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Materials {
	public class ShaderMaterial : Material {
		public ShaderMaterial(string ShaderName) : base(Engine.GetShader(ShaderName), ShaderName) {
		}
	}
}
