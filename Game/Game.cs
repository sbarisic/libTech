using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using libTech;
using libTech.UI;
using libTech.Graphics;
using OpenGL;
using System.Numerics;
using Matrix4 = System.Numerics.Matrix4x4;
using libTech.Importer;
using libTechGeometry;
using libTechGeometry.ConstructiveSolidGeometry;
using libTech.libNative;
using System.Runtime.InteropServices;
using System.Diagnostics;
using libTech.Entities;

namespace Game {
	public unsafe class Game : LibTechGame {
		Texture CrosshairTex;
		//ShaderProgram DefaultShader, MSDF;

		MSDFFont FontTest;

		World GameWorld;
		Player Ply;
		
		public override void Load() {
			FontTest = new MSDFFont("content/fonts/Hack.ttf");
			CrosshairTex = Importers.Load<Texture>("content/textures/crosshair_default.png");
			
			/*MSDF = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/default.vert"),
			   new ShaderStage(ShaderType.FragmentShader, "content/shaders/msdf.frag"));*/
			   
			Ply = new Player();
			Engine.SpawnEntity(Ply);

			GameWorld = new World("content/maps/sandbox.txt");
			Ply.Position = GameWorld.RelativeSpawns.FirstOrDefault();
			Engine.SpawnEntity(GameWorld);
			
			GConsole.RegisterCommand("getcam", () => {
				GConsole.WriteLine("Pos: " + Ply.Position.ToString());
			});
		}

		public override void Update(float Dt) {
			Camera.ActiveCamera = Ply.ViewCamera;
		}

		public override void Draw(float Dt) {
			/*Camera.ActiveCamera = Ply.ViewCamera;
			GameWorld.Draw(Dt);

			Gl.Disable(EnableCap.CullFace);
			Gl.DepthMask(false);
			GameWorld.DrawTransparent(Dt);
			Gl.DepthMask(true);*/

			/*if (GizmoEnabled) {
				Gl.Clear(ClearBufferMask.DepthBufferBit);
				Immediate.Axes(Vector3.Zero, 100);

				Immediate.GizmoInput(Engine.WindowSize / 2, Engine.GetMouseButton(Glfw3.Glfw.MouseButton.ButtonLeft), Engine.GetKey(Glfw3.Glfw.KeyCode.T),
					Engine.GetKey(Glfw3.Glfw.KeyCode.R), Engine.GetKey(Glfw3.Glfw.KeyCode.Y), Engine.GetKey(Glfw3.Glfw.KeyCode.L), Engine.GetKey(Glfw3.Glfw.KeyCode.LeftShift));

				if (Immediate.Gizmo(Dt, ref Pos, ref Rot, ref Scl))
					Mat = Camera.CreateModel(Pos, Scl, Rot);
			}*/
		}

		public override void DrawTransparent(float Dt) {

		}

		public override void DrawGUI(float Dt) {
			Immediate.UseShaders(() => {
				Immediate.TriangleShader = ShaderProgram.Default;
				Immediate.Texture2D(Engine.WindowSize / 2, CrosshairTex, true);

				/*Immediate.TriangleShader = MSDF;
				Gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.SrcColor, BlendingFactor.One);
				Gl.BlendFuncSeparate(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha, BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);*/
			});
		}
	}
}