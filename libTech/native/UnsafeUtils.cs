using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace libTech {
	public static unsafe class UnsafeUtils {
		public static ref T AsRef<T>(this IntPtr Ptr) {
			return ref Unsafe.AsRef<T>(Ptr.ToPointer());
		}

		public static T[] ReadArray<T>(this IntPtr Ptr, uint Len) where T : struct {
			return new T[Len].Fill(Ptr);
		}

		public static T1 Cast<T1, T2>(this T2 Struct) where T1 : struct where T2 : struct {
			GCHandle H = GCHandle.Alloc(Struct, GCHandleType.Pinned);
			T1 Struct2 = H.AddrOfPinnedObject().AsRef<T1>();
			H.Free();
			return Struct2;
		}

		public static T[] Fill<T>(this T[] Arr, IntPtr Memory) where T : struct {
			GCHandle H = GCHandle.Alloc(Arr, GCHandleType.Pinned);
			Unsafe.CopyBlock(H.AddrOfPinnedObject().ToPointer(), Memory.ToPointer(), (uint)Arr.Length * (uint)Marshal.SizeOf<T>());
			H.Free();
			return Arr;
		}
	}
}
