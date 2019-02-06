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
	public class libTechCollisionShape {
		internal CollisionShape CollisionShape;

		internal libTechCollisionShape(CollisionShape CollisionShape) {
			this.CollisionShape = CollisionShape;
		}

		public static libTechCollisionShape FromVertices(IEnumerable<Vector3> Verts) {
			ConvexHullShape HullShape = new ConvexHullShape(Verts);
			HullShape.InitializePolyhedralFeatures();
			return new libTechCollisionShape(HullShape);
		}

		public static libTechCollisionShape FromVerticesConcave(IEnumerable<Vector3> Verts) {
			Vector3[] VertsArray = Verts.ToArray();
			TriangleMesh TriMesh = new TriangleMesh();

			for (int i = 0; i < VertsArray.Length; i += 3)
				TriMesh.AddTriangle(VertsArray[i + 0], VertsArray[i + 1], VertsArray[i + 2]);

			BvhTriangleMeshShape TriShape = new BvhTriangleMeshShape(TriMesh, false);
			return new libTechCollisionShape(TriShape);
		}

		public static libTechCollisionShape CreateBoxShape(float X, float Y, float Z) {
			BoxShape Box = new BoxShape(X / 2, Y / 2, Z / 2);
			return new libTechCollisionShape(Box);
		}
	}

	public class EntPhysics : Entity {
		public static EntPhysics FromModel(libTechModel Model, float Mass) {
			EntPhysics PhysEnt = new EntPhysics(Model.Meshes.First().GetVertices().Select(V => V.Position), Mass);
			PhysEnt.RenderModel = Model;
			return PhysEnt;
		}

		internal RigidBody RigidBody;
		public libTechModel RenderModel;

		public bool IsStatic { get; private set; }

		Vector3 UpdatedScale;
		Quaternion UpdatedRotation;
		Vector3 UpdatedPosition;

		public EntPhysics(libTechCollisionShape Shape, float Mass = 10) {
			bool IsDynamic = Mass != 0;

			Vector3 LocalInertia = Vector3.Zero;
			if (IsDynamic)
				Shape.CollisionShape.CalculateLocalInertia(Mass, out LocalInertia);

			DefaultMotionState MotionState = new DefaultMotionState(Matrix4x4.Identity);
			RigidBody Body = null;

			using (RigidBodyConstructionInfo RBInfo = new RigidBodyConstructionInfo(Mass, MotionState, Shape.CollisionShape, LocalInertia))
				Body = new RigidBody(RBInfo);

			RigidBody = Body;
			RigidBody.UserObject = this;

			IsStatic = Mass == 0;
		}

		public EntPhysics(IEnumerable<Vector3> Vertices, float Mass = 10) : this(libTechCollisionShape.FromVertices(Vertices), Mass) {
		}

		public override void Spawned() {
			Map.World.AddRigidBody(RigidBody);
		}

		public virtual void GetWorldTransform(out Vector3 Scale, out Quaternion Rotation, out Vector3 Position) {
			Matrix4x4.Decompose(RigidBody.WorldTransform, out Scale, out Rotation, out Position);
		}

		public virtual void SetWorldTransform(Vector3 Scale, Quaternion Rotation, Vector3 Position) {
			RigidBody.WorldTransform = Matrix4x4.CreateTranslation(Position) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale);
		}

		public void SetPosition(Vector3 Pos) {
			GetWorldTransform(out Vector3 Scale, out Quaternion Rot, out Vector3 _);
			SetWorldTransform(Scale, Rot, Pos);
		}

		public virtual void DisableSleep() {
			RigidBody.ActivationState = ActivationState.DisableDeactivation;
		}

		public virtual void Freeze() {
			RigidBody.LinearFactor = Vector3.Zero;
			RigidBody.AngularFactor = Vector3.Zero;
			RigidBody.LinearVelocity = Vector3.Zero;
			RigidBody.AngularVelocity = Vector3.Zero;
		}

		public virtual void Unfreeze() {
			RigidBody.LinearFactor = Vector3.One;
			RigidBody.AngularFactor = Vector3.One;

			RigidBody.ForceActivationState(ActivationState.ActiveTag);
			RigidBody.DeactivationTime = 0;
		}

		public override void Update(float Dt) {
			base.Update(Dt);
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
	}
}
