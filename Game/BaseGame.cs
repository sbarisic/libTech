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
		libTechMap Map;
		Texture CrosshairTex;

		Player PlayerEnt;

		const float BtnPadding = 7;
		const float BtnHeight = 30;
		const float BtnWidth = 200;

		public override void Load() {
			CrosshairTex = Engine.Load<Texture>("/content/textures/gui/crosshair_default.png");
			UtilityGun UtilGun = new UtilityGun();

			Console.WriteLine("Loading map");
			Map = BSPMap.LoadMap("/content/maps/gm_flatgrass.bsp");
			Map.InitPhysics();
			Console.WriteLine("Done!");

			PlayerSpawn[] SpawnPositions = Map.GetEntities<PlayerSpawn>().ToArray();
			PlayerEnt = new Player();
			PlayerEnt.Position = SpawnPositions.Random().SpawnPosition + new Vector3(0, 100, 0);

			Map.SpawnEntity(UtilGun);
			Map.SpawnEntity(PlayerEnt);
			PlayerEnt.WeaponPickUp(UtilGun);

			BarrelModel = Engine.Load<libTechModel>("models/props_c17/oildrum001_explosive.mdl");
			BarrelModel.CenterModel();
			Vector3 BarrelSpawn = SpawnPositions.Random().SpawnPosition;

			/*for (int i = 0; i < 10; i++) {
				EntPhysics Barrel = EntPhysics.FromModel(BarrelModel, 1 + 5 * i);
				Barrel.SetPosition(BarrelSpawn + new Vector3(0, 30 + i * 20, 0));
				Map.SpawnEntity(Barrel);
			}*/

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
			Map.Update(Dt);

			//Console.WriteLine(PlayerEnt.Position);
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

			Map?.DrawOpaque();

			//Gfx.ClearDepth();
			PlayerEnt?.DrawViewModel();
		}

		public override void DrawTransparent() {
			Map?.DrawTransparent();
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