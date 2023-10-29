using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace libTech.Graphics.Voxels {
	struct GlobalPlacedBlock {
		public Vector3 GlobalPos;
		public PlacedBlock Block;
		public Chunk Chunk;

		public GlobalPlacedBlock(Vector3 GlobalPos, PlacedBlock Block, Chunk Chunk) {
			this.GlobalPos = GlobalPos;
			this.Block = Block;
			this.Chunk = Chunk;
		}
	}

	class ChunkMap {
		public Materials.Material Material;
		List<Chunk> Chunks = new List<Chunk>();

		public ChunkMap(Materials.Material Material) {
			this.Material = Material;
		}

		/*public void LoadFromChunk(string FileName) {
			string[] Lines = File.ReadAllText(FileName).Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			ChunkPalette Palette = new ChunkPalette();

			foreach (var L in Lines) {
				string Line = L.Trim();
				if (Line.StartsWith("#"))
					continue;

				string[] XYZT = Line.Split(new[] { ' ' });

				// Swapped for reasons
				int Z = int.Parse(XYZT[0]);
				int X = int.Parse(XYZT[1]);
				int Y = int.Parse(XYZT[2]);

				SetBlock(X, Y, Z, Palette.GetBlock(XYZT[3]));
			}
		}*/

		float Simplex(int Octaves, float X, float Y, float Z, float Scale) {
			float Val = 0.0f;

			for (int i = 0; i < Octaves; i++)
				Val += Noise.CalcPixel3D(X * Math.Pow(2, i), Y * Math.Pow(2, i), Z * Math.Pow(2, i), Scale);

			return (Val / Octaves) / 255;
		}

		void MinMax(float Val, ref float Min, ref float Max) {
			if (Val < Min)
				Min = Val;

			if (Val > Max)
				Max = Val;
		}

		public bool ContainsChunk(Vector3i Pos) {
			foreach (Chunk C in Chunks) {
				if (C.Position == Pos)
					return true;
			}

			return false;
		}

		public bool ContainsChunk(Chunk C) {
			return Chunks.Contains(C);
		}

		public void AddChunk(Chunk C) {
			if (ContainsChunk(C))
				throw new Exception("Cannot add chunk twice");

			Chunks.Add(C);
		}

		public Chunk GetChunk(Vector3i ChunkPosition) {
			foreach (Chunk C in Chunks) {
				if (C.Position == ChunkPosition)
					return C;
			}

			throw new Exception("Chunk not found at " + ChunkPosition.ToString());
		}

		public Chunk[] GetAllChunks() {
			return Chunks.ToArray();
		}

		public void GenerateFloatingIsland(int Width, int Length, int Seed = 666) {
			Noise.Seed = Seed;
			float Scale = 0.02f;
			int WorldHeight = 64;

			Vector3 Center = new Vector3(Width, 0, Length) / 2;
			float CenterRadius = Math.Min(Width / 2, Length / 2);

			for (int x = 0; x < Width; x++)
				for (int z = 0; z < Length; z++)
					for (int y = 0; y < WorldHeight; y++) {
						//float YScale = 1.0f - (float)Math.Pow((float)y / WorldHeight, 0.5);

						Vector3 Pos = new Vector3(x, (WorldHeight - y), z);
						float CenterFalloff = 1.0f - Utils.Clamp(((Center - Pos).Length() / CenterRadius) / 1.2f, 0, 1);

						float Height = (float)y / WorldHeight;
						// float HeightScale = Utils.Clamp(Height * 0.5f, 0.0f, 1.0f) * 256;

						const float HeightFallStart = 0.8f;
						const float HeightFallEnd = 1.0f;
						const float HeightFallRange = HeightFallEnd - HeightFallStart;

						float HeightFalloff = 1;
						if (Height <= HeightFallStart)
							HeightFalloff = 1.0f;
						else if (Height > HeightFallStart && Height < HeightFallEnd)
							HeightFalloff = 1.0f - (Height - HeightFallStart) * (HeightFallRange * 10);
						else
							HeightFalloff = 0;

						float Density = Simplex(2, x, y * 0.5f, z, Scale) * CenterFalloff * HeightFalloff;

						if (Density > 0.1f) {
							float Caves = Simplex(1, x, y, z, Scale * 0.5f) * HeightFalloff;

							if (Caves < 0.95f)
								SetBlock(x, y, z, BlockType.Stone);
						}
					}

			for (int x = 0; x < Width; x++)
				for (int z = 0; z < Length; z++) {
					int DownRayHits = 0;

					for (int y = WorldHeight - 1; y >= 0; y--) {

						if (GetBlock(x, y, z) != BlockType.None) {
							DownRayHits++;

							if (DownRayHits == 1)
								SetBlock(x, y, z, BlockType.Grass);
							else if (DownRayHits < 5)
								SetBlock(x, y, z, BlockType.Dirt);
						} else {
							if (DownRayHits != 0)
								break;
						}
					}
				}
		}

		void TransPosScalar(int S, out int ChunkIndex, out int BlockPos) {
			ChunkIndex = (int)Math.Floor((float)S / Chunk.ChunkSize);
			BlockPos = Utils.Mod(S, Chunk.ChunkSize);
		}

		void TranslateChunkPos(int X, int Y, int Z, out Vector3i ChunkPosition, out Vector3 BlockPos) {
			TransPosScalar(X, out int ChkX, out int BlkX);
			TransPosScalar(Y, out int ChkY, out int BlkY);
			TransPosScalar(Z, out int ChkZ, out int BlkZ);

			ChunkPosition = new Vector3i(ChkX, ChkY, ChkZ);
			BlockPos = new Vector3(BlkX, BlkY, BlkZ);
		}

		public void GetWorldPos(Vector3i Local, Vector3i ChunkPosition, out Vector3 GlobalPos) {
			GlobalPos = (Vector3)ChunkPosition * Chunk.ChunkSize + (Vector3)Local;
		}

		void MarkDirty(Vector3i ChunkPosition) {
			/*Vector3 ChunkIndex = new Vector3(ChunkX, ChunkY, ChunkZ);

			if (Chunks.ContainsKey(ChunkIndex))
				Chunks[ChunkIndex].MarkDirty();*/

			foreach (Chunk C in Chunks) {
				if (C.Position == ChunkPosition)
					C.MarkDirty();
			}
		}

		void MarkDirty(int X, int Y, int Z) {
			MarkDirty(new Vector3i(X, Y, Z));
		}

		public void SetPlacedBlock(int X, int Y, int Z, PlacedBlock Block) {
			TranslateChunkPos(X, Y, Z, out Vector3i ChunkPosition, out Vector3 BlockPos);

			int XX = (int)BlockPos.X;
			int YY = (int)BlockPos.Y;
			int ZZ = (int)BlockPos.Z;
			int CX = (int)ChunkPosition.X;
			int CY = (int)ChunkPosition.Y;
			int CZ = (int)ChunkPosition.Z;

			const int MaxBlock = Chunk.ChunkSize - 1;

			// Edge cases, literally
			if (XX == 0)
				MarkDirty(CX - 1, CY, CZ);
			if (YY == 0)
				MarkDirty(CX, CY - 1, CZ);
			if (ZZ == 0)
				MarkDirty(CX, CY, CZ - 1);
			if (XX == MaxBlock)
				MarkDirty(CX + 1, CY, CZ);
			if (YY == MaxBlock)
				MarkDirty(CX, CY + 1, CZ);
			if (ZZ == MaxBlock)
				MarkDirty(CX, CY, CZ + 1);

			// Corners
			if (XX == 0 && YY == 0 && ZZ == 0)
				MarkDirty(CX - 1, CY - 1, CZ - 1);
			if (XX == 0 && YY == 0 && ZZ == MaxBlock)
				MarkDirty(CX - 1, CY - 1, CZ + 1);
			if (XX == 0 && YY == MaxBlock && ZZ == 0)
				MarkDirty(CX - 1, CY + 1, CZ - 1);
			if (XX == 0 && YY == MaxBlock && ZZ == MaxBlock)
				MarkDirty(CX - 1, CY + 1, CZ + 1);
			if (XX == MaxBlock && YY == 0 && ZZ == 0)
				MarkDirty(CX + 1, CY - 1, CZ - 1);
			if (XX == MaxBlock && YY == 0 && ZZ == MaxBlock)
				MarkDirty(CX + 1, CY - 1, CZ + 1);
			if (XX == MaxBlock && YY == MaxBlock && ZZ == 0)
				MarkDirty(CX + 1, CY + 1, CZ - 1);
			if (XX == MaxBlock && YY == MaxBlock && ZZ == MaxBlock)
				MarkDirty(CX + 1, CY + 1, CZ + 1);

			// Diagonals
			if (XX == 0 && YY == 0)
				MarkDirty(CX - 1, CY - 1, CZ);
			if (XX == MaxBlock && YY == MaxBlock)
				MarkDirty(CX + 1, CY + 1, CZ);
			if (XX == 0 && YY == MaxBlock)
				MarkDirty(CX - 1, CY + 1, CZ);
			if (XX == MaxBlock && YY == 0)
				MarkDirty(CX + 1, CY - 1, CZ);
			if (YY == 0 && ZZ == 0)
				MarkDirty(CX, CY - 1, CZ - 1);
			if (YY == MaxBlock && ZZ == MaxBlock)
				MarkDirty(CX, CY + 1, CZ + 1);
			if (YY == 0 && ZZ == MaxBlock)
				MarkDirty(CX, CY - 1, CZ + 1);
			if (YY == MaxBlock && ZZ == 0)
				MarkDirty(CX, CY + 1, CZ - 1);



			/*for (int x = -1; x < 2; x++)
				for (int y = -1; y < 2; y++)
					for (int z = -1; z < 2; z++)
						MarkDirty(ChunkIndex + new Vector3(x, y, z));*/

			if (!ContainsChunk(ChunkPosition)) {
				Chunk Chk = new Chunk(ChunkPosition, this);
				AddChunk(Chk);
			}

			GetChunk(ChunkPosition).SetBlock(XX, YY, ZZ, Block);
		}

		public void SetBlock(int X, int Y, int Z, BlockType T) {
			/*if (Chunk.EmitsLight(T)) {
				SetPlacedBlock(X, Y, Z, new Chunk.PlacedBlock(T, BlockLight.FullBright));

				Vector3 Origin = new Vector3(X, Y, Z);
				int CastDist = 20;

				Utils.RaycastSphere(Origin, CastDist, (XX, YY, ZZ, Norm) => {
					Chunk.PlacedBlock Cur = GetPlacedBlock(XX, YY, ZZ, out Chunk Chk);

					// Ray hit something solid
					if (Chunk.IsOpaque(Cur)) {
						float Dist = (new Vector3(XX, YY, ZZ) - Origin).Length();
						float Amt = Utils.Clamp(1.0f - (Dist / CastDist), 0, 1);

						Cur.Lights[Utils.DirToByte(Norm)].SetMin((byte)(Amt * 32));
						Chk.MarkDirty();
						//return true;
					}

					return false;
				}, 256);
			} else*/

			SetPlacedBlock(X, Y, Z, new PlacedBlock(T));
		}

		IEnumerable<GlobalPlacedBlock> GetAllExistingBlocks() {
			foreach (var C in Chunks) {
				for (int X = 0; X < Chunk.ChunkSize; X++) {
					for (int Y = 0; Y < Chunk.ChunkSize; Y++) {
						for (int Z = 0; Z < Chunk.ChunkSize; Z++) {

							PlacedBlock CurBlock = C.GetBlock(X, Y, Z);
							if (CurBlock.Type == BlockType.None)
								continue;

							GetWorldPos(new Vector3i(X, Y, Z), C.Position, out Vector3 GlobalPos);
							yield return new GlobalPlacedBlock(GlobalPos, CurBlock, C);
						}
					}
				}
			}
		}

		public int CountHits(int X, int Y, int Z, int Distance, Vector3 Dir, out int MaxHits) {
			int Hits = Utils.RaycastHalfSphere(new Vector3(X, Y, Z), Dir, Distance, (XX, YY, ZZ, Face) => {
				if (GetBlock(XX, YY, ZZ) != BlockType.None)
					return true;

				return false;
			}, out MaxHits, 6);

			return Hits;
		}

		public int CountAmbientHits(Vector3 Pos) {
			int Hits = 0;

			for (int i = 0; i < Utils.MainDirs.Length; i++) {
				if (BlockInfo.IsOpaque(GetBlock(Pos + Utils.MainDirs[i])))
					Hits++;
			}

			return Hits;
		}

		public bool IsCovered(int X, int Y, int Z) {
			for (int i = 0; i < Utils.MainDirs.Length; i++) {
				int XX = (int)(X + Utils.MainDirs[i].X);
				int YY = (int)(Y + Utils.MainDirs[i].Y);
				int ZZ = (int)(Z + Utils.MainDirs[i].Z);

				if (GetBlock(XX, YY, ZZ) == BlockType.None)
					return false;
			}

			return true;
		}

		/*void ComputeLighting() {
			GlobalPlacedBlock[] PlacedBlocks = GetAllExistingBlocks().ToArray();

			for (int i = 0; i < PlacedBlocks.Length; i++) {
				Chunk.PlacedBlock Block = PlacedBlocks[i].Block;
				if (Block.Type == BlockType.None)
					continue;

				Vector3 GlobalPos = PlacedBlocks[i].GlobalPos;
				int X = (int)GlobalPos.X;
				int Y = (int)GlobalPos.Y;
				int Z = (int)GlobalPos.Z;

				if (IsCovered(X, Y, Z))
					continue;

				// Ambient occlusion
				for (int j = 0; j < Utils.MainDirs.Length; j++) {
					Vector3 Origin = new Vector3(X, Y, Z) + Utils.MainDirs[j];
					float AmbientHitRatio = ((float)CountAmbientHits(Origin) - 1) / 5;


					int Light = 32 - (int)(AmbientHitRatio * 24);

					Block.Lights[Utils.DirToByte(Utils.MainDirs[j])] = new BlockLight((byte)(Light));
					PlacedBlocks[i].Chunk.MarkDirty();
				}

				// TODO: Actual lights

				// Set block back into world
				// SetPlacedBlock(X, Y, Z, Block, false);
			}
		}*/

		public PlacedBlock GetPlacedBlock(int X, int Y, int Z, out Chunk Chk) {
			TranslateChunkPos(X, Y, Z, out Vector3i ChunkPosition, out Vector3 BlockPos);

			if (ContainsChunk(ChunkPosition)) {
				Chk = GetChunk(ChunkPosition);
				return Chk.GetBlock((int)BlockPos.X, (int)BlockPos.Y, (int)BlockPos.Z);
			}

			Chk = null;
			return new PlacedBlock(BlockType.None);
		}

		public BlockType GetBlock(int X, int Y, int Z) {
			return GetPlacedBlock(X, Y, Z, out Chunk Chk).Type;
		}

		public BlockType GetBlock(Vector3 Pos) {
			return GetBlock((int)Pos.X, (int)Pos.Y, (int)Pos.Z);
		}

		public void Draw() {
			foreach (Chunk C in Chunks) {
				Vector3 ChunkPos = C.Position * new Vector3(Chunk.ChunkSize);
				C.Draw(ChunkPos);
			}
		}

		public void DrawTransparent() {
			foreach (Chunk C in Chunks) {
				Vector3 ChunkPos = C.Position * new Vector3(Chunk.ChunkSize);
				C.DrawTransparent(ChunkPos);
			}
		}
	}
}
