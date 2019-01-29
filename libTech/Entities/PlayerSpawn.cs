using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Entities {
	public class PlayerSpawn : Entity {
		public Vector3 SpawnPosition;
		public Quaternion SpawnOrientation;

		public PlayerSpawn(Vector3 SpawnPosition, Quaternion SpawnOrientation) {
			this.SpawnPosition = SpawnPosition;
			this.SpawnOrientation = SpawnOrientation;
		}

		public override string ToString() {
			return string.Format("PlayerSpawn({0})", SpawnPosition);
		}
	}
}
