using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libTech.Entities;

namespace libTech {
	public unsafe static partial class Engine {
		static List<Entity> Entities = new List<Entity>();

		public static void SpawnEntity(Entity E) {
			if (Entities.Contains(E))
				throw new InvalidOperationException("Can not spawn already spawned entity");

			if (E is PhysicsEntity)
				SpawnPhysicsEntity((PhysicsEntity)E);

			Entities.Add(E);
		}

		static void UpdateEntities(float Dt) {
			foreach (var E in Entities)
				E.Update(Dt);
		}

		static void DrawEntities(float Dt) {
			foreach (var E in Entities)
				E.Draw(Dt);
		}

		static void DrawTransparentEntities(float Dt) {
			foreach (var E in Entities)
				E.DrawTransparent(Dt);
		}

	}
}
