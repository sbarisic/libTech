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
using System.Threading;

namespace Game {
	public unsafe class Game : LibTechGame {
		static float NearClip = 0.0001f;

		Window MainMenuWindow;
		int BtnNum;

		ShaderProgram LegShader;

		ConVar<int> Sparse;
		ConVar<float> LegLength;
		ConVar<float> LegWidth;
		ConVar<float> PickDistance;
		ConVar<float> PickSize;
		ConVar<int> PickSampleNum;

		Mesh3D VertsMesh;

		Vertex3[] PointCloudVerts;
		Vertex3[] ProcessedVerts = new Vertex3[] { new Vertex3() };
		int ProcessedCount;

		object VertsMeshLock = new object();

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
			AddButton("Stop", RealSenseCamera.Stop);
			AddButton("Start", RealSenseCamera.Start);

			Engine.Camera3D.SetPerspective(Engine.Window.WindowSize, (90.0f).ToRad(), NearClip, 100);

			LegShader = new ShaderProgram(new ShaderStage(ShaderType.VertexShader, "content/shaders/leg.vert"),
				new ShaderStage(ShaderType.FragmentShader, "content/shaders/leg.frag"));

			VertsMesh = new Mesh3D();
			VertsMesh.PrimitiveType = PrimitiveType.Points;
			VertsMesh.SetVertices(new Vertex3());

			// Variables
			Sparse = ConVar.Register(nameof(Sparse).ToLower(), 0); // 3
			LegLength = ConVar.Register(nameof(LegLength).ToLower(), 100.0f); // 1.0f
			LegWidth = ConVar.Register(nameof(LegWidth).ToLower(), 100.0f); // 0.5f		
			PickDistance = ConVar.Register(nameof(PickDistance).ToLower(), 1.0f);
			PickSize = ConVar.Register(nameof(PickSize).ToLower(), 0.01f); // 0.025f
			PickSampleNum = ConVar.Register(nameof(PickSampleNum).ToLower(), 10); // 10

			ConCmd.Register("list", (Argv) => {
				GConsole.WriteLine(Sparse);
				GConsole.WriteLine(LegLength);
				GConsole.WriteLine(LegWidth);
				GConsole.WriteLine(PickDistance);
				GConsole.WriteLine(PickSize);
				GConsole.WriteLine(PickSampleNum);
			});

			Thread PollingThread = new Thread(() => {
				while (true) {
					RealSenseCamera.PollForFrames(OnPointCloud: OnPointCloud);
					Thread.Sleep(0);
				}
			});
			PollingThread.IsBackground = true;
			PollingThread.Start();

			Engine.Camera3D.Position = new Vector3(0, 0, -1);
			Engine.Camera3D.LookAt(Vector3.Zero);
			Engine.Camera3D.Position = new Vector3(0, 0, -NearClip);
		}

		Vector3 CursorWorldPos;
		int CollisionSamples;

		public override void Draw(float Dt) {
			ShaderUniforms.Camera = Engine.Camera3D;
			ShaderUniforms.Model = Matrix4x4.CreateRotationZ((float)-Math.PI);
			Gfx.EnableDepthDest(false);
			Gfx.EnableCullFace(false);

			lock (VertsMeshLock) {
				VertsMesh.SetVertices(ProcessedVerts, ProcessedCount);
				// VertsMesh.SetVertices(ProcessedCount, ProcessedVerts);
			}

			LegShader.Bind();
			VertsMesh.Draw();
			LegShader.Unbind();

			//////// 

			Vector2 MousePos = (Engine.Window.WindowSize.GetHeight() - Engine.Window.MousePos) * new Vector2(-1, 1);
			CursorWorldPos = Engine.Camera3D.ScreenToWorldDirection(MousePos, ShaderUniforms.Model) * PickDistance.Value;

			ShaderUniforms.Camera = Engine.Camera2D;
			ShaderUniforms.Model = Matrix4x4.Identity;
			Gfx.Point(new Vertex2(MousePos, (CollisionSamples > PickSampleNum.Value) ? Color.Red : Color.Green), 5);
		}

		Vertex3[] OnPointCloud(int Count, Vertex3[] Verts, FrameData[] Frames) {
			if (Verts == null && Frames == null) {
				if (PointCloudVerts == null || PointCloudVerts.Length < Count) {
					PointCloudVerts = new Vertex3[Count];
					ProcessedVerts = new Vertex3[Count];
				}

				return PointCloudVerts;
			}

			lock (VertsMeshLock) {
				ProcessedCount = 0;
				CollisionSamples = 0;

				Parallel.For(0, Count, (i) => {
					// Discard points that are too close to the camera
					if (Verts[i].Position.Z == 0)
						return;

					int SparseValue = Sparse.Value;

					if (SparseValue != 0) {
						Vector3 XYZ = Verts[i].Position * 1000;

						if (((int)(XYZ.X)) % SparseValue != 0)
							return;

						if (((int)(XYZ.Y)) % SparseValue != 0)
							return;

						if (((int)(XYZ.Z)) % SparseValue != 0)
							return;
					}

					float LegWidthHalf = LegWidth.Value / 2;
					if (Verts[i].Position.X < -LegWidthHalf || Verts[i].Position.X > LegWidthHalf)
						return;

					if (Verts[i].Position.LengthSquared() > (LegLength.Value * LegLength.Value))
						return;

					Color Clr;

					if (Vector2.DistanceSquared(Vector3.Normalize(CursorWorldPos).XY(), Vector3.Normalize(Verts[i].Position).XY()) < PickSize.Value * PickSize.Value) {
						Clr = Color.Blue;

						if (Vector3.DistanceSquared(Vector3.Zero, CursorWorldPos) > Vector3.DistanceSquared(Vector3.Zero, Verts[i].Position)) {
							Interlocked.Add(ref CollisionSamples, 1);
							Clr = Color.Red;
						}
					} else
						Clr = Frames[1].GetPixel(Verts[i].UV, false);

					ProcessedVerts[ProcessedCount] = new Vertex3(Verts[i], Clr);
					Interlocked.Add(ref ProcessedCount, 1);
				});
			}

			return null;
		}
	}
}