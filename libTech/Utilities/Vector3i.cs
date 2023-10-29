using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.CodeDom;

namespace libTech {
	public struct Vector3i {
		public static readonly Vector3i Empty = new Vector3i(0, 0, 0);

		public int X;
		public int Y;
		public int Z;

		public Vector3i(int x, int y, int z) {
			X = x;
			Y = y;
			Z = z;
		}

		public Vector3i(Vector3 V) : this((int)V.X, (int)V.Y, (int)V.Z) {
		}

		public override string ToString() {
			return new Vector3(X, Y, Z).ToString();
		}

		public static Vector3i operator *(Vector3i v1, Vector3i v2) {
			return new Vector3i(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
		}

		public static Vector3i operator *(Vector3i a, int b) {
			return new Vector3i(a.X * b, a.Y * b, a.Z * b);
		}

		public static implicit operator Vector3(Vector3i v) {
			return new Vector3(v.X, v.Y, v.Z);
		}

		public static bool operator ==(Vector3i A, Vector3i B) {
			return A.X == B.X && A.Y == B.Y && A.Z == B.Z;
		}

		public static bool operator !=(Vector3i A, Vector3i B) {
			return !(A == B);
		}
	}
}
