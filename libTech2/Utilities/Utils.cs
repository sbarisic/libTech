using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using ImageMagick;

using Vertex2 = FishGfx.Vertex2;
/*using PhysVector3 = BEPUutilities.Vector3;
using PhysMatrix = BEPUutilities.Matrix;
using PhysQuat = BEPUutilities.Quaternion;*/

namespace libTech {
	public delegate bool RaycastCallbackFunc(int X, int Y, int Z, Vector3 FaceNormal);

	public unsafe static class Utils {
		public static Random Rnd = new Random();
		static Dictionary<int, Vector3[]> SphereDirections = new Dictionary<int, Vector3[]>();
		public static readonly Vector3[] MainDirs = new[] { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, -1, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };

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

		public static Vector3 RandomVec3(float ScaleX = 1, float ScaleY = 1, float ScaleZ = 1) {
			return new Vector3(RandomFloat() * ScaleX, RandomFloat() * ScaleY, RandomFloat() * ScaleZ);
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

			// TODO: convert to code points
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

		public static Vector3 RandomPointOnSphere(this Vector3 Center, float Radius) {
			return Center + Vector3.Normalize(RandomVec3()) * Radius;
		}

		public static void Insert<T>(ref T[] Arr, T Element) {
			for (int i = 0; i < Arr.Length; i++) {
				if (Arr[i] == null) {
					Arr[i] = Element;
					return;
				}
			}

			Array.Resize(ref Arr, Arr.Length + 1);
			Arr[Arr.Length - 1] = Element;
		}

		public static void Remove<T>(ref T[] Arr, T Element) {
			for (int i = 0; i < Arr.Length; i++) {
				if (Arr[i].Equals(Element)) {
					Arr[i] = Arr[Arr.Length - 1];
					Array.Resize(ref Arr, Arr.Length - 1);
					return;
				}
			}
		}

		internal static Vertex2[] EmitRectangleTris(Vertex2[] Verts, int Offset, float X, float Y, float W, float H, float U0 = 0, float V0 = 0, float U1 = 1, float V1 = 1, Color? Color = null) {
			Color C = Color ?? FishGfx.Color.White;

			Verts[Offset] = new Vertex2(new Vector2(X, Y), new Vector2(U0, V0), C);
			Verts[Offset + 1] = new Vertex2(new Vector2(X + W, Y + H), new Vector2(U1, V1), C);
			Verts[Offset + 2] = new Vertex2(new Vector2(X, Y + H), new Vector2(U0, V1), C);
			Verts[Offset + 3] = new Vertex2(new Vector2(X, Y), new Vector2(U0, V0), C);
			Verts[Offset + 4] = new Vertex2(new Vector2(X + W, Y), new Vector2(U1, V0), C);
			Verts[Offset + 5] = new Vertex2(new Vector2(X + W, Y + H), new Vector2(U1, V1), C);

			return Verts;
		}

		public static byte DirToByte(Vector3 Normal) {
			int X = (int)Normal.X;
			int Y = (int)Normal.Y;
			int Z = (int)Normal.Z;

			if (X == -1)
				return 0;

			if (X == 1)
				return 1;

			if (Y == -1)
				return 2;

			if (Y == 1)
				return 3;

			if (Z == -1)
				return 4;

			if (Z == 1)
				return 5;

			throw new Exception("Invalid direction");
		}

		static Vector3[] CalculateSphereDirections(int Slices) {
			if (SphereDirections.ContainsKey(Slices))
				return SphereDirections[Slices];

			List<Vector3> Directions = new List<Vector3>();

			double da = Math.PI / (Slices - 1);
			double a = -0.5 * Math.PI;
			int Hits = 0;

			for (int ia = 0; ia < Slices; ia++, a += da) {
				double r = Math.Cos(a);
				int nb = (int)Math.Ceiling(2.0 * Math.PI * r / da);
				double db = 2.0 * Math.PI / (nb);

				if ((ia == 0) || (ia == Slices - 1)) {
					nb = 1;
					db = 0.0;
				}

				double b = 0;
				for (int ib = 0; ib < nb; ib++, b += db) {
					float x = (float)(r * Math.Cos(b));
					float y = (float)(r * Math.Sin(b));
					float z = (float)Math.Sin(a);

					Vector3 Normal = Vector3.Normalize(new Vector3(x, y, z));

					Directions.Add(Normal);
				}
			}

			Vector3[] DirectionsArray = Directions.ToArray();
			SphereDirections.Add(Slices, DirectionsArray);
			return DirectionsArray;
		}

		static float IntBound(float S, float Ds) {
			if (Ds < 0) {
				Ds = -Ds;
				S = -S;
			}

			return (1 - (S % 1 + 1) % 1) / Ds;
		}

		public static bool Raycast(Vector3 Origin, Vector3 Direction, float Length, RaycastCallbackFunc Callback) {
			// Cube containing origin point.
			float X = (float)Math.Floor(Origin.X);
			float Y = (float)Math.Floor(Origin.Y);
			float Z = (float)Math.Floor(Origin.Z);

			// Break out direction vector.
			float Dx = Direction.X;
			float Dy = Direction.Y;
			float Dz = Direction.Z;

			// Direction to increment x,y,z when stepping.
			float StepX = Dx > 0 ? 1 : Dx < 0 ? -1 : 0;
			float StepY = Dy > 0 ? 1 : Dy < 0 ? -1 : 0;
			float StepZ = Dz > 0 ? 1 : Dz < 0 ? -1 : 0;

			// See description above. The initial values depend on the fractional
			// part of the origin.
			float tMaxX = IntBound(Origin.X, Dx);
			float tMaxY = IntBound(Origin.Y, Dy);
			float tMaxZ = IntBound(Origin.Z, Dz);

			// The change in t when taking a step (always positive).
			float tDeltaX = StepX / Dx;
			float tDeltaY = StepY / Dy;
			float tDeltaZ = StepZ / Dz;

			// Buffer for reporting faces to the callback.
			var face = new Vector3();

			// Avoids an infinite loop.
			if (Dx == 0 && Dy == 0 && Dz == 0)
				throw new Exception("Raycast in zero direction!");

			// Rescale from units of 1 cube-edge to units of 'direction' so we can
			// compare with 't'.
			Length /= (float)Math.Sqrt(Dx * Dx + Dy * Dy + Dz * Dz);

			while (true) {
				if (Callback((int)X, (int)Y, (int)Z, face))
					return true;

				// tMaxX stores the t-value at which we cross a cube boundary along the
				// X axis, and similarly for Y and Z. Therefore, choosing the least tMax
				// chooses the closest cube boundary. Only the first case of the four
				// has been commented in detail.
				if (tMaxX < tMaxY) {
					if (tMaxX < tMaxZ) {
						if (tMaxX > Length)
							break;
						// Update which cube we are now in.
						X += StepX;
						// Adjust tMaxX to the next X-oriented boundary crossing.
						tMaxX += tDeltaX;
						// Record the normal vector of the cube face we entered.
						face.X = -StepX;
						face.Y = 0;
						face.Z = 0;
					} else {
						if (tMaxZ > Length)
							break;
						Z += StepZ;
						tMaxZ += tDeltaZ;
						face.X = 0;
						face.Y = 0;
						face.Z = -StepZ;
					}
				} else {
					if (tMaxY < tMaxZ) {
						if (tMaxY > Length)
							break;
						Y += StepY;
						tMaxY += tDeltaY;
						face.X = 0;
						face.Y = -StepY;
						face.Z = 0;
					} else {
						// Identical to the second case, repeated for simplicity in
						// the conditionals.
						if (tMaxZ > Length)
							break;
						Z += StepZ;
						tMaxZ += tDeltaZ;
						face.X = 0;
						face.Y = 0;
						face.Z = -StepZ;
					}
				}
			}

			return false;
		}

		public static int RaycastSphere(Vector3 Origin, int Radius, RaycastCallbackFunc Callback, int Slices = 32) {
			Vector3[] Dirs = CalculateSphereDirections(Slices);
			int Hits = 0;

			foreach (var Dir in Dirs) {
				if (Raycast(Origin, Dir, Radius, Callback))
					Hits++;
			}

			return Hits;
		}

		public static int RaycastHalfSphere(Vector3 Origin, Vector3 HalfSphereDir, int Radius, RaycastCallbackFunc Callback, out int MaxHits, int Slices = 32) {
			Vector3[] Dirs = CalculateSphereDirections(Slices);
			int Hits = 0;
			MaxHits = 0;

			foreach (var Dir in Dirs) {
				if (Vector3.Dot(Dir, HalfSphereDir) > 0) {
					MaxHits++;

					if (Raycast(Origin, Dir, Radius, Callback))
						Hits++;
				}
			}

			return Hits;
		}

		public static int Mod(int A, int B) {
			return A - B * (int)Math.Floor((float)A / (float)B);
		}

		public static Vector3 Slide(Vector3 Vec, Vector3 WallNormal) {
			Vector3 Undesired = WallNormal * Vector3.Dot(Vec, WallNormal);
			return Vec - Undesired;
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

		public static bool AddVirtualPrefix(ref string Pth, string Prefix) {
			Pth = CleanUp(Pth);
			Prefix = CleanUp(Prefix);

			/*if (Pth.StartsWith(Prefix)) {
				Pth = CleanUp(Pth.Substring(Prefix.Length));
				return true;
			}*/

			Pth = Combine(Prefix, Pth);
			return true;
		}

		public static Bitmap ToBitmap(this MagickImage Img, PixelMapping Mapping = PixelMapping.RGBA) {
			//StackTrace ST = new StackTrace(true);
			//MethodInfo CallingMethod = (MethodInfo)ST.GetFrame(1).GetMethod();
			//Console.WriteLine(">> {0}.{1}()", CallingMethod.DeclaringType.FullName, CallingMethod.Name);

			if (!OperatingSystem.IsWindows())
				throw new NotImplementedException("ToBitmap not implemented on non-Windows OS");

			PixelFormat Fmt = PixelFormat.Format32bppArgb;

			using (IPixelCollection<ushort> Px = Img.GetPixelsUnsafe()) {
				Bitmap Bmp = new Bitmap(Img.Width, Img.Height, Fmt);
				BitmapData Dat = Bmp.LockBits(new Rectangle(0, 0, Img.Width, Img.Height), ImageLockMode.ReadWrite, Fmt);
				nint Dst = Dat.Scan0;

				for (int y = 0; y < Img.Height; y++) {
					byte[] Bytes = Px.ToByteArray(0, y, Img.Width, 1, Mapping);
					Marshal.Copy(Bytes, 0, Dst, Bytes.Length);

					Dst = new IntPtr(Dst.ToInt64() + Dat.Stride);
				}

				Bmp.UnlockBits(Dat);
				return Bmp;
			}
		}
	}
}
