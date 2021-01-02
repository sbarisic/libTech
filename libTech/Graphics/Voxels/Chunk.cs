using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using libTech.Models;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FishGfx;
using System.Numerics;

namespace libTech.Graphics.Voxels {
	class ChunkPalette {
		Dictionary<int, BlockType> Palette;

		public ChunkPalette() {
			Palette = new Dictionary<int, BlockType>() {
				{ 0, BlockType.None },
				{ 0x847E87, BlockType.Stone }, // floor grey
				{ 0x8A6F30, BlockType.Dirt }, // wall brown
				{ 0x9BADB7, BlockType.Plank }, // metal blue
				{ 0x6ABE30, BlockType.Leaf }, // green
			};
		}

		public BlockType GetBlock(string Str) {
			int BlockClr = int.Parse(Str, System.Globalization.NumberStyles.HexNumber);

			if (Palette.ContainsKey(BlockClr))
				return Palette[BlockClr];

			return BlockType.Sand;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct BlockLight {
		public static readonly BlockLight Black = new BlockLight(0, 0, 0);
		public static readonly BlockLight Ambient = new BlockLight(LightLevels);
		public static readonly BlockLight FullBright = new BlockLight(256 / LightLevels);

		[FieldOffset(0)]
		public byte R;

		[FieldOffset(1)]
		public byte G;

		[FieldOffset(2)]
		public byte B;

		[FieldOffset(3)]
		byte Unused;

		[FieldOffset(0)]
		public int LightInteger;

		const int LightLevels = 8;

		public BlockLight(byte R, byte G, byte B) {
			LightInteger = Unused = 0;

			this.R = R;
			this.G = G;
			this.B = B;
		}

		public BlockLight(byte Amt) {
			LightInteger = Unused = 0;

			R = G = B = Amt;
		}

		public void Increase(byte Amt) {
			if (R + Amt > 255)
				R = 255;
			else
				R += Amt;

			if (G + Amt > 255)
				G = 255;
			else
				G += Amt;

			if (B + Amt > 255)
				B = 255;
			else
				B += Amt;
		}

		public void SetMin(byte Amt) {
			if (R < Amt)
				R = Amt;

			if (G < Amt)
				G = Amt;

			if (B < Amt)
				B = Amt;
		}

		public void Set(byte Amt) {
			R = Amt;
			G = Amt;
			B = Amt;
		}

		public Color ToColor() {
			byte RR = (byte)Utils.Clamp(R * LightLevels, 0, 255);
			byte GG = (byte)Utils.Clamp(G * LightLevels, 0, 255);
			byte BB = (byte)Utils.Clamp(B * LightLevels, 0, 255);
			return new Color(RR, GG, BB);
		}
	}

	class PlacedBlock {
		public BlockType Type;

		// Recalculated, always 6
		BlockLight[] Lights;

		public PlacedBlock(BlockType Type, BlockLight DefaultLight) {
			Lights = new BlockLight[6];

			for (int i = 0; i < Lights.Length; i++)
				Lights[i] = DefaultLight;

			this.Type = Type;
		}

		public PlacedBlock(BlockType Type) : this(Type, BlockLight.FullBright) {
		}

		public PlacedBlock(PlacedBlock Copy) : this(Copy.Type) {
			for (int i = 0; i < Lights.Length; i++)
				Lights[i] = Copy.Lights[i];
		}

		public void SetBlockLight(BlockLight L) {
			for (int i = 0; i < Lights.Length; i++)
				Lights[i] = L;
		}

		public void SetBlockLight(Vector3 Dir, BlockLight L) {
			Lights[Utils.DirToByte(Dir)] = L;
		}

		public BlockLight GetBlockLight(Vector3 Dir) {
			return Lights[Utils.DirToByte(Dir)];
		}

		public Color GetColor(Vector3 Normal) {
			return Lights[Utils.DirToByte(Normal)].ToColor();
		}

		// Serialization stuff

		public void Write(BinaryWriter Writer) {
			Writer.Write((ushort)Type);

			/*for (int i = 0; i < Lights.Length; i++)
				Writer.Write(Lights[i].LightInteger);*/
		}

		public void Read(BinaryReader Reader) {
			Type = (BlockType)Reader.ReadUInt16();

			/*for (int i = 0; i < Lights.Length; i++)
				Lights[i].LightInteger = Reader.ReadInt32();*/
		}
	}

	unsafe class Chunk {
		public const int ChunkSize = 16;
		public const float BlockSize = 1;
		public const int AtlasSize = 16;

		PlacedBlock[] Blocks;
		bool Dirty;

		libTechModel CachedModelOpaque;
		libTechMesh CachedMeshOpaque;

		public Color ChunkColor = Color.White;

		Vector3 GlobalChunkIndex;
		ChunkMap WorldMap;


		public Chunk(Vector3 GlobalChunkIndex, ChunkMap WorldMap) {
			this.GlobalChunkIndex = GlobalChunkIndex;
			this.WorldMap = WorldMap;

			Blocks = new PlacedBlock[ChunkSize * ChunkSize * ChunkSize];
			for (int i = 0; i < Blocks.Length; i++)
				Blocks[i] = new PlacedBlock(BlockType.None);

			Dirty = true;
			//int TileTexSize = AtlasTex.width / 16;
		}

		public void Write(BinaryWriter Writer) {
			for (int i = 0; i < Blocks.Length;) {
				PlacedBlock Cur = Blocks[i];
				ushort Count = 1;

				for (int j = i + 1; j < Blocks.Length; j++) {
					if (Blocks[j].Type == Cur.Type)
						Count++;
					else
						break;
				}

				Writer.Write(Count);
				Cur.Write(Writer);

				i += Count;
			}
		}

		public void Read(BinaryReader Reader) {
			for (int i = 0; i < Blocks.Length;) {
				ushort Count = Reader.ReadUInt16();

				PlacedBlock Block = new PlacedBlock(BlockType.None);
				Block.Read(Reader);

				for (int j = 0; j < Count; j++)
					Blocks[i + j] = new PlacedBlock(Block);

				i += Count;
			}

			Dirty = true;
		}

		public PlacedBlock GetBlock(int X, int Y, int Z) {
			/*if (X < 0 || X >= ChunkSize)
				return PlacedBlock.None;

			if (Y < 0 || Y >= ChunkSize)
				return PlacedBlock.None;

			if (Z < 0 || Z >= ChunkSize)
				return PlacedBlock.None;*/

			if (X < 0 || X >= ChunkSize || Y < 0 || Y >= ChunkSize || Z < 0 || Z >= ChunkSize) {
				WorldMap.GetWorldPos(0, 0, 0, GlobalChunkIndex, out Vector3 GlobalBlockPos);
				return WorldMap.GetPlacedBlock((int)GlobalBlockPos.X + X, (int)GlobalBlockPos.Y + Y, (int)GlobalBlockPos.Z + Z, out Chunk Chk);
			}

			return Blocks[X + ChunkSize * (Y + ChunkSize * Z)];
		}

		public void SetBlock(int X, int Y, int Z, PlacedBlock Block) {
			Blocks[X + ChunkSize * (Y + ChunkSize * Z)] = Block;
			Dirty = true;
		}

		public void Fill(PlacedBlock Block) {
			for (int i = 0; i < Blocks.Length; i++)
				Blocks[i] = Block;
			Dirty = true;
		}

		public void Fill(BlockType T) {
			Fill(new PlacedBlock(T));
		}

		public void MarkDirty() {
			Dirty = true;
		}

		void PrintVert(Vector3 V) {
			Console.WriteLine("new Vector3({0}, {1}, {2}) * Size + Pos,", V.X, V.Y, V.Z);
		}


		public void To3D(int Idx, out int X, out int Y, out int Z) {
			Z = Idx / (ChunkSize * ChunkSize);
			Idx -= (Z * ChunkSize * ChunkSize);
			Y = Idx / ChunkSize;
			X = Idx % ChunkSize;
		}

		bool IsCovered(int X, int Y, int Z) {
			for (int i = 0; i < Utils.MainDirs.Length; i++) {
				int XX = (int)(X + Utils.MainDirs[i].X);
				int YY = (int)(Y + Utils.MainDirs[i].Y);
				int ZZ = (int)(Z + Utils.MainDirs[i].Z);

				if (GetBlock(XX, YY, ZZ).Type == BlockType.None)
					return false;
			}

			return true;
		}

		void ComputeLighting() {
			for (int i = 0; i < Blocks.Length; i++) {
				PlacedBlock Block = Blocks[i];
				if (Block.Type == BlockType.None)
					continue;

				To3D(i, out int LocalX, out int LocalY, out int LocalZ);
				if (IsCovered(LocalX, LocalY, LocalZ))
					continue;

				WorldMap.GetWorldPos(LocalX, LocalY, LocalZ, GlobalChunkIndex, out Vector3 GlobalPos);

				int X = (int)GlobalPos.X;
				int Y = (int)GlobalPos.Y;
				int Z = (int)GlobalPos.Z;

				if (WorldMap.IsCovered(X, Y, Z))
					continue;

				/*// Ambient occlusion
				for (int j = 0; j < Utils.MainDirs.Length; j++) {
					Vector3 Origin = new Vector3(X, Y, Z) + Utils.MainDirs[j];

					//float AmbientHits = (float)WorldMap.CountHits((int)Origin.X, (int)Origin.Y, (int)Origin.Z, 2, Utils.MainDirs[j], out int MaxHits) / MaxHits;
					float AmbientHitRatio = ((float)WorldMap.CountAmbientHits(Origin) - 1) / 5;

					int Light = 32 - (int)(AmbientHitRatio * 24);

					Block.Lights[Utils.DirToByte(Utils.MainDirs[j])] = new BlockLight((byte)(Light));
				}*/

				// TODO: Actual lights

				// Set block back into world
				// SetPlacedBlock(X, Y, Z, Block, false);
			}
		}

		Color CalcAOColor(Vector3 GlobalBlockPos, Vector3 A, Vector3 B, Vector3 C) {
			int Hits = 0;

			if (BlockInfo.IsOpaque(WorldMap.GetBlock(GlobalBlockPos + A)))
				Hits++;

			if (BlockInfo.IsOpaque(WorldMap.GetBlock(GlobalBlockPos + B)))
				Hits++;

			if (BlockInfo.IsOpaque(WorldMap.GetBlock(GlobalBlockPos + C)))
				Hits++;

			if (Hits != 0)
				return new Color(0.8f, 0.8f, 0.8f); // 0.9f*/

			return Color.White;
		}

		void SetBlockTextureUV(BlockType BlockType, Vector3 FaceNormal, MeshBuilder Verts) {
			int BlockID = (int)BlockType - 1;

			if (BlockType == BlockType.Grass) {
				if (FaceNormal.Y == 1)
					BlockID = 240;
				else if (FaceNormal.Y == 0)
					BlockID = 241;
				else
					BlockID = 1;
			} else if (BlockType == BlockType.Grass) {
				if (FaceNormal.Y == 0)
					BlockID = 242;
				else
					BlockID = 243;
			}

			int BlockX = BlockID % AtlasSize;
			int BlockY = BlockID / AtlasSize;

			Vector2 UVSize = new Vector2(1.0f / AtlasSize, 1.0f / AtlasSize);
			Vector2 UVPos = UVSize * new Vector2(BlockX, BlockY);
			Verts.SetUVOffsetSize(UVPos + new Vector2(0, UVSize.Y), UVSize * new Vector2(1, -1));
		}

		Vertex3[] GenMesh() {
			MeshBuilder Vertices = new MeshBuilder();
			Vector3 Size = new Vector3(BlockSize);
			Color AOColor = new Color(128, 128, 128);
			AOColor = AOColor * AOColor;

			for (int x = 0; x < ChunkSize; x++) {
				for (int y = 0; y < ChunkSize; y++) {
					for (int z = 0; z < ChunkSize; z++) {
						WorldMap.GetWorldPos(x, y, z, GlobalChunkIndex, out Vector3 GlobalBlockPos);

						PlacedBlock CurBlock = null;
						if ((CurBlock = GetBlock(x, y, z)).Type != BlockType.None) {
							Vertices.SetPositionOffset(new Vector3(x, y, z) * BlockSize);

							BlockType XPosType = GetBlock(x + 1, y, z).Type;
							BlockType XNegType = GetBlock(x - 1, y, z).Type;
							BlockType YPosType = GetBlock(x, y + 1, z).Type;
							BlockType YNegType = GetBlock(x, y - 1, z).Type;
							BlockType ZPosType = GetBlock(x, y, z + 1).Type;
							BlockType ZNegType = GetBlock(x, y, z - 1).Type;

							bool XPosSkipFace = false;
							bool XNegSkipFace = false;
							bool YPosSkipFace = false;
							bool YNegSkipFace = false;
							bool ZPosSkipFace = false;
							bool ZNegSkipFace = false;

							if (BlockInfo.IsOpaque(CurBlock.Type)) {
								XPosSkipFace = BlockInfo.IsOpaque(XPosType);
								XNegSkipFace = BlockInfo.IsOpaque(XNegType);
								YPosSkipFace = BlockInfo.IsOpaque(YPosType);
								YNegSkipFace = BlockInfo.IsOpaque(YNegType);
								ZPosSkipFace = BlockInfo.IsOpaque(ZPosType);
								ZNegSkipFace = BlockInfo.IsOpaque(ZNegType);
							}

							// X++
							if (!XPosSkipFace) {
								Vector3 CurDir = new Vector3(1, 0, 0);
								Color FaceClr = Color.White; // Color FaceClr = CurBlock.GetBlockLight(CurDir).ToColor();

								SetBlockTextureUV(CurBlock.Type, CurDir, Vertices);

								Vertices.Add(new Vector3(1, 1, 0), new Vector2(1, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(1, 0, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 0)));
								Vertices.Add(new Vector3(1, 1, 1), new Vector2(0, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1)));
								Vertices.Add(new Vector3(1, 0, 1), new Vector2(0, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(1, -1, 0), new Vector3(1, -1, 1), new Vector3(1, 0, 1)));
								Vertices.Add(new Vector3(1, 0, 0), new Vector2(1, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(1, 0, -1), new Vector3(1, -1, -1), new Vector3(1, -1, 0)));
								Vertices.Add(new Vector3(1, 1, 0), new Vector2(1, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(1, 0, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 0)));
								Vertices.Add(new Vector3(1, 0, 1), new Vector2(0, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(1, -1, 0), new Vector3(1, -1, 1), new Vector3(1, 0, 1)));
							}

							// X--
							if (!XNegSkipFace) {
								Vector3 CurDir = new Vector3(-1, 0, 0);
								Color FaceClr = Color.White; // Color FaceClr = CurBlock.GetBlockLight(CurDir).ToColor();
								SetBlockTextureUV(CurBlock.Type, CurDir, Vertices);

								Vertices.Add(new Vector3(0, 1, 1), new Vector2(1, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, 1, 0), new Vector3(-1, 1, 1), new Vector3(-1, 0, 1)));
								Vertices.Add(new Vector3(0, 1, 0), new Vector2(0, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, 0, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 0)));
								Vertices.Add(new Vector3(0, 0, 0), new Vector2(0, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, 0, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 0)));
								Vertices.Add(new Vector3(0, 0, 1), new Vector2(1, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, -1, 0), new Vector3(-1, -1, 1), new Vector3(-1, 0, 1)));
								Vertices.Add(new Vector3(0, 1, 1), new Vector2(1, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, 1, 0), new Vector3(-1, 1, 1), new Vector3(-1, 0, 1)));
								Vertices.Add(new Vector3(0, 0, 0), new Vector2(0, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, 0, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 0)));
							}

