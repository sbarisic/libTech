using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.InteropServices;

namespace libTech {
	[StructLayout(LayoutKind.Sequential)]
	public struct Matrix3x3 {
		public Vector3 RowA, RowB, RowC;

		public static implicit operator Matrix4x4(Matrix3x3 M) {
			Matrix4x4 Mat = Matrix4x4.Identity;
			Mat.M11 = M.RowA.X;
			Mat.M12 = M.RowA.Y;
			Mat.M13 = M.RowA.Z;

			Mat.M21 = M.RowB.X;
			Mat.M22 = M.RowB.Y;
			Mat.M23 = M.RowB.Z;

			Mat.M31 = M.RowC.X;
			Mat.M32 = M.RowC.Y;
			Mat.M33 = M.RowC.Z;
			return Mat;
		}

		public static implicit operator Matrix3x3(Matrix4x4 M) {
			Matrix3x3 Mat = new Matrix3x3();
			Mat.RowA.X = M.M11;
			Mat.RowA.Y = M.M12;
			Mat.RowA.Z = M.M13;

			Mat.RowB.X = M.M21;
			Mat.RowB.Y = M.M22;
			Mat.RowB.Z = M.M23;

			Mat.RowC.X = M.M31;
			Mat.RowC.Y = M.M32;
			Mat.RowC.Z = M.M33;
			return Mat;
		}
	}
}
