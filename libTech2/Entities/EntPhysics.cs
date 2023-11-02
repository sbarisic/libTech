using FishGfx;
using libTech.Map;
using libTech.Materials;
using libTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using libTech.Physics;
using BepuPhysics;

namespace libTech.Entities {
	[EntityClassName("prop_physics")]
	public class EntPhysics : Entity {
		public libTechModel RenderModel;

		Vector3 UpdatedScale;
		Quaternion UpdatedRotation;
		Vector3 UpdatedPosition;

		PhysEngine PhysEng;
		PhysShape PhysShape;
		PhysBodyDescription RigidBody;

		//Quaternion InitialRotation;
		//Vector3 InitialPosition;

		//Matrix4x4 _WorldTransform;

		void Init(PhysEngine PhysEng, PhysShape PhysShape, float Mass = 10) {
			this.PhysEng = PhysEng;
			this.PhysShape = PhysShape;


			RigidBody = new PhysBodyDescription(PhysEng, PhysShape, Mass);
		}

		void Init(PhysEngine Engine, IEnumerable<Vector3> Vertices, float Mass = 10) {
			Init(Engine, PhysShape.FromVertices(Engine, Vertices), Mass);
		}

		void Init(PhysEngine Engine, libTechModel Model, float Mass = 10) {
			Init(Engine, Model.GetMeshes().First().GetVertices().Select(V => V.Position), Mass);
			RenderModel = Model;
		}

		public EntPhysics(PhysEngine Engine, PhysShape Shape, float Mass = 10) {
			Init(Engine, Shape, Mass);
		}

		public EntPhysics(PhysEngine Engine, IEnumerable<Vector3> Vertices, float Mass = 10) {
			Init(Engine, Vertices, Mass);
		}

		public EntPhysics(PhysEngine Engine, libTechModel Model, float Mass = 10) {
			Init(Engine, Model, Mass);
		}

		public EntPhysics(EntityKeyValues KVs) {
			string ModelName = KVs.Get<string>("model");
			libTechModel Model = KVs.Map.LoadModel(ModelName);

			// TODO
			/*if (ModelName.Contains("oil"))
				Debugger.Break();*/

			Quaternion InitialRotation = KVs.Get<Quaternion>("qangles");
			Vector3 InitialPosition = KVs.Get<Vector3>("origin");

			Init(KVs.Map.PhysicsEngine, Model, 18);
			SetWorldTransform(Vector3.One, InitialRotation, InitialPosition);
		}

		public override void Spawned() {
			//Map.World.AddRigidBody(RigidBody);

			Map.PhysicsEngine.AddShape(PhysShape);
			Map.PhysicsEngine.AddBody(RigidBody);
		}

		public virtual void GetWorldTransform(out Vector3 Scale, out Quaternion Rotation, out Vector3 Position) {
			RigidBody.GetWorldTransform(PhysEng, out Scale, out Rotation, out Position);

			//Matrix4x4.Decompose(WorldTransform, out Scale, out Rotation, out Position);
		}

		public virtual void SetWorldTransform(Vector3 Scale, Quaternion Rotation, Vector3 Position) {
			RigidBody.SetWorldTransform(PhysEng, Scale, Rotation, Position);

			//WorldTransform = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
		}

		public void SetPosition(Vector3 Pos) {
			GetWorldTransform(out Vector3 Scale, out Quaternion Rot, out Vector3 _);
			SetWorldTransform(Scale, Rot, Pos);
		}

		public virtual void DisableSleep() {
			//RigidBody.ActivationState = ActivationState.DisableDeactivation;
		}

		public virtual void Freeze() {
			//RigidBody.LinearFactor = Vector3.Zero;
			//RigidBody.AngularFactor = Vector3.Zero;
			//RigidBody.LinearVelocity = Vector3.Zero;
			//RigidBody.AngularVelocity = Vector3.Zero;
		}

		public virtual void Unfreeze() {
			//RigidBody.LinearFactor = Vector3.One;
			//RigidBody.AngularFactor = Vector3.One;

			//RigidBody.ForceActivationState(ActivationState.ActiveTag);
			//RigidBody.DeactivationTime = 0;
		}

		public override void Update(float Dt) {
			GetWorldTransform(out UpdatedScale, out UpdatedRotation, out UpdatedPosition);
		}

		void SetRenderModelData() {
			RenderModel.Position = UpdatedPosition;
			RenderModel.Rotation = UpdatedRotation;
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

		public override void DrawShadowVolume(BoundSphere Light, ShaderMaterial ShadowVolume) {
			if (RenderModel != null) {
				if (Light.Collide(RenderModel.BoundingSphere + UpdatedPosition)) {
					SetRenderModelData();
					RenderModel.DrawShadowVolume(ShadowVolume);
				}
			}
		}
	}
}
