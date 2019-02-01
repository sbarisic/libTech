﻿using FishGfx;
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
using libTech.Map;
using libTech.Models;
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

		/*ShaderProgram MenuMeshShader;
		Mesh3D MenuMesh;
		Texture MenuMeshTex;*/

		libTechModel MenuModel;
		Texture MenuWallpaperTex;

		libTechModel BarrelModel;

		libTechMap Map;

		Texture CrosshairTex;

		const float BtnPadding = 7;
		const float BtnHeight = 30;
		const float BtnWidth = 200;

		public override void Load() {
			/*MenuModel = Engine.Load<libTechModel>(new[] { "logo_gmod_b.mdl", "dynamite.mdl", "armchair.mdl" }.Random());
			MenuModel.CenterModel();
			MenuModel.ScaleToSize(80);

			MenuWallpaperTex = Texture.FromFile("content/textures/wallpaper.png");
			//*/

			CrosshairTex = Engine.Load<Texture>("/content/textures/gui/crosshair_default.png");

			Console.WriteLine("Loading map");
			Map = BSPMap.LoadMap("/content/maps/gm_construct.bsp");
			Map.InitPhysics();
			Console.WriteLine("Done!");

			PlayerSpawn[] SpawnPositions = Map.GetEntities<PlayerSpawn>().ToArray();
			Player PlayerEnt = new Player();
			PlayerEnt.Position = SpawnPositions.Random().SpawnPosition + new Vector3(0, 100, 0);
			Map.SpawnEntity(PlayerEnt);

			BarrelModel = Engine.Load<libTechModel>("models/props_c17/oildrum001_explosive.mdl");
			BarrelModel.CenterModel();

			/*for (int i = 0; i < 5; i++) {
				EntPhysics Barrel = EntPhysics.FromModel(BarrelModel, 1);
				Barrel.SetPosition(new Vector3(0, 30 + i * 20, 0));
				Map.SpawnEntity(Barrel);
			}*/


			/*Camera Cam = Engine.Camera3D;
			Cam.Position = new Vector3(40, 25, 40);
			Cam.LookAt(Vector3.Zero);

			Window MainMenuWindow = new Window(Engine.UI);
			MainMenuWindow.Position = new Vector2(50, 50);
			MainMenuWindow.SizeMode = SizeMode.EncapsuleChildren;
			//MainMenuWindow.Size = new Vector2(200, 200);
			MainMenuWindow.Title = "libTech";
			MainMenuWindow.Closable = false;
			Engine.UI.AddControl(MainMenuWindow);

			MainMenuWindow.AddButton(MainMenuWindow.ClientArea + new Vector2(0, BtnHeight * 2 + BtnPadding * 2), new Vector2(BtnWidth, BtnHeight), "New Game", (S, E) => {
			});

			MainMenuWindow.AddButton(MainMenuWindow.ClientArea + new Vector2(0, BtnHeight * 1 + BtnPadding * 1), new Vector2(BtnWidth, BtnHeight), "Options", (S, E) => {
			});

			MainMenuWindow.AddButton(MainMenuWindow.ClientArea + new Vector2(0, BtnHeight * 0 + BtnPadding * 0), new Vector2(BtnWidth, BtnHeight), "Quit", (S, E) => {
				Environment.Exit(0);
			});
			//*/

			//*
			Engine.Camera3D.MouseMovement = true;
			Engine.Window.CaptureCursor = true;

			Engine.Window.OnMouseMoveDelta += (Wnd, X, Y) => {
				PlayerEnt.MouseMove(new Vector2(-X, -Y));
			};

			Engine.Window.OnKey += (Wnd, Key, Scancode, Pressed, Repeat, Mods) => {
				PlayerEnt.OnKey(Key, Pressed, Mods);
				if (Key == Key.Escape && Pressed)
					Environment.Exit(0);
			};
			//*/
		}

		void SpawnOptionsWindow() {
			if (OptionsWindowShown)
				return;

			OptionsWindowShown = true;

			Window OptionsWindow = new Window(Engine.UI);
			OptionsWindow.OnClose += () => OptionsWindowShown = false;
			OptionsWindow.Title = "Options";
			OptionsWindow.SizeMode = SizeMode.EncapsuleChildren;
			//OptionsWindow.Size = new Vector2(200, 200);
			OptionsWindow.Position = new Vector2(200, 200);
			Engine.UI.AddControl(OptionsWindow);

			OptionsWindow.AddButton(OptionsWindow.ClientArea + new Vector2(100, 0), new Vector2(BtnWidth / 2, BtnHeight), "OK", (S, E) => {
				OptionsWindow.Close();
			});

			OptionsWindow.AddButton(OptionsWindow.ClientArea + new Vector2(100 + BtnWidth / 2 + BtnPadding, 0), new Vector2(BtnWidth / 2, BtnHeight), "Cancel", (S, E) => {
				OptionsWindow.Close();
			});

			Label Lbl = new Label(Engine.UI);
			Lbl.Position = new Vector2(10, 200);
			Lbl.Text = "// TODO: Put stuff here";
			OptionsWindow.AddChild(Lbl);

			for (int i = 1; i < 6; i++) {
				CheckBox CBox = new CheckBox(Engine.UI);
				CBox.Position = new Vector2(10, 200 - 20 * i);
				CBox.Text = "Check me #" + i;
				OptionsWindow.AddChild(CBox);
			}
		}

		public override void Update(float Dt) {
			base.Update(Dt);
			Map.Update(Dt);
		}

		public override void Draw(float Dt) {
			base.Draw(Dt);
			Gfx.Clear(new Color(80, 90, 100));

			if (MenuWallpaperTex != null) {
				ShaderUniforms.Current.Camera = Engine.Camera2D;
				RenderState RS = Gfx.PeekRenderState();
				RS.EnableDepthMask = false;
				Gfx.PushRenderState(RS);

				Gfx.TexturedRectangle(0, 0, Engine.WindowWidth, Engine.WindowHeight, Texture: MenuWallpaperTex);

				Gfx.PopRenderState();
			}

			ShaderUniforms.Current.Camera = Engine.Camera3D;
			if (MenuModel != null) {
				MenuModel.Rotation = Matrix4x4.CreateFromYawPitchRoll(Engine.Time / 4, -(float)Math.PI / 2, 0);
				MenuModel.Position = new Vector3(7, -10, -25);
				MenuModel.Draw();
			}

			Map?.Draw();
		}

		public override void DrawGUI(float Dt) {
			if (CrosshairTex != null) {
				float CW = CrosshairTex.Width;
				float CH = CrosshairTex.Height;
				Gfx.TexturedRectangle(Engine.Window.WindowWidth / 2 - CW / 2, Engine.Window.WindowHeight / 2 - CH / 2, CW, CH, Texture: CrosshairTex);
			}
		}
	}
}