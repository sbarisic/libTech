using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libTech.Graphics.Voxels {
	enum BlockType : ushort {
		None,
		Stone,
		Dirt,
		StoneBrick,
		Sand,
		Bricks,
		Plank,
		EndStoneBrick,
		Ice,
		Test,
		Leaf,
		Water,
		Glass,
		Glowstone,
		Test2,

		// Blocks with different sides go here
		Grass,
		Wood,
	}

	static class BlockInfo {
		public static bool IsOpaque(BlockType T) {
			switch (T) {
				case BlockType.None:
					return false;

				/*case BlockType.Water:
				case BlockType.Glass:
				case BlockType.Ice:
					return false;*/

				default:
					return true;
			}
		}

		public static bool EmitsLight(BlockType T) {
			switch (T) {
				case BlockType.Glowstone:
					return true;

				default:
					return false;
			}
		}
	}
}
