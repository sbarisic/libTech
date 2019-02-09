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
using libTech.Map;
using libTech.Models;
using libTech.Scripting;
using libTech.Weapons;
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
		libTechModel MenuModel;
		Texture MenuWallpaperTex;

		libTechModel BarrelModel;
		Texture CrosshairTex;

		Player PlayerEnt;

		const float BtnPadding = 7;
		const float BtnHeight = 30;
		const float BtnWidth = 200;

		public override void Load() {
			CrosshairTex = Engine.Load<Texture>("/content/textures/gui/crosshair_default.png");
			UtilityGun UtilGun = new UtilityGun();

			Console.WriteLine("Loading map");
			Engine.Map = BSPMap.LoadMap("/content/maps/gm_construct.bsp");
			Engine.Map.InitPhysics();
			Console.WriteLine("Done!");

			PlayerSpawn[] SpawnPositions = Engine.Map.GetEntities<PlayerSpawn>().ToArray();
			PlayerEnt = new Player();
			PlayerEnt.SetPosition(SpawnPositions.Random().SpawnPosition + new Vector3(0, 100, 0));
			PlayerEnt.Camera.LookAt(Vector3.Zero);

			Engine.Map.SpawnEntity(UtilGun);
			Engine.Map.SpawnEntity(PlayerEnt);
			PlayerEnt.WeaponPickUp(UtilGun);

			BarrelModel = Engine.Load<libTechModel>("models/props_c17/oildrum001_explosive.mdl");
			BarrelModel.CenterModel();
			Vector3 BarrelSpawn = SpawnPositions.Random().SpawnPosition;

			/*for (int i = 0; i < 10; i++) {
				EntPhysics Barrel = EntPhysics.FromModel(BarrelModel, 1 + 5 * i);
				Barrel.SetPosition(BarrelSpawn + new Vector3(0, 30 + i * 20, 0));
				Map.SpawnEntity(Barrel);
			}*/

			/*
			Engine.Map.SpawnEntity(new DynamicLight(new Vector3(-1274, -12396, 568), new Color(66, 244, 83)));
			Engine.Map.SpawnEntity(new DynamicLight(new Vector3(-1151, -12396, 1123), new Color(221, 63, 226)));
			Engine.Map.SpawnEntity(new DynamicLight(new Vector3(-675, -12396, 1415), new Color(204, 204, 42)));
			Engine.Map.SpawnEntity(new DynamicLight(new Vector3(-866, -12067, 1078), new Color(33, 154, 160)));
			Engine.Map.SpawnEntity(new DynamicLight(new Vector3(-211, -12555, 872), new Color(18, 28, 132)));
			Engine.Map.SpawnEntity(new DynamicLight(new Vector3(-375, -12500, 926), new Color(170, 18, 33)));
			Engine.Map.SpawnEntity(new DynamicLight(new Vector3(-466, -12504, 1028), new Color(214, 124, 0)));
			//*/

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
		}

		public override void Update(float Dt) {
			base.Update(Dt);

			//Console.WriteLine(string.Format("new DynamicLight(new Vector3({0}, {1}, {2}), Color.White)", (int)PlayerEnt.Position.X, (int)PlayerEnt.Position.Y, (int)PlayerEnt.Position.Z));
		}

		public override void DrawOpaque() {
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
				MenuModel.Rotation = Quaternion.CreateFromYawPitchRoll(Engine.Time / 4, -(float)Math.PI / 2, 0);
				MenuModel.Position = new Vector3(7, -10, -25);
				MenuModel.Draw();
			}

			Engine.Map?.DrawOpaque();

			//Gfx.ClearDepth();
			PlayerEnt?.DrawViewModel();
		}

		public override void DrawTransparent() {
			Engine.Map?.DrawTransparent();
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