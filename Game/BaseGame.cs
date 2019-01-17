using FishGfx;
using FishGfx.Formats;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using libTech;
using libTech.Entities;
using libTech.Graphics;
using libTech.GUI;
using libTech.GUI.Controls;
using libTech.Importer;
using libTech.libNative;
using libTech.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Color = FishGfx.Color;

namespace Game {
	public unsafe class Game : LibTechGame {
		bool OptionsWindowShown = false;

		ShaderProgram MenuMeshShader;
		Mesh3D MenuMesh;
		Texture MenuMeshTex;

		public override void Load() {
			MenuMeshShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"), new ShaderStage(ShaderType.FragmentShader, "content/shaders/default_tex_clr.frag"));
			MenuMesh = new Mesh3D(Smd.Load("content/models/oildrum001_explosive_reference.smd")[0]);
			MenuMeshTex = Texture.FromFile("content/textures/oil_drum001h.png");
			MenuMeshTex.SetFilter(TextureFilter.Linear);

			Camera Cam = Engine.Camera3D;
			Cam.Position = new Vector3(40, 25, 40);
			Cam.LookAt(Vector3.Zero);

			Window MainMenuWindow = new Window(Engine.UI);
			MainMenuWindow.Position = new Vector2(50, 50);
			MainMenuWindow.SizeMode = SizeMode.EncapsuleChildren;
			MainMenuWindow.Title = "libTech";
			MainMenuWindow.Closable = false;
			Engine.UI.AddControl(MainMenuWindow);

			const float BtnPadding = 7;
			const float BtnHeight = 30;
			const float BtnWidth = 200;

			MainMenuWindow.AddButton(MainMenuWindow.ClientArea + new Vector2(0, BtnHeight * 2 + BtnPadding * 2), new Vector2(BtnWidth, BtnHeight), "New Game", (S, E) => { });
			MainMenuWindow.AddButton(MainMenuWindow.ClientArea + new Vector2(0, BtnHeight * 1 + BtnPadding * 1), new Vector2(BtnWidth, BtnHeight), "Options", (S, E) => { SpawnOptionsWindow(); });
			MainMenuWindow.AddButton(MainMenuWindow.ClientArea + new Vector2(0, BtnHeight * 0 + BtnPadding * 0), new Vector2(BtnWidth, BtnHeight), "Quit", (S, E) => { Environment.Exit(0); });
		}

		void SpawnOptionsWindow() {
			if (OptionsWindowShown)
				return;

			OptionsWindowShown = true;

			Window OptionsWindow = new Window(Engine.UI);
			OptionsWindow.OnClose += () => OptionsWindowShown = false;
			OptionsWindow.Title = "Options";
			OptionsWindow.Position = new Vector2(200, 200);
			OptionsWindow.Size = new Vector2(300, 300);
			Engine.UI.AddControl(OptionsWindow);
		}

		public override void Draw(float Dt) {
			base.Draw(Dt);

			/*ShaderUniforms.Current.Model = Matrix4x4.CreateFromYawPitchRoll(Engine.Time / 4, -(float)Math.PI / 2, 0) * Matrix4x4.CreateTranslation(new Vector3(7, -25, -25));

			MenuMeshShader.Bind(ShaderUniforms.Current);
			MenuMeshTex.BindTextureUnit();
			MenuMesh.Draw();
			MenuMeshTex.UnbindTextureUnit();
			MenuMeshShader.Unbind();

			ShaderUniforms.Current.Model = Matrix4x4.Identity;*/
		}
	}
}