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
using libTech.Materials;
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
			Engine.Map = BSPMap.LoadMap("/content/maps/gm_flatgrass.bsp");
			Engine.Map.InitPhysics();
			Console.WriteLine("Done!");

			PlayerSpawn[] SpawnPositions = Engine.Map.GetEntities<PlayerSpawn>().ToArray();
			PlayerEnt = new Player();
			PlayerEnt.SetPosition(SpawnPositions.Random().SpawnPosition + new Vector3(0, 100, 0));
			PlayerEnt.Camera.LookAt(Vector3.Zero);

			Engine.Map.SpawnEntity(UtilGun);
			Engine.Map.SpawnEntity(PlayerEnt);
			PlayerEnt.WeaponPickUp(UtilGun);

			string MdlName = /*"models/hunter/blocks/cube05x05x05.mdl";*/  "models/props_c17/oildrum001_explosive.mdl";
			BarrelModel = Engine.Load<libTechModel>(MdlName);
			BarrelModel.CenterModel();
			Vector3 BarrelSpawn = SpawnPositions.Random().SpawnPosition;

			/*ShaderProgram ShadowVolumeProg = Engine.GetShader("shadow_volume");
			ShadowVolumeProg.Uniform3f("LightPosition", new Vector3(-20, -12103, 109));
			ShadowVolumeProg.Uniform1f("LightRadius", 300.0f);

			libTechMesh BarrelMesh = BarrelModel.Meshes.First();
			BarrelMesh.Material = new ShaderMaterial("shadow_volume", ShadowVolumeProg);*/

			/*for (int i = 0; i < 10; i++) {
				EntPhysics Barrel = EntPhysics.FromModel(BarrelModel, 1 + 5 * i);
				Barrel.SetPosition(BarrelSpawn + new Vector3(0, 30 + i * 20, 0));
				Map.SpawnEntity(Barrel);
			}*/

			foreach (var L in Engine.Map.GetLights())
				Engine.Map.RemoveEntity(L);

			//*
			//Engine.Map.SpawnEntity(new DynamicLight(new Vector3(-20, -12103, 109), Color.White, 300));

			Engine.Map.SpawnEntity(new DynamicLight(new Vector3(-237, -12150, 776), Color.Green, 600));
			Engine.Map.SpawnEntity(new DynamicLight(new Vector3(-44, -12150, 772), Color.Red, 600));
			Engine.Map.SpawnEntity(new DynamicLight(new Vector3(-75, -12249, 1016), Color.Blue, 600));
			Engine.Map.SpawnEntity(new DynamicLight(new Vector3(470, -12448, 1022), Color.White, 600));

			//*/
			{
				/*EntPhysics Barrel = EntPhysics.FromModel(BarrelModel, 10);
				Barrel.SetPosition(new Vector3(-20, -12103, 109));
				Engine.Map.SpawnEntity(Barrel);*/

				for (int i = 0; i < 2; i++) {
					EntPhysics Barrel = EntPhysics.FromModel(BarrelModel, 10);
					//Barrel.SetPosition(new Vector3(-20, -12103, 109) + new Vector3(0, 20, 0) * i);
					Barrel.SetPosition(BarrelSpawn + new Vector3(0, 20, 0) * i);
					Engine.Map.SpawnEntity(Barrel);
				}
			}

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

			//Console.WriteLine(string.Format("Engine.Map.SpawnEntity(new DynamicLight(new Vector3({0}, {1}, {2}), Color.White, 600))", (int)PlayerEnt.Position.X, (int)PlayerEnt.Position.Y, (int)PlayerEnt.Position.Z));
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
				MenuModel.DrawOpaque();
				MenuModel.DrawTransparent();
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