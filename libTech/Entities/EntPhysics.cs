using BulletSharp;
using libTech.Map;
using libTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Entities {
	public class EntPhysics : Entity {
		public static EntPhysics FromModel(libTechModel Model, float Mass) {
			EntPhysics PhysEnt = new EntPhysics(Model.Meshes.First().GetVertices().Select(V => V.Position), Mass);
			PhysEnt.RenderModel = Model;
			return PhysEnt;
		}

		public libTechRigidBody RigidBody;
		public libTechModel RenderModel;

		public bool IsStatic { get; private set; }

		Vector3 UpdatedScale;
		Quaternion UpdatedRotation;
		Vector3 UpdatedPosition;

		public EntPhysics(libTechCollisionShape Shape, float Mass = 10) {
			RigidBody = libTechRigidBody.CreateRigidBody(Mass, Matrix4x4.Identity, Shape);
			RigidBody.Body.UserObject = this;
			IsStatic = Mass == 0;
		}

		public EntPhysics(IEnumerable<Vector3> Vertices, float Mass = 10) : this(libTechCollisionShape.FromVertices(Vertices), Mass) {
		}

		public override void Spawned() {
			Map.World.AddRigidBody(RigidBody.Body);
		}

		public virtual void GetWorldTransform(out Vector3 Scale, out Quaternion Rotation, out Vector3 Position) {
			Matrix4x4.Decompose(RigidBody.Body.WorldTransform, out Scale, out Rotation, out Position);
		}

		public virtual void SetWorldTransform(Vector3 Scale, Quaternion Rotation, Vector3 Position) {
			RigidBody.Body.WorldTransform = Matrix4x4.CreateTranslation(Position) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale);
		}

		public void SetPosition(Vector3 Pos) {
			GetWorldTransform(out Vector3 Scale, out Quaternion Rot, out Vector3 _);
			SetWorldTransform(Scale, Rot, Pos);
		}

		public override void Update(float Dt) {
			base.Update(Dt);
			GetWorldTransform(out UpdatedScale, out UpdatedRotation, out UpdatedPosition);
		}

		void SetRenderModelData() {
			RenderModel.Position = UpdatedPosition;
			RenderModel.Rotation = Matrix4x4.CreateFromQuaternion(UpdatedRotation);
		}

		public override void DrawOpaque() {
			if (RenderModel != null) {
				SetRenderModelData();
				RenderModel.DrawOpaque();
			}
		}

		public override void DrawTransparent() {
			if (RenderModel != null) {
				SetRenderModelData();
				RenderModel.DrawTransparent();
			}
		}
	}
}
