using FishGfx;
using FishGfx.Formats;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
//using SourceUtils;
//using SourceUtils.ValveBsp;
using libTech.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vector3 = System.Numerics.Vector3;

namespace libTech.Map {
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct BSPHeader {
		public fixed byte Magic[4];
		public int Version;
		fixed int Entries[34];

		public BSPDirEntry[] GetDirEntries() {
			List<BSPDirEntry> DirEntriesList = new List<BSPDirEntry>();

			for (int i = 0; i < 17; i++)
				DirEntriesList.Add(new BSPDirEntry() { Offset = Entries[i * 2], Length = Entries[i * 2 + 1] });

			return DirEntriesList.ToArray();
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct BSPDirEntry {
		public int Offset;
		public int Length;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct BSPEntityLump {
		public string Ents;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct BSPTexture {
		public fixed byte Name[64];
		public int Flags;
		public int Contents;

		public string GetName() {
			fixed (byte* NamePtr = Name)
				return Encoding.ASCII.GetString(NamePtr, 64).TrimEnd(new[] { '\0' });
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct BSPLeaf {
		public int Cluster;
		public int Area;

		public int MinX;
		public int MinY;
		public int MinZ;

		public int MaxX;
		public int MaxY;
		public int MaxZ;

		public int FirstFace;
		public int NumFaces;

		public int FirstBrush;
		public int NumBrushes;
	}

	public enum FaceType : int {
		Polygon = 1,
		Patch,
		Mesh,
		Billboard
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct BSPFace {
		public int Texture;
		public int Effect;
		public FaceType Type;

		public int Vert;
		public int NVerts;

		public int MeshVert;
		public int NMeshVerts;

		public int LightmapIndex;

		public int LightmapStartX;
		public int LightmapStartY;

		public int LightmapSizeX;
		public int LightmapSizeY;

		public Vector3 LightmapOrigin;
		public Vector2 LightmapVecs1;
		public Vector2 LightmapVecs2;
		public Vector2 LightmapVecs3;

		public Vector3 SurfaceNormal;

		public int SizeX;
		public int SizeY;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct BSPVertex {
		public Vector3 Position;
		public Vector2 SurfaceUV;
		public Vector2 LightmapUV;
		public Vector3 Normal;
		public int Color;

		public Vertex3 ToVertex3() {
			return new Vertex3(new Vector3(Position.X, Position.Z, Position.Y), SurfaceUV, new FishGfx.Color(Color));
			//return new Vertex3(new Vector3(Position.X, Position.Z, Position.Y), SurfaceUV, FishGfx.Color.White);
		}
	}

	public unsafe class BSP {
		public BSPHeader Header;
		public BSPEntityLump Entities;
		public BSPTexture[] Textures;

		public BSPLeaf[] Leaves;
		public int[] LeafFaces;
		public BSPFace[] Faces;

		public BSPVertex[] Vertices;
		public int[] MeshVertices;

		public static BSP FromStream(Stream BSPStream) {
			BSP Map = new BSP();
			byte[] BSPBytes = null;

			using (MemoryStream MS = new MemoryStream()) {
				BSPStream.CopyTo(MS);
				BSPBytes = MS.ToArray();
			}

			fixed (byte* BSPPtr = BSPBytes) {
				Map.Header = *(BSPHeader*)BSPPtr;

				BSPDirEntry[] DirEntries = Map.Header.GetDirEntries();
				for (int DirEntryIdx = 0; DirEntryIdx < DirEntries.Length; DirEntryIdx++) {
					BSPDirEntry DirEntry = DirEntries[DirEntryIdx];

					void* LumpPtr = (void*)(BSPPtr + DirEntry.Offset);
					int LumpLen = DirEntry.Length;

					switch (DirEntryIdx) {
						case 0: {
								Map.Entities = new BSPEntityLump();
								Map.Entities.Ents = Encoding.ASCII.GetString((byte*)LumpPtr, LumpLen);
								break;
							}

						case 1: {
								BSPTexture[] Textures = new BSPTexture[LumpLen / sizeof(BSPTexture)];
								BSPTexture* TexturesPtr = (BSPTexture*)LumpPtr;

								for (int i = 0; i < Textures.Length; i++)
									Textures[i] = TexturesPtr[i];

								Map.Textures = Textures;
								break;
							}

						case 4: {
								BSPLeaf[] Leaves = new BSPLeaf[LumpLen / sizeof(BSPLeaf)];
								BSPLeaf* LeavesPtr = (BSPLeaf*)LumpPtr;

								for (int i = 0; i < Leaves.Length; i++)
									Leaves[i] = LeavesPtr[i];

								Map.Leaves = Leaves;
								break;
							}

						case 5: {
								int[] LeafFaces = new int[LumpLen / sizeof(int)];
								int* LeafFacesPtr = (int*)LumpPtr;

								for (int i = 0; i < LeafFaces.Length; i++)
									LeafFaces[i] = LeafFacesPtr[i];

								Map.LeafFaces = LeafFaces;
								break;
							}

						case 10: {
								BSPVertex[] Vertices = new BSPVertex[LumpLen / sizeof(BSPVertex)];
								BSPVertex* VerticesPtr = (BSPVertex*)LumpPtr;

								for (int i = 0; i < Vertices.Length; i++)
									Vertices[i] = VerticesPtr[i];

								Map.Vertices = Vertices;
								break;
							}

						case 11: {
								int[] MeshVertices = new int[LumpLen / sizeof(int)];
								int* MeshVerticesPtr = (int*)LumpPtr;

								for (int i = 0; i < MeshVertices.Length; i++)
									MeshVertices[i] = MeshVerticesPtr[i];

								Map.MeshVertices = MeshVertices;
								break;
							}

						case 13: {
								int BSPFaceSize = sizeof(BSPFace);
								BSPFace[] Faces = new BSPFace[LumpLen / BSPFaceSize];
								BSPFace* FacesPtr = (BSPFace*)LumpPtr;

								for (int i = 0; i < Faces.Length; i++)
									Faces[i] = FacesPtr[i];

								Map.Faces = Faces;
								break;
							}

						default:
							break;
					}
				}


			}

			return Map;
		}

		public static libTechModel LoadAsModel(string Pth) {
			BSP Map = BSP.FromStream(Engine.VFS.OpenFile(Pth));

			libTechModel Model = new libTechModel();
			Dictionary<int, List<Vertex3>> TexturedMeshes = new Dictionary<int, List<Vertex3>>();

			foreach (var FaceIdx in GetFaceIndices(Map)) {
				BSPFace Face = Map.Faces[FaceIdx];

				if (!TexturedMeshes.ContainsKey(Face.Texture))
					TexturedMeshes.Add(Face.Texture, new List<Vertex3>());

				if (Face.Type == FaceType.Polygon || Face.Type == FaceType.Mesh) {
					Vertex3[] Vertices = new Vertex3[Face.NVerts];
					for (int VertOffset = 0; VertOffset < Face.NVerts; VertOffset++)
						Vertices[VertOffset] = Map.Vertices[Face.Vert + VertOffset].ToVertex3();

					for (int MeshVertOffset = 0; MeshVertOffset < Face.NMeshVerts; MeshVertOffset++)
						TexturedMeshes[Face.Texture].Add(Vertices[Map.MeshVertices[Face.MeshVert + MeshVertOffset]]);
				}
			}

			string[] TextureNames = Map.Textures.Select(T => T.GetName()).ToArray();

			foreach (var KV in TexturedMeshes) {
				string TexName = TextureNames[KV.Key];

				if (TexName == "textures/lun3dm5/lblusky2")
					continue;

				Model.AddMesh(new libTechMesh(KV.Value.ToArray().Reverse().ToArray(), Engine.GetMaterial(TexName)));
			}

			return Model;
		}

		static IEnumerable<int> GetFaceIndices(BSP Map) {
			HashSet<int> AlreadyVisible = new HashSet<int>();

			foreach (var L in Map.Leaves) {
				/*if (L.Cluster < 0)
					continue;*/

				for (int FaceOffset = 0; FaceOffset < L.NumFaces; FaceOffset++) {
					int FaceIdx = L.FirstFace + FaceOffset;

					if (AlreadyVisible.Contains(FaceIdx))
						continue;

					AlreadyVisible.Add(FaceIdx);
					yield return Map.LeafFaces[FaceIdx];
				}
			}
		}
	}
}