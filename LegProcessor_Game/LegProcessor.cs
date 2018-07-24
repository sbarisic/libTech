using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Diagnostics;
using libTech;
using libTech.Entities;
using libTech.Importer;
using libTech.libNative;
using libTech.GUI;
using libTech.Graphics;

using FishGfx;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using FishGfx.RealSense;

namespace Game {
	public unsafe class Game : LibTechGame {
		Window MainMenuWindow;
		int BtnNum;

		void AddButton(string Name, Action OnClick) {
			if (MainMenuWindow == null) {
				BtnNum = 0;
				MainMenuWindow = Engine.GUI.AddChild(new Window());
				MainMenuWindow.Resizable = false;
				MainMenuWindow.Position = new Vector2(20, 20);
			}

			const float Padding = 5;
			const float ButtonHeight = 30;
			TextButton Btn = MainMenuWindow.AddChild(new TextButton(DefaultFonts.MainMenuMedium, Name, ButtonHeight));

			Btn.Position = new Vector2(Padding, (ButtonHeight * BtnNum) + (Padding * (BtnNum + 1)));
			Btn.OnMouseClick += (K, P) => OnClick();
			BtnNum++;

			MainMenuWindow.AutoResize(new Vector2(Padding));
			//MainMenuWindow.Center(Engine.Window.WindowSize / 2);
		}

		public override void Load() {
			AddButton("Exit", () => Engine.CreateYesNoPrompt("Exit Program?", () => Environment.Exit(0)).Center((Engine.Window.WindowSize / 2)));
			AddButton("Camera Stop", RealSenseCamera.Stop);
			AddButton("Camera Start", RealSenseCamera.Start);
		}

		public override void Draw(float Dt) {
			ShaderUniforms.Model = Matrix4x4.Identity;
			Gfx.EnableDepthDest(false);
			Gfx.EnableCullFace(false);

			ShaderUniforms.Camera = Engine.Camera2D;
			Draw2D();

			ShaderUniforms.Camera = Engine.Camera3D;
			Draw3D();

		}

		void Draw2D() {
			Gfx.Point(new Vertex2(200, 400));

			Gfx.Point(new Vertex2(400, 400));

			Gfx.Point(new Vertex2(600, 400));
		}

		void Draw3D() {
			/*Gfx.Point(new Vertex3(-0.5f, 0, -1));

			Gfx.Point(new Vertex3(0, 0, -2));

			Gfx.Point(new Vertex3(0.5f, 0, -3));*/
		}
	}
}