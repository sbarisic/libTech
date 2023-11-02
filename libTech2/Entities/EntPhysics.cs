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

namespace libTech.Entities {
	[EntityClassName("prop_physics")]
	public class EntPhysics : Entity {
		public libTechModel RenderModel;

		public bool IsStatic {
			get; private set;
		}

		Vector3 UpdatedScale;
		Quaternion UpdatedRotation;
		Vector3 UpdatedPosition;
		
		internal PhysBodyDescription RigidBody;

		Matrix4x4 WorldTransform;

		void Init(PhysShape Shape, float Mass = 10) {
			/*bool IsDynamic = Mass != 0;

			Vector3 LocalInertia = Vector3.Zero;
			if (IsDynamic)
				Shape.CollisionShape.CalculateLocalInertia(Mass, out LocalInertia);

			DefaultMotionState MotionState = new DefaultMotionState(Matrix4x4.Identity);
			RigidBody Body = null;

			using (RigidBodyConstructionInfo RBInfo = new RigidBodyConstructionInfo(Mass, MotionState, Shape.CollisionShape, LocalInertia))
				Body = new RigidBody(RBInfo);

			RigidBody = Body;
			RigidBody.UserObject = this;
			RigidBody.SetAnisotropicFriction(Shape.CollisionShape.AnisotropicRollingFrictionDirection, AnisotropicFrictionFlags.AnisotropicRollingFriction);
			RigidBody.Friction = 0.75f;
			// RigidBody.CenterOfMassTransform

			//RigidBody.CenterOfMassTransform = Matrix4x4.CreateTranslation(-Shape.CenterOfVertices);



			// TODO: What are these for
			RigidBody.CcdMotionThreshold = 1e-7f;
			//RigidBody.CcdSweptSphereRadius = 0.9f;

			IsStatic = Mass == 0;*/

			IsStatic = true;
		}

		void Init(IEnumerable<Vector3> Vertices, float Mass = 10) {
			Init(PhysShape.FromVertices(Vertices), Mass);
		}

		void Init(libTechModel Model, float Mass = 10) {
			Init(Model.GetMeshes().First().GetVertices().Select(V => V.Position), Mass);
			RenderModel = Model;
		}

		public EntPhysics(PhysShape Shape, float Mass = 10) {
			Init(Shape, Mass);
		}

		public EntPhysics(IEnumerable<Vector3> Vertices, float Mass = 10) {
			Init(Vertices, Mass);
		}

		public EntPhysics(libTechModel Model, float Mass = 10) {
			Init(Model, Mass);
		}

		public EntPhysics(EntityKeyValues KVs) {
			string ModelName = KVs.Get<string>("model");
			libTechModel Model = KVs.Map.LoadModel(ModelName);

			// TODO
			/*if (ModelName.Contains("oil"))
				Debugger.Break();*/

			Init(Model, 18);
			SetWorldTransform(Vector3.One, KVs.Get<Quaternion>("qangles"), KVs.Get<Vector3>("origin"));
		}

		public override void Spawned() {
			//Map.World.AddRigidBody(RigidBody);

			Map.PhysicsEngine.AddBody(RigidBody);
		}

		public virtual void GetWorldTransform(out Vector3 Scale, out Quaternion Rotation, out Vector3 Position) {			
			Matrix4x4.Decompose(WorldTransform, out Scale, out Rotation, out Position);
		}

		public virtual void SetWorldTransform(Vector3 Scale, Quaternion Rotation, Vector3 Position) {
			WorldTransform = Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
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
