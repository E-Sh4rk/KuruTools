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
        public int addr00;
        [FieldOffset(4)]
        public int addr01; // Sometimes zero
        [FieldOffset(8)]
        public int addr02; // Sometimes zero (in particular for first levels)
        [FieldOffset(12)]
        public int addr03;
        [FieldOffset(16)]
        public int level_data_offset;
        [FieldOffset(20)]
        public int graphical_data_offset; // Sometimes zero
        [FieldOffset(24)]
        public int background_data_offset;
        [FieldOffset(28)]
        public int addr07; // Sometimes zero (in particular for first levels)
        [FieldOffset(32)]
        public int addr08;
        [FieldOffset(36)]
        public int addr09; // Sometimes zero (in particular for first levels)
        [FieldOffset(40)]
        public int addr10;
        [FieldOffset(44)]
        public int minimap_offset;
        [FieldOffset(48)]
        public int flags;
        [FieldOffset(52)]
        public int addr13; // Lot of zeros...
        [FieldOffset(56)]
        public int addr14; // Sometimes zero (in particular for first levels)
        [FieldOffset(60)]
        public int object_data_offset;
        [FieldOffset(64)]
        public int addr16; // Sometimes zero
        [FieldOffset(68)]
        public int addr17; // Sometimes zero (in particular for first levels)
        [FieldOffset(72)]
        public int addr18; // Often zero (in particular for first levels)

        public int LevelDataOffset
        {
            get { return level_data_offset - ROM_MEMORY_DOMAIN; }
        }
        public int ObjectDataOffset
        {
            get { return object_data_offset - ROM_MEMORY_DOMAIN; }
        }
        public int GraphicalDataOffset
        {
            get { return graphical_data_offset != 0 ? graphical_data_offset - ROM_MEMORY_DOMAIN : 0; }
        }
        public int BackgroundDataOffset
        {
            get { return background_data_offset - ROM_MEMORY_DOMAIN; }
        }
        public int MinimapOffset
        {
            get { return minimap_offset - ROM_MEMORY_DOMAIN; }
        }
    }
    public class ParadiseLevels
    {
        public const int NUMBER_LEVELS = 75;
        public const int MINIMAP_SIZE = 64 * 64 / 2;
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
            public int GraphicalBaseAddress;
            public int GraphicalUncompressedSize;
            public int BackgroundBaseAddress;
            public int BackgroundUncompressedSize;
            public int MinimapBaseAddress;
            public int MinimapUncompressedSize;
            public int Flags;
        }
        public struct RawMapData
        {
            public byte[] CompressedData;
            public byte[] RawData;
            public byte[] RawObjects;
            public byte[] CompressedGraphical;
            public byte[] RawGraphical;
            public byte[] CompressedBackground;
            public byte[] RawBackground;
            public byte[] CompressedMinimap;
            public byte[] RawMinimap;
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
            // For debugging purpose
            /*for (int l = 0; l < level_entries.Length; l++)
            {
                ParadiseLevelEntry e = level_entries[l];
                int[] toTest = new int[] { e.addr00, e.addr01, e.addr02, e.addr03, e.addr07, e.addr08, e.addr09, e.addr10,
                    e.addr11, e.addr12, e.addr13, e.addr14, e.addr16, e.addr17, e.addr18 };
                int k = 0;
                foreach (int addr in toTest)
                {
                    if (addr >= 0x08000000)
                    {
                        try
                        {
                            rom.Seek(addr - 0x08000000, SeekOrigin.Begin);
                            Console.WriteLine(l.ToString() + "." + k.ToString() + ":" + reader.ReadInt16());
                        }
                        catch { }
                    }
                    k++;
                }
                Console.ReadLine();
            }*/
            /*for (int l = 0; l < level_entries.Length; l++)
            {
                ParadiseLevelEntry e = level_entries[l];
                Console.WriteLine(l.ToString("D2") + ": " + Convert.ToString(e.addr12, 2).PadLeft(24, '0'));
            }
            Console.ReadLine();*/
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

            base_addr = level_entries[level].GraphicalDataOffset;
            if (base_addr == 0)
            {
                res.GraphicalUncompressedSize = 0;
                res.GraphicalBaseAddress = 0;
            }
            else
            {
                rom.Seek(base_addr, SeekOrigin.Begin);
                res.GraphicalUncompressedSize = reader.ReadInt32();
                res.GraphicalBaseAddress = (int)rom.Position;
            }

            base_addr = level_entries[level].BackgroundDataOffset;
            rom.Seek(base_addr, SeekOrigin.Begin);
            res.BackgroundUncompressedSize = reader.ReadInt32();
            res.BackgroundBaseAddress = (int)rom.Position;

            res.MinimapBaseAddress = level_entries[level].MinimapOffset;
            res.MinimapUncompressedSize = MINIMAP_SIZE;

            res.Flags = level_entries[level].flags;

            return res;
        }

        const int BACKGROUND_CHALLENGE_MASK = 0x2000;
        const ushort BACKGROUND_CHALLENGE_WIDTH = 0x10;
        const ushort BACKGROUND_CHALLENGE_HEIGHT = 0x10;

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
            res.CompressedData = reader.ReadBytes(length);

            rom.Seek(info.ObjectsBaseAddress, SeekOrigin.Begin);
            res.RawObjects = reader.ReadBytes(info.ObjectsSize);

            rom.Seek(info.GraphicalBaseAddress, SeekOrigin.Begin);
            startPos = rom.Position;
            res.RawGraphical = LzCompression.Decompress(rom, info.GraphicalUncompressedSize);
            length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedGraphical = reader.ReadBytes(length);

            rom.Seek(info.BackgroundBaseAddress, SeekOrigin.Begin);
            startPos = rom.Position;
            res.RawBackground = LzCompression.Decompress(rom, info.BackgroundUncompressedSize);
            length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedBackground = reader.ReadBytes(length);
            if ((info.Flags & BACKGROUND_CHALLENGE_MASK) != 0)
            {
                byte[] newRaw = new byte[res.RawBackground.Length + 4];
                BinaryWriter bw = new BinaryWriter(new MemoryStream(newRaw));
                bw.Write(BACKGROUND_CHALLENGE_WIDTH);
                bw.Write(BACKGROUND_CHALLENGE_HEIGHT);
                bw.Write(res.RawBackground);
                bw.Close();
                res.RawBackground = newRaw;
            }

            rom.Seek(info.MinimapBaseAddress, SeekOrigin.Begin);
            startPos = rom.Position;
            res.RawMinimap = LzCompression.Decompress(rom, info.MinimapUncompressedSize);
            length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedMinimap = reader.ReadBytes(length);

            return res;
        }

        public void Dispose()
        {
            rom.Close();
        }
    }
}
