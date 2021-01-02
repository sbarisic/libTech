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
using libTech.Graphics.Voxels;
using System.Text;
using System.Threading.Tasks;
using Color = FishGfx.Color;


namespace libTech.Game {
	public unsafe class Game : LibTechGame {
		//libTechModel MenuModel;
		//Texture MenuWallpaperTex;

		//libTechModel BarrelModel;
		Texture CrosshairTex;

		Player PlayerEnt;

		const float BtnPadding = 7;
		const float BtnHeight = 30;
		const float BtnWidth = 200;

		EntPhysics LightEmitter;
		DynamicLight Light;

		Vector3 PlySpawnPos;

		ChunkMap VoxelMap;

		public override void Load() {
			CrosshairTex = Engine.Load<Texture>("/content/textures/gui/crosshair_default.png");
			UtilityGun UtilGun = new UtilityGun();

			Console.WriteLine("Loading map");
			Engine.Map = BSPMap.LoadMap("/content/maps/lt_test.bsp");
			Engine.Map.InitPhysics();
			Console.WriteLine("Done!");

			PlayerSpawn[] SpawnPositions = Engine.Map.GetEntities<PlayerSpawn>().ToArray();
			PlayerEnt = new Player();
			PlayerEnt.SetPosition((PlySpawnPos = SpawnPositions.Random().SpawnPosition) + new Vector3(0, 100, 0));
			PlayerEnt.Camera.LookAt(Vector3.Zero);

			PlayerEnt.SetPosition(new Vector3(-285.1535f, -964.8776f, 229.2883f));

			Engine.Map.SpawnEntity(UtilGun);
			Engine.Map.SpawnEntity(PlayerEnt);
			PlayerEnt.WeaponPickUp(UtilGun);

			/*foreach (var L in Engine.Map.GetLights())
				Engine.Map.RemoveEntity(L);*/

			//LightEmitter = Engine.Map.GetEntities<EntPhysics>().Skip(5).First();
			//Light = new DynamicLight(Vector3.Zero, Color.Red);
			//Engine.Map.SpawnEntity(Light);


			/*EntStatic TestEntity = new EntStatic(new libTechModel(Obj.Load("content/models/cube.obj"), Engine.GetMaterial(FogMaterial.Name)));
			TestEntity.Model.Scale = new Vector3(120); // 20
													   //DragonEnt.Model.Position = new Vector3(0, -1120, 0);
			TestEntity.Model.Position = new Vector3(0, -1000, 0);
			Engine.Map.SpawnEntity(TestEntity);*/

			/*
			EntStatic TestEntity2 = new EntStatic(new libTechModel(TestEntity.Model));
			TestEntity2.Model.Position = new Vector3(240, -1000, 0);
			Engine.Map.SpawnEntity(TestEntity2);
			//*/

			//PlayerEnt.Camera.LookAt(TestEntity.Position);

			TexturedShaderMaterial VoxelMat = new TexturedShaderMaterial("default", Texture.FromFile("content/textures/voxel_atlas.png", true));
			VoxelMap = new ChunkMap(VoxelMat);
			VoxelMap.GenerateFloatingIsland(64, 64);


			Engine.Camera3D.MouseMovement = true;
			Engine.Window.CaptureCursor = true;

			Engine.Window.OnMouseMoveDelta += (Wnd, X, Y) => {
				PlayerEnt.MouseMove(new Vector2(-X, -Y));
			};

			Engine.Window.OnKey += (Wnd, Key, Scancode, Pressed, Repeat, Mods) => {
				PlayerEnt.OnKey(Key, Pressed, Mods);
				if (Key == Key.Escape && Pressed)
					Engine.Exit();

				if (Key == Key.F5 && Pressed)
					RenderDoc.CaptureFrame();

				if (Key == Key.F6 && Pressed)
					if (!Debugger.IsAttached)
						Debugger.Launch();
			};
		}

		public override void Update(float Dt) {
			if (LightEmitter != null && Light != null) {
				LightEmitter.GetWorldTransform(out Vector3 S, out Quaternion R, out Vector3 Pos);
				Light.Position = Pos;
			}

			base.Update(Dt);
			//Console.WriteLine(string.Format("Engine.Map.SpawnEntity(new DynamicLight(new Vector3({0}, {1}, {2}), Color.White, 600))", (int)PlayerEnt.Position.X, (int)PlayerEnt.Position.Y, (int)PlayerEnt.Position.Z));

			//Console.WriteLine("{0} {1}", PlayerEnt.Position, PlayerEnt.Camera.Rotation);
		}

		public override void DrawOpaque() {
			/*if (MenuWallpaperTex != null) {
				ShaderUniforms.Current.Camera = Engine.Camera2D;
				RenderState RS = Gfx.PeekRenderState();
				RS.EnableDepthMask = false;
				Gfx.PushRenderState(RS);

				Gfx.TexturedRectangle(0, 0, Engine.WindowWidth, Engine.WindowHeight, Texture: MenuWallpaperTex);

				Gfx.PopRenderState();
			}*/

			ShaderUniforms.Current.Camera = Engine.Camera3D;
			/*if (MenuModel != null) {
				MenuModel.Rotation = Quaternion.CreateFromYawPitchRoll(Engine.Time / 4, -(float)Math.PI / 2, 0);
				MenuModel.Position = new Vector3(7, -10, -25);
				MenuModel.DrawOpaque();
				MenuModel.DrawTransparent();
			}*/

			//Gfx.ClearDepth();

			VoxelMap.Draw();
			PlayerEnt?.DrawViewModel();
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
