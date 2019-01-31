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

		// TODO: Move these to a separate file and make a proper definition thing?	
		internal static void LoadShaders() {
			RegisterShader("default", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/default_tex_clr.frag")));
			RegisterShader("water", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/water.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/water.frag")));
			RegisterShader("framebuffer", new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/fb.frag")));
		}

		internal static void LoadMaterials() {
			ShaderMaterial WaterMaterial = new ShaderMaterial("water");
			WaterMaterial.Translucent = true;
			RegisterMaterial("water", WaterMaterial);

			string[] MaterialDefs = VFS.GetFiles("/content/materials/").Where(FName => Path.GetExtension(FName) == ".ltm").ToArray();

			foreach (var MatDefFile in MaterialDefs) {
				string[] MatDefs = VFS.ReadAllText(MatDefFile).Replace("\r", "").Trim().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

				foreach (var MatDef in MatDefs) {
					string[] KV = MatDef.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					Texture Texx = GetTexture("/content/" + KV[1]);
					Texx.SetFilter(TextureFilter.Linear);
					Texx.SetWrap(TextureWrap.Repeat);

					TexturedShaderMaterial Mat = new TexturedShaderMaterial("default", Texx);
					RegisterMaterial(KV[0], Mat);
				}
			}
		}
	}
}