							// Y++
							if (!YPosSkipFace) {
								Vector3 CurDir = new Vector3(0, 1, 0);
								Color FaceClr = Color.White; // Color FaceClr = CurBlock.GetBlockLight(CurDir).ToColor();
								SetBlockTextureUV(CurBlock.Type, CurDir, Vertices);

								Vertices.Add(new Vector3(1, 1, 0), new Vector2(1, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, 1, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 0)));
								Vertices.Add(new Vector3(0, 1, 0), new Vector2(0, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 0)));
								Vertices.Add(new Vector3(0, 1, 1), new Vector2(0, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, 1, 0), new Vector3(-1, 1, 1), new Vector3(0, 1, 1)));
								Vertices.Add(new Vector3(1, 1, 1), new Vector2(1, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1)));
								Vertices.Add(new Vector3(1, 1, 0), new Vector2(1, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, 1, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 0)));
								Vertices.Add(new Vector3(0, 1, 1), new Vector2(0, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, 1, 0), new Vector3(-1, 1, 1), new Vector3(0, 1, 1)));
							}

							// Y--
							if (!YNegSkipFace) {
								Vector3 CurDir = new Vector3(0, -1, 0);
								Color FaceClr = Color.White; // Color FaceClr = CurBlock.GetBlockLight(CurDir).ToColor();
								SetBlockTextureUV(CurBlock.Type, CurDir, Vertices);

								Vertices.Add(new Vector3(1, 0, 1), new Vector2(0, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, -1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, 0)));
								Vertices.Add(new Vector3(0, 0, 1), new Vector2(1, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, 0)));
								Vertices.Add(new Vector3(0, 0, 0), new Vector2(1, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, -1, 0), new Vector3(-1, -1, -1), new Vector3(0, -1, -1)));
								Vertices.Add(new Vector3(1, 0, 0), new Vector2(0, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(1, -1, 0), new Vector3(1, -1, -1), new Vector3(0, -1, -1)));
								Vertices.Add(new Vector3(1, 0, 1), new Vector2(0, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, -1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, 0)));
								Vertices.Add(new Vector3(0, 0, 0), new Vector2(1, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, -1, 0), new Vector3(-1, -1, -1), new Vector3(0, -1, -1)));
							}

							// Z++
							if (!ZPosSkipFace) {
								Vector3 CurDir = new Vector3(0, 0, 1);
								Color FaceClr = Color.White; // Color FaceClr = CurBlock.GetBlockLight(CurDir).ToColor();
								SetBlockTextureUV(CurBlock.Type, CurDir, Vertices);

								Vertices.Add(new Vector3(1, 0, 1), new Vector2(1, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, -1, 1), new Vector3(1, -1, 1), new Vector3(1, 0, 1)));
								Vertices.Add(new Vector3(1, 1, 1), new Vector2(1, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 0, 1)));
								Vertices.Add(new Vector3(0, 1, 1), new Vector2(0, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, 0, 1)));
								Vertices.Add(new Vector3(0, 0, 1), new Vector2(0, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, 0, 1), new Vector3(-1, -1, 1), new Vector3(0, -1, 1)));
								Vertices.Add(new Vector3(1, 0, 1), new Vector2(1, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, -1, 1), new Vector3(1, -1, 1), new Vector3(1, 0, 1)));
								Vertices.Add(new Vector3(0, 1, 1), new Vector2(0, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, 0, 1)));
							}

							// Z--
							if (!ZNegSkipFace) {
								Vector3 CurDir = new Vector3(0, 0, -1);
								Color FaceClr = Color.White; // Color FaceClr = CurBlock.GetBlockLight(CurDir).ToColor();
								SetBlockTextureUV(CurBlock.Type, CurDir, Vertices);

								Vertices.Add(new Vector3(1, 1, 0), new Vector2(0, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, 1, -1), new Vector3(1, 1, -1), new Vector3(1, 0, -1)));
								Vertices.Add(new Vector3(1, 0, 0), new Vector2(0, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, -1, -1), new Vector3(1, -1, -1), new Vector3(1, 0, -1)));
								Vertices.Add(new Vector3(0, 0, 0), new Vector2(1, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 0, -1)));
								Vertices.Add(new Vector3(0, 1, 0), new Vector2(1, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(-1, 0, -1), new Vector3(-1, 1, -1), new Vector3(0, 1, -1)));
								Vertices.Add(new Vector3(1, 1, 0), new Vector2(0, 1), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, 1, -1), new Vector3(1, 1, -1), new Vector3(1, 0, -1)));
								Vertices.Add(new Vector3(0, 0, 0), new Vector2(1, 0), FaceClr * CalcAOColor(GlobalBlockPos, new Vector3(0, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 0, -1)));
							}
						}

						//GraphicsUtils.AppendBlockVertices(Verts, new Vector3(x, y, z) * BlockSize, new Vector3(BlockSize));
					}
				}
			}

			return Vertices.ToArray().Reverse().ToArray();
		}

		libTechModel GetModel() {
			if (!Dirty)
				return CachedModelOpaque;

			Dirty = false;

			if (CachedMeshOpaque == null)
				CachedMeshOpaque = new libTechMesh();

			CachedMeshOpaque.Material = WorldMap.Material;
			CachedMeshOpaque.SetVertices(GenMesh());
			CachedMeshOpaque.MeshMatrix = Matrix4x4.CreateScale(10);

			if (CachedModelOpaque == null) {
				CachedModelOpaque = new libTechModel();
				CachedModelOpaque.AddMesh(CachedMeshOpaque);
			}

			// TODO: Fix shit
			/*if (ModelValid) {
				CachedModelOpaque.materials[0].maps[0].texture.id = 1;
				Raylib.UnloadModel(CachedModelOpaque);
			}

			CachedModelOpaque = Raylib.LoadModelFromMesh(CachedMeshOpaque);
			CachedModelOpaque.materials[0].maps[0].texture = ResMgr.AtlasTexture;*/

			return CachedModelOpaque;
		}

		public void Draw(Vector3 Position) {
			// TODO: Draw chunk model
			//Raylib.DrawModel(GetModel(), Position, BlockSize, ChunkColor);

			libTechModel Model = GetModel();
			Model.Position = Position;
			Model.DrawOpaque();
		}

		public void DrawTransparent(Vector3 Position) {

		}
	}
}
