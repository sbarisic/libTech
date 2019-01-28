using FishGfx;
using FishGfx.Formats;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
//using SourceUtils;
//using SourceUtils.ValveBsp;
using libTech.Models;
using SourceUtils;
using SourceUtils.ValveBsp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SVector2 = SourceUtils.Vector2;
using SVector3 = SourceUtils.Vector3;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace libTech.Map {
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct BSPHeader {
		public fixed byte Magic[4];
		public int Version;
		fixed int Entries[34];

		public string GetMagic() {
			fixed (byte* MagicPtr = Magic)
				return Encoding.ASCII.GetString(MagicPtr, 4);
		}

		public BSPDirEntry[] GetDirEntries() {
			List<BSPDirEntry> DirEntriesList = new List<BSPDirEntry>();

			for (int i = 0; i < 17; i++)
				DirEntriesList.Add(new BSPDirEntry() { Offset = Entries[i * 2], Length = Entries[i * 2 + 1] });

			return DirEntriesList.ToArray();
		}

		public override string ToString() {
			return GetMagic() + Version.ToString();
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct BSPDirEntry {
		public int Offset;
		public int Length;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct Q3BSPEntityLump {
		public string Ents;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct Q3BSPTexture {
		public fixed byte Name[64];
		public int Flags;
		public int Contents;

		public string GetName() {
			fixed (byte* NamePtr = Name)
				return Encoding.ASCII.GetString(NamePtr, 64).TrimEnd(new[] { '\0' });
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct Q3BSPLeaf {
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
	public unsafe struct Q3BSPFace {
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
	public unsafe struct Q3BSPVertex {
		public Vector3 Position;
		public Vector2 SurfaceUV;
		public Vector2 LightmapUV;
		public Vector3 Normal;
		public int Color;

		public Vertex3 ToVertex3() {
			return new Vertex3(new Vector3(-Position.X, Position.Z, Position.Y), SurfaceUV, new FishGfx.Color(Color));
			//return new Vertex3(new Vector3(Position.X, Position.Z, Position.Y), SurfaceUV, FishGfx.Color.White);
		}
	}

	public unsafe class Q3BSP {
		public BSPHeader Header;
		public Q3BSPEntityLump Entities;
		public Q3BSPTexture[] Textures;

		public Q3BSPLeaf[] Leaves;
		public int[] LeafFaces;
		public Q3BSPFace[] Faces;

		public Q3BSPVertex[] Vertices;
		public int[] MeshVertices;

		public static Q3BSP FromStream(Stream BSPStream) {
			Q3BSP Map = new Q3BSP();
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
								Map.Entities = new Q3BSPEntityLump();
								Map.Entities.Ents = Encoding.ASCII.GetString((byte*)LumpPtr, LumpLen);
								break;
							}

						case 1: {
								Q3BSPTexture[] Textures = new Q3BSPTexture[LumpLen / sizeof(Q3BSPTexture)];
								Q3BSPTexture* TexturesPtr = (Q3BSPTexture*)LumpPtr;

								for (int i = 0; i < Textures.Length; i++)
									Textures[i] = TexturesPtr[i];

								Map.Textures = Textures;
								break;
							}

						case 4: {
								Q3BSPLeaf[] Leaves = new Q3BSPLeaf[LumpLen / sizeof(Q3BSPLeaf)];
								Q3BSPLeaf* LeavesPtr = (Q3BSPLeaf*)LumpPtr;

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
								Q3BSPVertex[] Vertices = new Q3BSPVertex[LumpLen / sizeof(Q3BSPVertex)];
								Q3BSPVertex* VerticesPtr = (Q3BSPVertex*)LumpPtr;

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
								int BSPFaceSize = sizeof(Q3BSPFace);
								Q3BSPFace[] Faces = new Q3BSPFace[LumpLen / BSPFaceSize];
								Q3BSPFace* FacesPtr = (Q3BSPFace*)LumpPtr;

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

		public static libTechModel LoadAsModel(Stream BSPStream) {
			Q3BSP Map = Q3BSP.FromStream(BSPStream);

			libTechModel Model = new libTechModel();
			Dictionary<int, List<Vertex3>> TexturedMeshes = new Dictionary<int, List<Vertex3>>();

			foreach (var FaceIdx in GetFaceIndices(Map)) {
				Q3BSPFace Face = Map.Faces[FaceIdx];

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

		static IEnumerable<int> GetFaceIndices(Q3BSP Map) {
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

	public static class ValveBSP {
		public static libTechModel LoadAsModel(string FilePath) {
			// TODO: Ugh, fix
			FilePath = Path.GetFullPath("." + FilePath);

			libTechModel libTechModel = new libTechModel();
			Dictionary<string, List<Vertex3>> TexturedMeshes = new Dictionary<string, List<Vertex3>>();

			ValveBspFile BSP = new ValveBspFile(FilePath);
			Engine.VFS.GetSourceProvider().Add(Path.GetFileName(FilePath), BSP.PakFile);

			Face[] Faces = BSP.Faces.ToArray();
			SVector3[] Verts = BSP.Vertices.ToArray();
			BspModel[] Models = BSP.Models.ToArray();
			Edge[] Edges = BSP.Edges.ToArray();
			int[] SurfEdges = BSP.SurfEdges.ToArray();
			TextureInfo[] TexInfos = BSP.TextureInfos.ToArray();
			TextureData[] TexDatas = BSP.TextureData.ToArray();
			var Entities = BSP.Entities.ToArray();

			for (int ModelIdx = 0; ModelIdx < Models.Length; ModelIdx++) {
				if (ModelIdx > 0)
					continue;

				ref BspModel Model = ref Models[ModelIdx];

				for (int FaceIdx = Model.FirstFace; FaceIdx < Model.FirstFace + Model.NumFaces; FaceIdx++) {
					ref Face Face = ref Faces[FaceIdx];
					ref TextureInfo TexInfo = ref TexInfos[Face.TexInfo];
					ref TextureData TexData = ref TexDatas[TexInfo.TexData];
					string MatName = "/" + BSP.GetTextureString(TexData.NameStringTableId);
					SVector2 TexScale = new SVector2(1f / Math.Max(TexData.Width, 1), 1f / Math.Max(TexData.Height, 1));

					if (MatName == "/GLASS/REFLECTIVEGLASS001")
						Debugger.Break();

					if (!MatName.StartsWith("/materials"))
						MatName = "/materials" + MatName;
					if (!MatName.EndsWith(".vmt"))
						MatName += ".vmt";

					if ((TexInfo.Flags & (SurfFlags.NODRAW | SurfFlags.LIGHT | SurfFlags.SKY | SurfFlags.SKY2D)) != 0)
						continue;

					/*if ((TexInfo.Flags & SurfFlags.TRANS) != 0)
						Debugger.Break();*/

					// Displacements
					if (Face.DispInfo != -1) {

					} else {
						int TriangleVert = 0;
						Vertex3[] Triangle = new Vertex3[3];

						for (int EdgeIdx = Face.FirstEdge; EdgeIdx < Face.FirstEdge + Face.NumEdges; EdgeIdx++) {
							int SurfEdge = SurfEdges[EdgeIdx];
							int VertIdx = SurfEdge < 0 ? VertIdx = Edges[-SurfEdge].B : VertIdx = Edges[SurfEdge].A;

							SVector3 Vert = Verts[VertIdx] + Model.Origin;
							SVector2 UV = GetUV(Vert, TexInfo.TextureUAxis, TexInfo.TextureVAxis) * TexScale;

							Triangle[TriangleVert++] = new Vertex3(ToVec3(Vert), ToVec2(UV));

							if (TriangleVert > 2) {
								if (!TexturedMeshes.ContainsKey(MatName))
									TexturedMeshes.Add(MatName, new List<Vertex3>());

								TexturedMeshes[MatName].AddRange(Triangle);

								Triangle[1] = Triangle[2];
								TriangleVert = 2;
							}
						}
					}
				}
			}

			foreach (var KV in TexturedMeshes) {
				if (KV.Value.Count > 0) {
					Materials.ValveMaterial Mat = (Materials.ValveMaterial)Engine.GetMaterial(KV.Key);
					bool ErrorTexture = Mat.Texture == Engine.ErrorTexture;
					
					libTechModel.AddMesh(new libTechMesh(KV.Value.ToArray(), Mat));
				}
			}

			libTechModel.CenterModel();
			return libTechModel;
		}

		static SVector2 GetUV(SVector3 Pos, TexAxis UAxis, TexAxis VAxis) {
			return new SVector2(Pos.Dot(UAxis.Normal) + UAxis.Offset, Pos.Dot(VAxis.Normal) + VAxis.Offset);
		}

		static Vector3 ToVec3(SVector3 V) {
			return new Vector3(-V.X, V.Z, V.Y);
		}

		static Vector2 ToVec2(SVector2 V) {
			return new Vector2(V.X, 1.0f - V.Y);
		}

		static IEnumerable<int> GetFaceIndices(ValveBspFile Map) {
			HashSet<int> AlreadyVisible = new HashSet<int>();

			foreach (var L in Map.Leaves) {
				/*if (L.Cluster < 0)
					continue;*/

				for (int FaceOffset = 0; FaceOffset < L.NumLeafFaces; FaceOffset++) {
					int FaceIdx = L.FirstLeafFace + FaceOffset;

					if (AlreadyVisible.Contains(FaceIdx))
						continue;

					AlreadyVisible.Add(FaceIdx);
					yield return Map.LeafFaces[FaceIdx];
				}
			}
		}
	}

	public static class BSPMap {
		public static libTechModel LoadAsModel(string FilePath) {
			string HeaderVer;

			using (Stream BSPStream = Engine.VFS.OpenFile(FilePath)) {
				BSPHeader Header;

				using (BinaryReader BR = new BinaryReader(BSPStream)) {
					Header = BR.ReadStruct<BSPHeader>();
					HeaderVer = Header.ToString();
					BSPStream.Seek(0, SeekOrigin.Begin);
				}

				if (HeaderVer == "IBSP46" || HeaderVer == "IBSP47")
					return Q3BSP.LoadAsModel(BSPStream);
			}

			if (HeaderVer.StartsWith("VBSP"))
				return ValveBSP.LoadAsModel(FilePath);

			throw new Exception("Unsupported BSP format " + HeaderVer);
		}
	}
}