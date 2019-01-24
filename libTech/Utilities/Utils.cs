using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
/*using PhysVector3 = BEPUutilities.Vector3;
using PhysMatrix = BEPUutilities.Matrix;
using PhysQuat = BEPUutilities.Quaternion;*/

namespace libTech {
	public unsafe static class Utils {
		/*public static Vector3 ToVec3(this PhysVector3 V) {
			return new Vector3(V.X, V.Y, V.Z);
		}

		public static PhysVector3 ToPhysVec3(this Vector3 V) {
			return new PhysVector3(V.X, V.Y, V.Z);
		}

		public static IEnumerable<Vector3> ToVec3(this IEnumerable<PhysVector3> Vecs) {
			foreach (var V in Vecs)
				yield return V.ToVec3();
		}

		public static IEnumerable<PhysVector3> ToPhysVec3(this IEnumerable<Vector3> Vecs) {
			foreach (var V in Vecs)
				yield return V.ToPhysVec3();
		}

		public static PhysMatrix ToPhysMatrix(this Matrix4x4 M) {
			return new PhysMatrix(M.M11, M.M12, M.M13, M.M14, M.M21, M.M22, M.M23, M.M24, M.M31, M.M32, M.M33, M.M34, M.M41, M.M42, M.M43, M.M44);
		}

		public static PhysQuat ToPhysQuat(this Quaternion Q) {
			return new PhysQuat(Q.X, Q.Y, Q.Z, Q.W);
		}

		public static Quaternion ToQuat(this PhysQuat Q) {
			return new Quaternion(Q.X, Q.Y, Q.Z, Q.W);
		}*/

		static Random Rnd = new Random();

		public static IEnumerable<string[]> Split(this string[] Array, string Separator) {
			List<string> Previous = new List<string>();

			foreach (var Item in Array) {
				if (Item != Separator)
					Previous.Add(Item);
				else {
					yield return Previous.ToArray();
					Previous.Clear();
				}
			}

			yield return Previous.ToArray();
		}

		public static char RandomChar() {
			return (char)Random(33, 127);
		}

		public static byte RandomByte() {
			return (byte)Rnd.Next(256);
		}

		public static FishGfx.Color RandomColor() {
			return new FishGfx.Color(RandomByte(), RandomByte(), RandomByte());
		}

		public static string RandomString(int MinInclusiveLen, int MaxExclusiveLen) {
			return new string(Random(MinInclusiveLen, MaxExclusiveLen).Range().Select(_ => RandomChar()).ToArray());
		}

		public static int Random(int Inclusive, int Exclusive) {
			return Rnd.Next(Inclusive, Exclusive);
		}

		public static float RandomFloat() {
			return (float)Rnd.NextDouble();
		}

		public static Vector2 RandomAround(this Vector2 Center, float Radius) {
			double Angle = Rnd.NextDouble() * 2 * Math.PI;
			double Rad = Radius * Math.Sqrt(Rnd.NextDouble());
			return new Vector2(Center.X + (float)(Rad * Math.Cos(Angle)), Center.Y + (float)(Rad * Math.Sin(Angle)));
		}

		public static Vector2 RandomVec2(float ScaleX = 1, float ScaleY = 1) {
			return new Vector2(RandomFloat() * ScaleX, RandomFloat() * ScaleY);
		}

		public static float ToRad(this float Deg) {
			return Deg * (float)Math.PI / 180;
		}

		public static IEnumerable<int> Range(this int Count) {
			for (int i = 0; i < Count; i++)
				yield return i;
		}

		public static float Clamp(this float Val, float Min, float Max) {
			if (Val < Min)
				return Min;
			if (Val > Max)
				return Max;
			return Val;
		}

		public static string NormalizeFilePath(this string Str) {
			return Str.Replace('\\', '/');
		}

		public static float ParseToFloat(this string Str) {
			return float.Parse(Str, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public static void Copy(IntPtr source, int sourceOffset, IntPtr destination, int destinationOffset, int count) {
			byte* src = (byte*)source + sourceOffset;
			byte* dst = (byte*)destination + destinationOffset;
			byte* end = dst + count;

			while (dst != end)
				*dst++ = *src++;
		}

		public static T Random<T>(this IEnumerable<T> Collection) {
			return Collection.ElementAt(Random(0, Collection.Count()));
		}

		public static uint[] ToUTF8CodePoints(this string Str) {
			byte[] Bytes = Encoding.UTF8.GetBytes(Str);

			byte* Char = stackalloc byte[4];
			uint* Unicode = (uint*)Char;
			int CharCounter = 0;

			List<uint> Unicodes = new List<uint>();

			for (int i = 0; i < Bytes.Length; i++) {
				if (Bytes[i].SingleUTF8Byte()) {
					if (CharCounter != 0) {
						CharCounter = 0;
						Unicodes.Add(*Unicode);
						*Unicode = 0;
					}

					Unicodes.Add(Bytes[i]);
				} else {
					if (Bytes[i].LeadingUTF8MultiByte()) {
						if (CharCounter != 0) {
							CharCounter = 0;
							Unicodes.Add(*Unicode);
							*Unicode = 0;
						}
					}

					Char[CharCounter++] = Bytes[i];
				}
			}

			if (CharCounter != 0) {
				CharCounter = 0;
				Unicodes.Add(*Unicode);
				*Unicode = 0;
			}

			// TODO; convert to code points
			return Unicodes.ToArray();
		}

		public static bool SingleUTF8Byte(this byte B) {
			return B >> 7 == 0;
		}

		public static bool LeadingUTF8MultiByte(this byte B) {
			return B >> 6 == 3;
		}

		public static string TrimQuotes(this string Str) {
			Str = Str.Trim();

			if (Str.StartsWith("\"") && Str.EndsWith("\""))
				return Str.Substring(1, Str.Length - 2);

			return Str;
		}

		public static Bitmap RemoveAlpha(this Bitmap Bmp, Color? BackgroundColor = null) {
			/*for (int Y = 0; Y < Bmp.Height; Y++) {
				for (int X = 0; X < Bmp.Width; X++) {
					Color Clr = Bmp.GetPixel(X, Y);
					Clr = Color.FromArgb(255, Clr);
					Bmp.SetPixel(X, Y, Clr);
				}
			}*/

			using (Bitmap BmpCopy = new Bitmap(Bmp))
			using (System.Drawing.Graphics Gfx = System.Drawing.Graphics.FromImage(Bmp)) {
				Gfx.Clear(BackgroundColor ?? Color.Black);
				Gfx.DrawImage(BmpCopy, 0, 0);
			}

			return Bmp;
		}
	}


	public static class PathUtils {
		public static string CleanUp(string Pth) {
			if (Pth.Contains("\\"))
				Pth = Pth.Replace("\\", "/");

			if (Pth.StartsWith("/"))
				Pth = Pth.Substring(1);

			if (Pth.EndsWith("/"))
				Pth = Pth.Substring(0, Pth.Length - 1);

			return Pth;
		}

		public static string GetFullPath(string Pth) {
			return CleanUp(Path.GetFullPath(Pth));
		}

		public static string Combine(params string[] Paths) {
			return CleanUp(Path.Combine(Paths));
		}

		public static bool RemoveVirtualPrefix(ref string Pth, string Prefix) {
			Pth = CleanUp(Pth);
			Prefix = CleanUp(Prefix);

			if (Pth.StartsWith(Prefix)) {
				Pth = CleanUp(Pth.Substring(Prefix.Length));
				return true;
			}

			return false;
		}
	}
}
