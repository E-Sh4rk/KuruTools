using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace KuruRomExtractor
{
    [StructLayout(LayoutKind.Explicit, Size = 0x4C)]
    struct ParadiseLevelEntry
    {
        public const int BASE_ADDRESS = 0x2C884;
        const int ROM_MEMORY_DOMAIN = 0x08000000;

        [FieldOffset(0)]
        public int addr00; // Not compressed data
        [FieldOffset(4)]
        public int addr01; // Not compressed data
        [FieldOffset(8)]
        public int addr02; // Sometimes zero
        [FieldOffset(12)]
        public int addr03; // First 2 bytes do not seem to indicate the size... (too big)
        [FieldOffset(16)]
        public int level_data_offset;
        [FieldOffset(20)]
        public int addr05; // Sometimes zero
        [FieldOffset(24)]
        public int addr06; // Might be compressed... Size of 512?
        [FieldOffset(28)]
        public int addr07; // Sometimes zero
        [FieldOffset(32)]
        public int addr08; // Not compressed data
        [FieldOffset(36)]
        public int addr09; // Sometimes zero
        [FieldOffset(40)]
        public int addr10; // Not compressed data
        [FieldOffset(44)]
        public int addr11; // Not compressed data
        [FieldOffset(48)]
        public int addr12; // Seems to be an offset (not an absolute address)
        [FieldOffset(52)]
        public int addr13; // Not compressed data
        [FieldOffset(56)]
        public int addr14; // Sometimes zero
        [FieldOffset(60)]
        public int object_data_offset;
        [FieldOffset(64)]
        public int addr16; // Not compressed data
        [FieldOffset(68)]
        public int addr17; // Sometimes zero
        [FieldOffset(72)]
        public int addr18; // Often zero

        public int LevelDataOffset
        {
            get { return level_data_offset - ROM_MEMORY_DOMAIN; }
        }
        public int ObjectDataOffset
        {
            get { return object_data_offset - ROM_MEMORY_DOMAIN; }
        }
    }
    public class ParadiseLevels
    {
        public const int NUMBER_LEVELS = 75;
        public static int[] AllLevels()
        {
            List<int> res = new List<int>();
            for (int i = 0; i < NUMBER_LEVELS; i++)
                res.Add(i);
            return res.ToArray();
        }

        FileStream rom;
        ParadiseLevelEntry[] level_entries;

        public struct LevelInfo
        {
            public int DataBaseAddress;
            public int DataUncompressedSize;
            public int ObjectsBaseAddress;
            public int ObjectsSize;
            // TODO: Data below in progress...
            public int GraphicalBaseAddress;
            public int GraphicalUncompressedSize;
            public int BackgroundBaseAddress;
            public int BackgroundUncompressedSize;
            //public int MinimapBaseAddress;
            //public int MinimapSize;
        }
        public struct RawMapData
        {
            public byte[] CompressedData;
            public byte[] RawData;
            public byte[] RawObjects;
            // TODO: Data below in progress...
            public byte[] CompressedGraphical;
            public byte[] RawGraphical;
            public byte[] CompressedBackground;
            public byte[] RawBackground;
            //public byte[] RawMinimap;
        }

        public ParadiseLevels(string romPath)
        {
            rom = File.Open(romPath, FileMode.Open, FileAccess.ReadWrite);
            LoadLevelInfos();
        }

        void LoadLevelInfos()
        {
            BinaryReader reader = new BinaryReader(rom);

            level_entries = new ParadiseLevelEntry[NUMBER_LEVELS];
            rom.Seek(ParadiseLevelEntry.BASE_ADDRESS, SeekOrigin.Begin);
            for (int l = 0; l < level_entries.Length; l++)
            {
                level_entries[l] = Utils.ByteToType<ParadiseLevelEntry>(reader);
            }
        }

        public LevelInfo GetLevelInfo(int level)
        {
            LevelInfo res;
            BinaryReader reader = new BinaryReader(rom);

            int base_addr = level_entries[level].LevelDataOffset;
            rom.Seek(base_addr, SeekOrigin.Begin);
            res.DataUncompressedSize = reader.ReadInt32();
            res.DataBaseAddress = (int)rom.Position;

            res.ObjectsBaseAddress = level_entries[level].ObjectDataOffset;
            res.ObjectsSize = 0;
            rom.Seek(res.ObjectsBaseAddress, SeekOrigin.Begin);
            byte[] endDelim = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            while (!reader.ReadBytes(endDelim.Length).SequenceEqual(endDelim))
                res.ObjectsSize += endDelim.Length;

            base_addr = level_entries[level].level_data_offset - 0x08000000; // TODO: TMP
            rom.Seek(base_addr, SeekOrigin.Begin);
            res.GraphicalUncompressedSize = reader.ReadInt32();
            res.GraphicalBaseAddress = (int)rom.Position;

            base_addr = level_entries[level].level_data_offset - 0x08000000; // TODO: TMP
            rom.Seek(base_addr, SeekOrigin.Begin);
            res.BackgroundUncompressedSize = reader.ReadInt32();
            res.BackgroundBaseAddress = (int)rom.Position;

            return res;
        }

        public RawMapData ExtractLevelData(int level)
        {
            RawMapData res;
            LevelInfo info = GetLevelInfo(level);
            BinaryReader reader = new BinaryReader(rom);

            rom.Seek(info.DataBaseAddress, SeekOrigin.Begin);
            long startPos = rom.Position;
            res.RawData = LzCompression.Decompress(rom, info.DataUncompressedSize);
            int length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedData = (new BinaryReader(rom)).ReadBytes(length);

            rom.Seek(info.ObjectsBaseAddress, SeekOrigin.Begin);
            res.RawObjects = reader.ReadBytes(info.ObjectsSize);

            rom.Seek(info.GraphicalBaseAddress, SeekOrigin.Begin);
            startPos = rom.Position;
            res.RawGraphical = LzCompression.Decompress(rom, info.GraphicalUncompressedSize);
            length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedGraphical = (new BinaryReader(rom)).ReadBytes(length);

            rom.Seek(info.BackgroundBaseAddress, SeekOrigin.Begin);
            startPos = rom.Position;
            res.RawBackground = LzCompression.Decompress(rom, info.BackgroundUncompressedSize);
            length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedBackground = (new BinaryReader(rom)).ReadBytes(length);

            return res;
        }

        public void Dispose()
        {
            rom.Close();
        }
    }
}
