using FishGfx.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Materials {
	public class ValveMaterial : Material {
		public Texture Texture;
		public string MaterialDefinition;

		public ValveMaterial(string MaterialDefinition, string MaterialName) : base(Engine.GetShader("default"), MaterialName) {
			RETRY:
			this.MaterialDefinition = MaterialDefinition;

			string[] Lines = MaterialDefinition.Replace("{", "").Replace("}", "").Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(L => L.Trim()).ToArray();
			Lines = Lines.Where(L => !(L.StartsWith("//") || L.Length == 0)).ToArray();

			string TexturePath = "";
			bool RemoveAlpha = false;

			/*if (Lines[0].ToLower() == "\"patch\"")
				Debugger.Break();*/

			for (int i = 0; i < Lines.Length; i++) {
				int SpaceChar = Lines[i].IndexOf(' ');
				if (SpaceChar == -1)
					SpaceChar = Lines[i].IndexOf('\t');

				if (i == 0)
					continue;

				string K = Lines[i].TrimQuotes().ToLower();
				string V = "";

				if (SpaceChar != -1) {
					K = Lines[i].Substring(0, SpaceChar).TrimQuotes().ToLower();
					V = Lines[i].Substring(SpaceChar + 1, Lines[i].Length - SpaceChar - 1).TrimQuotes();
				}

				switch (K) {
					case "include": {
							string FileName = "/" + V.Replace("\\", "/");
							MaterialDefinition = Engine.VFS.ReadAllText(FileName);
							goto RETRY;
						}

					case "$basetexture":
						TexturePath = PathUtils.Combine("materials", V + ".vtf");
						break;

					case "$basemapalphaphongmask":
					case "$basealphaenvmapmask":
					case "$selfillum":
						RemoveAlpha = true;
						break;

					case "$blendtintbybasealpha": // TODO
						BlendTintByBaseAlpha = V == "1";
						break;

					case "$nocull":
						NoCull = V == "1";
						break;

					case "$translucent":
						Translucent = V == "1";
						break;

					case "$alphatest":
						AlphaTest = V == "1";
						break;

					default:
						break;
				}
			}

			if (string.IsNullOrEmpty(TexturePath)) {
				Texture = Engine.GetTexture("error");
			} else {
				Texture = Engine.Load<Texture>(TexturePath);

				if (RemoveAlpha || BlendTintByBaseAlpha) {
					Bitmap Bmp = Texture.GetPixelsAsBitmap();
					Bmp.RemoveAlpha(BlendTintByBaseAlpha ? new Nullable<Color>(Color.FromArgb(128, 128, 128)) : null);
					Texture.UpdateFromImage(Texture, Bmp);
				}
			}
		}

		public override void Bind() {
			base.Bind();
			Texture.BindTextureUnit();
		}

		public override void Unbind() {
			Texture.UnbindTextureUnit();
			base.Unbind();
		}

		public static Material CreateMaterial(Stream MaterialStream, string MaterialName) {
			string MaterialDefinition = "";

			if (MaterialStream == null)
				return Engine.GetMaterial("error");

			using (StreamReader SR = new StreamReader(MaterialStream))
				MaterialDefinition = SR.ReadToEnd();

			return new ValveMaterial(MaterialDefinition, MaterialName);
		}

		public static Material CreateMaterial(string MaterialName) {
			return CreateMaterial(Engine.VFS.OpenFile(MaterialName), MaterialName);
		}
	}
}
