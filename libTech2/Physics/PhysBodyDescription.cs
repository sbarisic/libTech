using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;


namespace libTech.Physics {
	public class PhysBodyDescription {
		BodyDescription BodyDesc;

		internal PhysBodyDescription(BodyDescription BodyDesc) {
			this.BodyDesc = BodyDesc;
		}


		public PhysBodyDescription() {
		}
	}
}
