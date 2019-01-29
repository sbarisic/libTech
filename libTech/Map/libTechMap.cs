using FishGfx;
using libTech;
using libTech.Entities;
using libTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Map {
	public class libTechMap {
		libTechModel[] MapModels;
		Entity[] Entities;

		public libTechMap() {
			MapModels = new libTechModel[] { };
			Entities = new Entity[] { };
		}

		public void AddModel(libTechModel Model) {
			Array.Resize(ref MapModels, MapModels.Length + 1);
			MapModels[MapModels.Length - 1] = Model;
		}

		public AABB CalculateAABB() {
			AABB Bounds = MapModels[0].CalculateAABB();

			for (int ModelIdx = 1; ModelIdx < MapModels.Length; ModelIdx++) {
				Vector3[] Verts = new Vector3[16];
				Array.Copy(Bounds.GetVertices().ToArray(), 0, Verts, 0, 8);
				Array.Copy(MapModels[ModelIdx].CalculateAABB().GetVertices().ToArray(), 0, Verts, 8, 8);
				Bounds = AABB.CalculateAABB(Verts);
			}

			return Bounds;
		}

		public void SpawnEntity(Entity Ent) {
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

		public void Draw() {
			DrawOpaque();
			DrawTransparent();
		}
	}
}
