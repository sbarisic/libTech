using FishGfx.Graphics;
using libTech.Importer;
using libTech.Materials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	public static partial class Engine {
		static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();
		static Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();
		static Dictionary<string, Material> Materials = new Dictionary<string, Material>();
		
		public static T Load<T>(string Pth) {
			return Importers.Get<T>(Pth).Load(Pth);
		}

		public static void RegisterShader(string Name, ShaderProgram Prog) {
			Shaders.Add(Name, Prog);
		}

		public static ShaderProgram GetShader(string Name) {
			return Shaders[Name];
		}

		public static void RegisterTexture(string Name, Texture Tex) {
			Textures.Add(Name, Tex);
		}

		// TODO: Remove automatic loading
		public static Texture GetTexture(string Name) {
			if (!Textures.ContainsKey(Name)) {
				Texture T = Load<Texture>(Name);

				if (T == null) {
					Console.WriteLine("Could not find texture '{0}'", Name);
					T = GetTexture("error");
				} else
					RegisterTexture(Name, T);

				return T;
			}

			return Textures[Name];
		}

		public static void RegisterMaterial(Material Material) {
			Materials.Add(Material.MaterialName, Material);
		}

		public static Material GetMaterial(string Name) {
			if (!Materials.ContainsKey(Name)) {
				Material M = Load<Material>(Name);

				if (M == null) {
					Console.WriteLine("Could not find material '{0}'", Name);
					M = GetMaterial("error");
				} else
					Materials.Add(Name, M);

				return M;
			}

			return Materials[Name];
		}
	}
}
