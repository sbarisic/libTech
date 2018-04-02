using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using libTech.Graphics;

namespace libTech.Entities {
	public class Player : Entity {
		public Camera ViewCamera { get; private set; }
		public override Vector3 Position { get => ViewCamera.Position; set => ViewCamera.Position = value; }

		public Player() {
			ViewCamera = new Camera();
			ViewCamera.SetPerspective(Engine.Width, Engine.Height, 90 * 3.1415926535f / 180);
			ViewCamera.MouseMovement = true;
		}

		public override void Update(float Dt) {
			if (!GConsole.Open) {
				Engine.CaptureMouse(true);
				ViewCamera.Update(Engine.MouseDelta);

				const float Scale = 100;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.W))
					ViewCamera.Position += ViewCamera.WorldForwardNormal * Dt * Scale;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.A))
					ViewCamera.Position -= ViewCamera.WorldRightNormal * Dt * Scale;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.S))
					ViewCamera.Position -= ViewCamera.WorldForwardNormal * Dt * Scale;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.D))
					ViewCamera.Position += ViewCamera.WorldRightNormal * Dt * Scale;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.Space))
					ViewCamera.Position += ViewCamera.WorldUpNormal * Dt * Scale;

				if (Engine.GetKey(Glfw3.Glfw.KeyCode.LeftControl))
					ViewCamera.Position -= ViewCamera.WorldUpNormal * Dt * Scale;
			} else
				Engine.CaptureMouse(false);
		}
	}
}
