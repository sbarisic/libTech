using FishGfx.Graphics;
using libTech.Importer;
using libTech.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech {
	public static partial class Engine {
		static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();
		static Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();
		static Dictionary<string, Material> Materials = new Dictionary<string, Material>();

		public static Texture ErrorTexture;
		public static Material ErrorMaterial;

		public static T Load<T>(string Pth) {
			return Importers.Get<T>(Pth).Load(Pth);
		}

		public static void RegisterShader(string Name, ShaderProgram Prog) {
			Shaders.Add(Name, Prog);
		}

		public static ShaderProgram GetShader(string Name) {
			return Shaders[Name];
		}

		public static Texture GetTexture(string Name) {
			if (Name == "error") {
				if (ErrorTexture == null) {
					ErrorTexture = Texture.FromFile("content/textures/error.png");
					ErrorTexture.SetWrap(TextureWrap.Repeat);
				}

				return ErrorTexture;
			}

			if (!Textures.ContainsKey(Name)) {
				Texture T = Load<Texture>(Name);

				if (T == null) {
					Console.WriteLine("Could not find texture '{0}'", Name);
					T = GetTexture("error");
				} else
					Textures.Add(Name, T);

				return T;
			}

			return Textures[Name];
		}

		public static void RegisterMaterial(string Name, Material Material) {
			Materials.Add(Name, Material);
		}

		public static Material GetMaterial(string Name) {
			if (Name == "error") {
				if (ErrorMaterial == null)
					ErrorMaterial = new TexturedShaderMaterial("default", GetTexture("error"));

				return ErrorMaterial;
			}

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
