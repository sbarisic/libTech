using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libTech.Entities;
using System.Numerics;
using BulletSharp;

namespace libTech {
	public unsafe static partial class Engine {
		static CollisionConfiguration PhysConfig;
		static CollisionDispatcher PhysDispatcher;
		static DbvtBroadphase Broadphase;
		static DiscreteDynamicsWorld PhysWorld;

		static void InitPhysics() {
			PhysConfig = new DefaultCollisionConfiguration();
			PhysDispatcher = new CollisionDispatcher(PhysConfig);
			Broadphase = new DbvtBroadphase();

			PhysWorld = new DiscreteDynamicsWorld(PhysDispatcher, Broadphase, null, PhysConfig);
			PhysWorld.Gravity = new Vector3(0, -9.81f, 0);
		}

		static void SpawnPhysicsEntity(PhysicsEntity E) {
			if (E.Body != null)
				PhysWorld.AddRigidBody(E.Body);
		}

		static void UpdatePhysics(float Dt) {
			PhysWorld.StepSimulation(Dt);
		}
	}
}
