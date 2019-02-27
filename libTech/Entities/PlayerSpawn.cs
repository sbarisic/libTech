using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Entities {
	[EntityClassName("info_player_start")]
	[EntityClassName("info_player_deathmatch")]
	[EntityClassName("info_player_terrorist")]
	[EntityClassName("info_player_counterterrorist")]
	[EntityClassName("info_coop_spawn")]
	public class PlayerSpawn : Entity {
		public Vector3 SpawnPosition;
		public Quaternion SpawnOrientation;

		public PlayerSpawn([EntityKey("origin")] Vector3 SpawnPosition, [EntityKey("qangles")] Quaternion SpawnOrientation) {
			this.SpawnPosition = SpawnPosition;
			this.SpawnOrientation = SpawnOrientation;
		}

		public override string ToString() {
			return string.Format("PlayerSpawn({0})", SpawnPosition);
		}
	}
}
