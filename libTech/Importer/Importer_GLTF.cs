using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libTech.Graphics;
using glTFLoader;
using glTFLoader.Schema;
using System.IO;
using System.Numerics;
using GlMesh = glTFLoader.Schema.Mesh;
using GfxMesh = libTech.Graphics.Mesh;
using System.Runtime.InteropServices;

namespace libTech.Importer {
	/*
	public unsafe class Importer_GLTF : Importer<Graphics.Mesh> {
		public override bool CanLoadExt(string Extension) {
			return Extension == ".gltf";
		}

		int GetAccessorTypeSize(Accessor.TypeEnum T) {
			switch (T) {
				case Accessor.TypeEnum.SCALAR:
					return 1;
				case Accessor.TypeEnum.VEC2:
					return 2;
				case Accessor.TypeEnum.VEC3:
					return 3;
				case Accessor.TypeEnum.VEC4:
					return 4;
				case Accessor.TypeEnum.MAT2:
					return 2 * 2;
				case Accessor.TypeEnum.MAT3:
					return 3 * 3;
				case Accessor.TypeEnum.MAT4:
					return 4 * 4;
				default:
					throw new NotImplementedException();
			}
		}

		int GetAccessorComponentSize(Accessor.ComponentTypeEnum T) {
			switch (T) {
				case Accessor.ComponentTypeEnum.BYTE:
				case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
					return sizeof(byte);
				case Accessor.ComponentTypeEnum.SHORT:
				case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
					return sizeof(ushort);
				case Accessor.ComponentTypeEnum.UNSIGNED_INT:
					return sizeof(uint);
				case Accessor.ComponentTypeEnum.FLOAT:
					return sizeof(float);
				default:
					throw new NotImplementedException();
			}
		}

		int GetSize(Accessor A) {
			return GetAccessorTypeSize(A.Type) * GetAccessorComponentSize(A.ComponentType);
		}

		byte[] GetBuffer(string RootDir, Gltf G, Accessor A, BufferView View) {
			int Stride = View.ByteStride ?? 0;
			byte[] Val = new byte[View.ByteLength];

			Stride = Stride - GetSize(A);
			if (Stride < 0)
				throw new InvalidOperationException();

			glTFLoader.Schema.Buffer B = G.Buffers[View.Buffer];
			using (FileStream FS = File.OpenRead(Path.Combine(RootDir, B.Uri))) {
				FS.Seek(View.ByteOffset, SeekOrigin.Begin);

				if (Stride == 0)
					FS.Read(Val, 0, Val.Length);
				else
					for (int i = 0; i < Val.Length; i++) {
						FS.Seek(View.ByteOffset + i * Stride, SeekOrigin.Begin);
						FS.Read(Val, i, 1);
					}
			}

			return Val;
		}

		T[] GetBuffer<T>(string RootDir, Gltf G, Accessor A, BufferView V) where T : struct {
			byte[] Bytes = GetBuffer(RootDir, G, A, V);
			T[] Val = new T[Bytes.Length / Marshal.SizeOf<T>()];

			GCHandle H = GCHandle.Alloc(Val, GCHandleType.Pinned);
			Marshal.Copy(Bytes, 0, H.AddrOfPinnedObject(), Bytes.Length);
			H.Free();

			return Val;
		}

		public override GfxMesh Load(string FilePath) {
			GfxMesh Msh = new GfxMesh();
			string RootPath = Path.GetDirectoryName(FilePath);

			Gltf G = Interface.LoadModel(FilePath);
			GlMesh GlMsh = G.Meshes[0];

			for (int i = 0; i < GlMsh.Primitives.Length; i++) {
				MeshPrimitive P = GlMsh.Primitives[i];

				foreach (var Attr in P.Attributes) {
					Accessor Accessor = G.Accessors[Attr.Value];

					if (Attr.Key == "POSITION")
						Msh.SetVertices(GetBuffer<Vector3>(RootPath, G, Accessor, G.BufferViews[Accessor.BufferView ?? 0]));

					else if (Attr.Key == "TEXCOORD_0")
						Msh.SetUVs(GetBuffer<Vector2>(RootPath, G, Accessor, G.BufferViews[Accessor.BufferView ?? 0]));

					else if (Attr.Key == "COLOR_0") {
						Vector4[] Colors = null;

						if (Accessor.Type == Accessor.TypeEnum.VEC4 && Accessor.ComponentType == Accessor.ComponentTypeEnum.UNSIGNED_BYTE)
							Colors = GetBuffer<OpenGL.ColorRGBA32>(RootPath, G, Accessor, G.BufferViews[Accessor.BufferView ?? 0])
								.Select((C) => new Vector4(C.r / 255.0f, C.g / 255.0f, C.b / 255.0f, C.a / 255.0f)).ToArray();
						else if (Accessor.Type == Accessor.TypeEnum.VEC4 && Accessor.ComponentType == Accessor.ComponentTypeEnum.FLOAT)
							Colors = GetBuffer<Vector4>(RootPath, G, Accessor, G.BufferViews[Accessor.BufferView ?? 0]);
						else
							throw new NotImplementedException();

						Msh.SetColors(Colors);
					}
				}
			}

			return Msh;
		}
	}

	//*/
}
