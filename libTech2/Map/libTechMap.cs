﻿//using BulletSharp;

using FishGfx;
using FishGfx.Graphics;

using libTech;
using libTech.Entities;
using libTech.Graphics;
using libTech.Materials;
using libTech.Models;
using libTech.Physics;

using SharpFileSystem.FileSystems;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Map {
	public class libTechMap {
		const bool SkyboxEnabled = true;

		libTechModel[] MapModels;
		Entity[] Entities;
		DynamicLight[] Lights;

		// Physics
		/*internal DynamicsWorld World;
		internal CollisionConfiguration CollisionConf;
		internal CollisionDispatcher Dispatcher;
		internal BroadphaseInterface Broadphase;
		internal ConstraintSolver Solver;
		internal ClosestConvexResultCallback ClosestConvexResult;*/

		public PhysEngine PhysicsEngine;


		internal Dictionary<string, libTechModel> LoadedModels;
		internal libTechModel SkyboxModel;

		/*public Vector3 Gravity {
			get {
				return World.Gravity;
			}

			set {
				World.Gravity = value;
			}
		}*/

		public libTechMap() {
			MapModels = new libTechModel[] { };
			Entities = new Entity[] { };
			Lights = new DynamicLight[] { };

			LoadedModels = new Dictionary<string, libTechModel>();
			PhysicsEngine = new PhysEngine();
		}

		public void InitPhysics() {
			/*CollisionConf = new DefaultCollisionConfiguration();
			Dispatcher = new CollisionDispatcher(CollisionConf);

			Broadphase = new DbvtBroadphase();
			Broadphase.OverlappingPairCache.SetInternalGhostPairCallback(new GhostPairCallback());

			Solver = new SequentialImpulseConstraintSolver();

			World = new DiscreteDynamicsWorld(Dispatcher, Broadphase, Solver, CollisionConf);
			World.SolverInfo.SolverMode |= SolverModes.Use2FrictionDirections | SolverModes.RandomizeOrder;

			World.SolverInfo.NumIterations = 20;
			World.DispatchInfo.AllowedCcdPenetration = 0.0001f;
			World.DispatchInfo.UseContinuous = true;
			World.Gravity = new Vector3(0, -600, 0);
			//World.DebugDrawer = new DbgDrawPhysics();

			ClosestConvexResult = new ClosestConvexResultCallback();*/



			libTechModel[] Models = GetModels().ToArray();

			if (Models.Length > 0) {
				IEnumerable<Vector3> MapVerts = Models.First().GetMeshes().SelectMany(M => M.GetVertices().Select(V => V.Position));

				EntPhysics MapPhysics = new EntPhysics(PhysicsEngine, PhysShape.FromVerticesConcave(PhysicsEngine, MapVerts), 0);
				SpawnEntity(MapPhysics);
			}
		}

		public void AddModel(libTechModel Model) {
			Array.Resize(ref MapModels, MapModels.Length + 1);
			MapModels[MapModels.Length - 1] = Model;
			Model.SetLabel(string.Format("Map model {0}", MapModels.Length - 1));

			/*foreach (var Msh in Model.GetMeshes())
				if (Msh.Material.MaterialName == "water")
					Msh.SetWireframe(true);*/
		}

		public libTechModel LoadModel(string Pth) {
			if (LoadedModels.ContainsKey(Pth))
				return LoadedModels[Pth];

			libTechModel Mdl = Engine.Load<libTechModel>(Pth);

			if (Mdl == null)
				throw new Exception("Could not load model " + Pth);

			Mdl.SetLabel(Pth);
			LoadedModels.Add(Pth, Mdl);
			return Mdl;
		}

		public AABB CalculateAABB() {
			AABB Bounds = MapModels[0].BoundingBox;

			for (int ModelIdx = 1; ModelIdx < MapModels.Length; ModelIdx++) {
				Vector3[] Verts = new Vector3[16];
				Array.Copy(Bounds.GetVertices().ToArray(), 0, Verts, 0, 8);
				Array.Copy(MapModels[ModelIdx].BoundingBox.GetVertices().ToArray(), 0, Verts, 8, 8);
				Bounds = AABB.CalculateAABB(Verts);
			}

			return Bounds;
		}

		/*public bool RayCast(Vector3 From, Vector3 To, out Vector3 HitPos, out Vector3 HitNormal, out RigidBody Body) {
			using (ClosestRayResultCallback RayResult = new ClosestRayResultCallback(ref From, ref To)) {
				World.RayTestRef(ref From, ref To, RayResult);

				if (RayResult.HasHit) {
					HitPos = RayResult.HitPointWorld;
					HitNormal = RayResult.HitNormalWorld;
					Body = RayResult.CollisionObject as RigidBody;
					return true;
				}
			}

			Body = null;
			HitPos = HitNormal = Vector3.Zero;
			return false;
		}

		public bool RayCast(Vector3 From, Vector3 To) {
			return RayCast(From, To, out Vector3 HitPos, out Vector3 HitNormal, out RigidBody Body);
		}

		public float RayCastDistance(Vector3 From, Vector3 To) {
			if (RayCast(From, To, out Vector3 HitPos, out Vector3 HitNormal, out RigidBody Body))
				return Vector3.Distance(From, HitPos);

			return Vector3.Distance(From, To);
		}

		public RigidBody RayCastBody(Vector3 From, Vector3 To, out Vector3 PickPoint) {
			RayCast(From, To, out PickPoint, out Vector3 HitNormal, out RigidBody Body);
			return Body;
		}

		public SweepResult SweepTest(ConvexShape Shape, Vector3 From, Vector3 To, CollisionFilterGroups Filter = CollisionFilterGroups.None) {
			ClosestConvexResult.ClosestHitFraction = 1.0f;
			ClosestConvexResult.ConvexFromWorld = From;
			ClosestConvexResult.ConvexToWorld = To;
			ClosestConvexResult.CollisionFilterMask = ~Filter;
			World.ConvexSweepTest(Shape, Matrix4x4.CreateTranslation(From), Matrix4x4.CreateTranslation(To), ClosestConvexResult);

			SweepResult Result = new SweepResult();
			Result.SweepFrom = From;
			Result.HasHit = ClosestConvexResult.HasHit;
			Result.Fraction = 1;
			Result.HitPoint = Result.HitCenterOfMass = To;

			if (ClosestConvexResult.HasHit) {
				Result.Fraction = ClosestConvexResult.ClosestHitFraction;
				Result.Normal = Vector3.Normalize(ClosestConvexResult.HitNormalWorld);

				Result.HitPoint = ClosestConvexResult.HitPointWorld;
				Result.HitCenterOfMass = Vector3.Lerp(From, To, Result.Fraction);

				Result.Distance = Vector3.Distance(From, Result.HitCenterOfMass);
			} else
				Result.Distance = Vector3.Distance(From, To);

			return Result;
		}

		public ContactResult ContactTest(RigidBody Body) {
			ContactResult Result = new ContactResult(Body);
			World.ContactTest(Body, Result);
			return Result;
		}

		public SweepResult SweepTest(ConvexShape Shape, Vector3 Pos, Vector3 Normal, float Dist, CollisionFilterGroups Filter = CollisionFilterGroups.None) {
			return SweepTest(Shape, Pos, Pos + Normal * Dist, Filter);
		}*/

		void InitEntity(Entity Ent) {
			Ent.Map = this;

			Ent.Spawned();
			Ent.HasSpawned = true;
		}

		public void SpawnEntity(Entity Ent) {
			Ent.HasSpawned = false;

			if (Ent is DynamicLight L)
				Utils.Insert(ref Lights, L);

			Utils.Insert(ref Entities, Ent);
		}

		public void RemoveEntity(Entity Ent) {
			Utils.Remove(ref Entities, Ent);

			if (Ent is DynamicLight L)
				Utils.Remove(ref Lights, L);
		}

		public IEnumerable<libTechModel> GetModels() {
			for (int ModelIdx = 0; ModelIdx < MapModels.Length; ModelIdx++)
				yield return MapModels[ModelIdx];
		}

		public IEnumerable<Entity> GetEntities() {
			for (int EntityIdx = 0; EntityIdx < Entities.Length; EntityIdx++)
				if (Entities[EntityIdx] != null)
					yield return Entities[EntityIdx];
		}

		public IEnumerable<T> GetEntities<T>() where T : Entity {
			for (int EntityIdx = 0; EntityIdx < Entities.Length; EntityIdx++)
				if (Entities[EntityIdx] != null && Entities[EntityIdx].GetType() == typeof(T))
					yield return (T)Entities[EntityIdx];
		}

		public DynamicLight[] GetLights() {
			return Lights;
		}

		public void Update(float Dt) {
			//World.StepSimulation(Dt);
			PhysicsEngine.Timestep(Dt);

			for (int i = 0; i < Entities.Length; i++) {
				if (!Entities[i].HasSpawned)
					InitEntity(Entities[i]);

				Entities[i].Update(Dt);
			}
		}

		public void DrawSkybox() {
			if (!SkyboxEnabled)
				return;

			if (SkyboxModel == null) {
				SkyboxModel = new libTechModel();
				SkyboxModel.Scale = new Vector3(5000);

				libTechMesh CubeMesh = new libTechMesh(FishGfx.Formats.Obj.Load("content/models/cube.obj").First().Vertices.ToArray(), Engine.GetMaterial("skybox"));
				CubeMesh.SetLabel("Skybox Cube");

				SkyboxModel.AddMesh(CubeMesh);
			}

			RenderState RS = Gfx.PeekRenderState();
			RS.EnableCullFace = false;
			RS.EnableDepthMask = false;
			RS.EnableDepthTest = true;
			RS.EnableBlend = false;
			Gfx.PushRenderState(RS);

			SkyboxModel.Position = Engine.Camera3D.Position;
			SkyboxModel.DrawOpaque();

			Gfx.PopRenderState();
		}

		public void DrawOpaque() {
			DrawDebugPhysics();

			RenderAPI.DbgPushGroup("Map DrawOpaque");

			for (int ModelIdx = 0; ModelIdx < MapModels.Length; ModelIdx++)
				MapModels[ModelIdx].DrawOpaque();

			RenderAPI.DbgPopGroup();

			RenderAPI.DbgPushGroup("Entity DrawOpaque");

			for (int EntityIdx = 0; EntityIdx < Entities.Length; EntityIdx++)
				Entities[EntityIdx]?.DrawOpaque();

			RenderAPI.DbgPopGroup();
		}

		public void DrawDebugPhysics() {
			RenderAPI.DbgPushGroup("Debug Physics");

			//World.DebugDrawWorld();
			PhysicsEngine.DebugDraw();

			RenderAPI.DbgPopGroup();
		}

		public void DrawTransparent() {
			RenderAPI.DbgPushGroup("Map DrawTransparent");

			for (int ModelIdx = 0; ModelIdx < MapModels.Length; ModelIdx++)
				MapModels[ModelIdx].DrawTransparent();

			RenderAPI.DbgPopGroup();

			RenderAPI.DbgPushGroup("Entity DrawTransparent");

			for (int EntityIdx = 0; EntityIdx < Entities.Length; EntityIdx++)
				Entities[EntityIdx]?.DrawTransparent();

			RenderAPI.DbgPopGroup();
		}

		public void DrawShadowVolume(ShaderMaterial ShadowVolume) {
			RenderAPI.DbgPushGroup("Map DrawShadowVolume");

			for (int ModelIdx = 0; ModelIdx < MapModels.Length; ModelIdx++)
				MapModels[ModelIdx].DrawShadowVolume(ShadowVolume);

			RenderAPI.DbgPopGroup();
		}

		public void DrawEntityShadowVolume(DynamicLight Light, ShaderMaterial ShadowVolume) {
			RenderAPI.DbgPushGroup("Entity DrawShadowVolume");

			for (int i = 0; i < Entities.Length; i++)
				Entities[i].DrawShadowVolume(Light.GetBoundingSphere(), ShadowVolume);

			RenderAPI.DbgPopGroup();
		}
	}
}
