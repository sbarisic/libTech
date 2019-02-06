using SourceUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector3 = System.Numerics.Vector3;

namespace libTech.Models {
	public struct libTechBone {
		public string Name;
		public Vector3 Position;

		public libTechBone(string Name, StudioModelFile.StudioBone Bone) {
			this.Name = Name;
			this.Position = new Vector3(Bone.Pos.X, Bone.Pos.Y, Bone.Pos.Z);
		}

		public override string ToString() {
			return Name;
		}
	}
}
