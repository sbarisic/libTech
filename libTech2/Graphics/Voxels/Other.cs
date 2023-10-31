using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using FishGfx;

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
}
