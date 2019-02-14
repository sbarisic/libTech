using FishGfx;
using libTech.Map;
using libTech.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Entities {
	public abstract class Entity {
		public libTechMap Map;
		internal bool HasSpawned = false;

		public virtual void Spawned() {
		}

		public virtual BoundSphere GetBoundingSphere() {
			return BoundSphere.Empty;
		}

		public virtual void Update(float Dt) {
		}

		public virtual void DrawOpaque() {
		}

		public virtual void DrawTransparent() {
		}

		public virtual void DrawShadowVolume(BoundSphere Light, ShaderMaterial ShadowVolume) {
		}
	}
}
