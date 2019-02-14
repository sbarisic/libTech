﻿using BulletSharp;
using FishGfx;
using libTech;
using libTech.Entities;
using libTech.Materials;
using libTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Map {
	public struct SweepResult {
		public bool HasHit;
		public float Fraction;
		public float Distance;

		public Vector3 SweepFrom;
		public Vector3 HitPoint;
		public Vector3 HitCenterOfMass;
		public Vector3 Normal;

		public override string ToString() {
			return string.Format("{0} {1} {2}", HasHit, (int)Distance, Fraction);
		}
	}

	public class libTechMap {
		const float UnitMeterScale = 52.5f;

		public static float UnitToMeter(float Unit) {
			return Unit / UnitMeterScale;
		}

		public static float MeterToUnit(float Meter) {
			return Meter * UnitMeterScale;
		}

		public static Vector3 UnitToMeter(Vector3 Unit) {
			return Unit / UnitMeterScale;
		}

		public static Vector3 MeterToUnit(Vector3 Meter) {
			return Meter * UnitMeterScale;
		}

		libTechModel[] MapModels;
		Entity[] Entities;
		DynamicLight[] Lights;

		// Physics
		internal DynamicsWorld World;
		internal CollisionConfiguration CollisionConf;
		internal CollisionDispatcher Dispatcher;
		internal BroadphaseInterface Broadphase;
		internal ConstraintSolver Solver;

		internal ClosestConvexResultCallback ClosestConvexResult;

		internal Dictionary<string, libTechModel> LoadedModels;

		public Vector3 Gravity {
			get {
				return World.Gravity;
			}

			set {
				World.Gravity = value;
			}
		}

		public libTechMap() {
			MapModels = new libTechModel[] { };
			Entities = new Entity[] { };
			Lights = new DynamicLight[] { };

			LoadedModels = new Dictionary<string, libTechModel>();
		}

		public void InitPhysics() {
			CollisionConf = new DefaultCollisionConfiguration();
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

			ClosestConvexResult = new ClosestConvexResultCallback();

			EntPhysics MapPhysics = new EntPhysics(libTechCollisionShape.FromVerticesConcave(GetModels().First().Meshes.SelectMany(M => M.GetVertices().Select(V => V.Position))), 0);
			SpawnEntity(MapPhysics);
		}

		public void AddModel(libTechModel Model) {
			Array.Resize(ref MapModels, MapModels.Length + 1);
			MapModels[MapModels.Length - 1] = Model;
		}

		public libTechModel LoadModel(string Pth) {
			if (LoadedModels.ContainsKey(Pth))
				return LoadedModels[Pth];

			libTechModel Mdl = Engine.Load<libTechModel>(Pth);

			if (Mdl == null)
				throw new Exception("Could not load model " + Pth);

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

		public bool RayCast(Vector3 From, Vector3 To, out Vector3 HitPos, out Vector3 HitNormal, out RigidBody Body) {
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

		public SweepResult Sweep(ConvexShape Shape, Vector3 From, Vector3 To) {
			ClosestConvexResult.ClosestHitFraction = 1.0f;
			ClosestConvexResult.ConvexFromWorld = From;
			ClosestConvexResult.ConvexToWorld = To;

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

		public SweepResult Sweep(ConvexShape Shape, Vector3 Pos, Vector3 Normal, float Dist) {
			return Sweep(Shape, Pos, Pos + Normal * Dist);
		}

		void InitEntity(Entity Ent) {
			Ent.Map = this;
			Ent.Spawned();
			Ent.HasSpawned = true;
		}

		public void SpawnEntity(Entity Ent) {
			Ent.HasSpawned = false;

			if (Ent is DynamicLight L) {
				bool AddedLight = false;

				for (int i = 0; i < Lights.Length; i++) {
					if (Lights[i] == null) {
						Lights[i] = L;
						AddedLight = true;
					}
				}

				if (!AddedLight) {
					Array.Resize(ref Lights, Lights.Length + 1);
					Lights[Lights.Length - 1] = L;
				}
			}

			for (int EntityIdx = 0; EntityIdx < Entities.Length; EntityIdx++) {
				if (Entities[EntityIdx] == null) {
					Entities[EntityIdx] = Ent;
					return;
				}
			}

			Array.Resize(ref Entities, Entities.Length + 1);
			SpawnEntity(Ent);
		}

		public void RemoveEntity(Entity Ent) {
			for (int i = 0; i < Entities.Length; i++) {
				if (Entities[i] == Ent) {
					Entities[i] = null;
					break;
				}
			}

			if (Ent is DynamicLight L) {
				for (int i = 0; i < Lights.Length; i++) {
					if (Lights[i] == L)
						Lights[i] = null;
				}

				for (int i = 0; i < Lights.Length; i++) {
					if (Lights[i] == null) {
						if (i + 1 < Lights.Length) {
							Lights[i] = Lights[i + 1];
							Lights[i + 1] = null;
						}
					}
				}

				Array.Resize(ref Lights, Lights.Length - 1);
			}
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
			World.StepSimulation(Dt);

			for (int i = 0; i < Entities.Length; i++) {
				if (!Entities[i].HasSpawned)
					InitEntity(Entities[i]);

				Entities[i].Update(Dt);
			}
		}

		public void DrawOpaque() {
			for (int ModelIdx = 0; ModelIdx < MapModels.Length; ModelIdx++)
				MapModels[ModelIdx].DrawOpaque();

			for (int EntityIdx = 0; EntityIdx < Entities.Length; EntityIdx++)
				Entities[EntityIdx]?.DrawOpaque();
		}

		public void DrawTransparent() {
			for (int ModelIdx = 0; ModelIdx < MapModels.Length; ModelIdx++)
				MapModels[ModelIdx].DrawTransparent();

			for (int EntityIdx = 0; EntityIdx < Entities.Length; EntityIdx++)
				Entities[EntityIdx]?.DrawTransparent();
		}

		public void DrawShadowVolume(ShaderMaterial ShadowVolume) {
			for (int ModelIdx = 0; ModelIdx < MapModels.Length; ModelIdx++)
				MapModels[ModelIdx].DrawShadowVolume(ShadowVolume);
		}

		public void DrawEntityShadowVolume(DynamicLight Light, ShaderMaterial ShadowVolume) {
			for (int i = 0; i < Entities.Length; i++) 
				Entities[i].DrawShadowVolume(Light.GetBoundingSphere(), ShadowVolume);
		}
	}
}
