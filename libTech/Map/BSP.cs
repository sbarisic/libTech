using FishGfx;
using FishGfx.Formats;
using FishGfx.Graphics;
using FishGfx.Graphics.Drawables;
using libTech.Entities;
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
using Plane = System.Numerics.Plane;
using SVector2 = SourceUtils.Vector2;
using SVector3 = SourceUtils.Vector3;
using VBSPPlane = SourceUtils.ValveBsp.Plane;
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

		public static libTechMap LoadMap(Stream BSPStream) {
			Q3BSP Map = Q3BSP.FromStream(BSPStream);

			libTechMap Q3Map = new libTechMap();
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

			libTechModel Model = new libTechModel();
			Q3Map.AddModel(Model);

			foreach (var KV in TexturedMeshes) {
				string TexName = TextureNames[KV.Key];

				if (TexName == "textures/lun3dm5/lblusky2")
					continue;

				Model.AddMesh(new libTechMesh(KV.Value.ToArray().Reverse().ToArray(), Engine.GetMaterial(TexName)));
			}

			return Q3Map;
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
		public static libTechMap LoadMap(string FilePath) {
			// TODO: Ugh, fix
			FilePath = Path.GetFullPath("." + FilePath);

			libTechMap Map = new libTechMap();
			libTechModel CurrentMapModel = null;

			using (ValveBspFile BSP = new ValveBspFile(FilePath)) {
				string BSPProviderPath = Path.GetFileName(FilePath);
				Engine.VFS.GetSourceProvider().Add(BSPProviderPath, BSP.PakFile);

				Face[] Faces = BSP.Faces.ToArray();
				SVector3[] Verts = BSP.Vertices.ToArray();
				BspModel[] Models = BSP.Models.ToArray();
				Edge[] Edges = BSP.Edges.ToArray();
				int[] SurfEdges = BSP.SurfEdges.ToArray();
				TextureInfo[] TexInfos = BSP.TextureInfos.ToArray();
				TextureData[] TexDatas = BSP.TextureData.ToArray();
				Brush[] Brushes = BSP.Brushes.ToArray();
				BrushSide[] Sides = BSP.BrushSides.ToArray();

				string[] SpawnEntityNames = new string[] { "info_player_start", "info_player_deathmatch", "info_player_terrorist", "info_player_counterterrorist", "info_coop_spawn" };
				foreach (var VEnt in BSP.Entities) {
					if (SpawnEntityNames.Contains(VEnt.ClassName)) {
						Vector3 Angles = ToVec3(VEnt.Angles);
						Vector3 Origin = ToVec3(VEnt.Origin);
						Map.SpawnEntity(new PlayerSpawn(Origin, Quaternion.CreateFromYawPitchRoll(Angles.X, Angles.Y, Angles.Z)));
					}
				}

				List<Vertex3> DispTriangleStrip = new List<Vertex3>();

				for (int ModelIdx = 0; ModelIdx < Models.Length; ModelIdx++) {
					ref BspModel Model = ref Models[ModelIdx];

					CurrentMapModel = new libTechModel();
					if (ModelIdx > 0)
						CurrentMapModel.Enabled = false;

					Map.AddModel(CurrentMapModel);
					Dictionary<string, List<Vertex3>> TexturedMeshes = new Dictionary<string, List<Vertex3>>();

					for (int FaceIdx = Model.FirstFace; FaceIdx < Model.FirstFace + Model.NumFaces; FaceIdx++) {
						ref Face Face = ref Faces[FaceIdx];
						ref TextureInfo TexInfo = ref TexInfos[Face.TexInfo];

						if ((TexInfo.Flags & (SurfFlags.NODRAW | SurfFlags.LIGHT | SurfFlags.SKY | SurfFlags.SKY2D)) != 0)
							continue;

						ref TextureData TexData = ref TexDatas[TexInfo.TexData];
						SVector2 TexScale = new SVector2(1f / Math.Max(TexData.Width, 1), 1f / Math.Max(TexData.Height, 1));

						// TODO: Is this correct? A better way to fix?

						string TexName = BSP.GetTextureString(TexData.NameStringTableId);
						if (TexName.ToLower().EndsWith("nodraw"))
							continue;

						string MatName = "/" + TexName;
						if (!MatName.StartsWith("/materials"))
							MatName = "/materials" + MatName;
						if (!MatName.EndsWith(".vmt"))
							MatName += ".vmt";

						// TODO: Better way to add water
						if (MatName.ToLower().Contains("water"))
							MatName = "water";

						// Displacements
						if (Face.DispInfo != -1) {
							Displacement Disp = BSP.DisplacementManager[Face.DispInfo];
							Disp.GetCorners(out SVector3 C0, out SVector3 C1, out SVector3 C2, out SVector3 C3);

							SVector2 UV00 = GetUV(C0, TexInfo.TextureUAxis, TexInfo.TextureVAxis) * TexScale;
							SVector2 UV10 = GetUV(C3, TexInfo.TextureUAxis, TexInfo.TextureVAxis) * TexScale;
							SVector2 UV01 = GetUV(C1, TexInfo.TextureUAxis, TexInfo.TextureVAxis) * TexScale;
							SVector2 UV11 = GetUV(C2, TexInfo.TextureUAxis, TexInfo.TextureVAxis) * TexScale;

							float SubDivMul = 1f / Disp.Subdivisions;

							for (int Y = 0; Y < Disp.Subdivisions; Y++) {
								DispTriangleStrip.Clear();

								var V0 = (Y + 0) * SubDivMul;
								var V1 = (Y + 1) * SubDivMul;

								for (int X = 0; X < Disp.Size; X++) {
									var U = X * SubDivMul;

									SVector3 Pos1 = Disp.GetPosition(X, Y + 0);
									SVector2 UV1 = (UV00 * (1f - U) + UV10 * U) * (1f - V0) + (UV01 * (1f - U) + UV11 * U) * V0;
									float Alpha1 = Disp.GetAlpha(X, Y + 0);
									DispTriangleStrip.Add(new Vertex3(ToVec3(Pos1), ToVec2(UV1)));

									SVector3 Pos2 = Disp.GetPosition(X, Y + 1);
									SVector2 UV2 = (UV00 * (1f - U) + UV10 * U) * (1f - V1) + (UV01 * (1f - U) + UV11 * U) * V1;
									float Alpha2 = Disp.GetAlpha(X, Y + 1);
									DispTriangleStrip.Add(new Vertex3(ToVec3(Pos2), ToVec2(UV2)));
								}

								if (!TexturedMeshes.ContainsKey(MatName))
									TexturedMeshes.Add(MatName, new List<Vertex3>());
								List<Vertex3> VertList = TexturedMeshes[MatName];

								for (int i = 2; i < DispTriangleStrip.Count; i++) {
									if (i % 2 != 0) {
										VertList.Add(DispTriangleStrip[i - 2]);
										VertList.Add(DispTriangleStrip[i]);
										VertList.Add(DispTriangleStrip[i - 1]);
									} else {
										VertList.Add(DispTriangleStrip[i - 2]);
										VertList.Add(DispTriangleStrip[i - 1]);
										VertList.Add(DispTriangleStrip[i]);
									}
								}
							}
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

					foreach (var KV in TexturedMeshes) {
						if (KV.Value.Count > 0) {
							Materials.Material Mat = Engine.GetMaterial(KV.Key);
							CurrentMapModel.AddMesh(new libTechMesh(KV.Value.ToArray(), Mat));
						}
					}
				}

				Engine.VFS.GetSourceProvider().Remove(BSPProviderPath);
			}

			return Map;
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
		public static libTechMap LoadMap(string FilePath) {
			string HeaderVer;

			using (Stream BSPStream = Engine.VFS.OpenFile(FilePath)) {
				BSPHeader Header;

				using (BinaryReader BR = new BinaryReader(BSPStream)) {
					Header = BR.ReadStruct<BSPHeader>();
					HeaderVer = Header.ToString();
					BSPStream.Seek(0, SeekOrigin.Begin);
				}

				if (HeaderVer == "IBSP46" || HeaderVer == "IBSP47")
					return Q3BSP.LoadMap(BSPStream);
			}

			if (HeaderVer.StartsWith("VBSP"))
				return ValveBSP.LoadMap(FilePath);

			throw new Exception("Unsupported BSP format " + HeaderVer);
		}
	}
}