﻿using System;
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
        public const int ROM_MEMORY_DOMAIN = 0x08000000;

        [FieldOffset(0)]
        public int addr00; // Tileset 1
        [FieldOffset(4)]
        public int addr01; // Tileset 2
        [FieldOffset(8)]
        public int addr02; // Tileset 3
        [FieldOffset(12)]
        public int addr03; // Tileset 4
        [FieldOffset(16)]
        public int level_data_offset;
        [FieldOffset(20)]
        public int graphical_data_offset; // Sometimes zero
        [FieldOffset(24)]
        public int graphical2_data_offset;
        [FieldOffset(28)]
        public int background_data_offset; // Sometimes zero (in particular for first levels)
        [FieldOffset(32)]
        public int addr08; // Palette for tiles
        [FieldOffset(36)]
        public int addr09; // Palette for practice mode? Sometimes zero (in particular for first levels)
        [FieldOffset(40)]
        public int addr10; // Other versions of the first color set (32 first bytes)
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
        public int addr18; // Often zero, except 28, 36, 37

        public int LevelDataOffset
        {
            get { return level_data_offset - ROM_MEMORY_DOMAIN; }
            set { level_data_offset = value + ROM_MEMORY_DOMAIN; }
        }
        public int ObjectDataOffset
        {
            get { return object_data_offset - ROM_MEMORY_DOMAIN; }
            set { object_data_offset = value + ROM_MEMORY_DOMAIN; }
        }
        public int GraphicalDataOffset
        {
            get { return graphical_data_offset > 0 ? graphical_data_offset - ROM_MEMORY_DOMAIN : 0; }
            set { graphical_data_offset = value <= 0 ? 0 : value + ROM_MEMORY_DOMAIN; }
        }
        public int BackgroundDataOffset
        {
            // background_data_offset can be -1 on some levels...
            get { return background_data_offset > 0 ? background_data_offset - ROM_MEMORY_DOMAIN : 0; }
            set { background_data_offset = value <= 0 ? 0 : value + ROM_MEMORY_DOMAIN; }
        }
        public int Graphical2DataOffset
        {
            get { return graphical2_data_offset - ROM_MEMORY_DOMAIN; }
            set { graphical2_data_offset = value + ROM_MEMORY_DOMAIN; }
        }
        public int MinimapOffset
        {
            get { return minimap_offset - ROM_MEMORY_DOMAIN; }
            set { minimap_offset = value + ROM_MEMORY_DOMAIN; }
        }
    }

    struct ParadiseOverworldMarkerEntry
    {
        public byte connection_east;
        public byte connection_west;
        public byte connection_south;
        public byte connection_north;
        public byte progression_normal;
        public byte progression_secret;
        public ushort coord_x;
        public ushort coord_y;
        public byte door_key_association;
    }

    public class ParadiseLevels
    {
        public const int NUMBER_LEVELS = 75;
        public const int FIRST_CHALLENGE = 42;
        public const int MINIMAP_SIZE = 64 * 64 / 2;
        public static int[] AllLevels()
        {
            List<int> res = new List<int>();
            for (int i = 0; i < NUMBER_LEVELS; i++)
                res.Add(i);
            return res.ToArray();
        }

        public const int OVERWORLD_BG_WIDTH = 1680;
        const int OVERWORLD_BG_HEIGHT = 200;
        const int OVERWORLD_BG_TILES = 0x149D24;
        const int OVERWORLD_BG_PALETTE = 0x19BDA4;
        const int OVERWORLD_FG_PALETTE = 0x19C1A4;

        const int COMMON_PALETTE_11 = 0x1C5BF0;
        const int COMMON_PALETTE_12 = 0x1C5CF0;
        const int COMMON_PALETTE_13 = 0x1C5DF0;
        const int COMMON_PALETTE_14 = 0x19BF64;
        const int COMMON_PALETTE_15 = 0x19BF84;
        public const int COLORSET_SIZE = 0x20;

        const int NUMBER_AREAS_TABLE_OFFSET = 0x2E364;
        const int NUMBER_AREAS_TABLE_SIZE = 42;
        const int TIMES_TABLE_OFFSET = 0x28DB8;
        public const int TIMES_TABLE_HEIGHT = NUMBER_LEVELS + 42 /* Easy Mode Times */;
        public const int TIMES_TABLE_WIDTH = 3;

        public const int OVERWORLD_MARKER_COUNT = 54;
        public const int OVERWORLD_MARKER_PROPERTY_COUNT = 9;
        const int OVERWORLD_MARKER_CONNECTIONS_OFFSET = 0x2E148;
        const int OVERWORLD_MARKER_PROGRESSION_OFFSET = 0x2E220;
        const int OVERWORLD_MARKER_COORDS_OFFSET = 0x2E28C;
        const int OVERWORLD_MARKER_DOOR_KEY_ASSOCIATIONS_OFFSET = 0x2E38E;

        public class OverworldObject
        {
            public static readonly OverworldObject CONNECTOR_DOT = new OverworldObject("connectorDot", 0x29C6D4, 1, 8, 8);
            public static readonly OverworldObject BLUE_DOT = new OverworldObject("blueDot", 0x29C6FC, 1, 16, 16);
            public static readonly OverworldObject ORANGE_DOT = new OverworldObject("orangeDot", 0x29C784, 1, 16, 16);
            public static readonly OverworldObject STAR = new OverworldObject("star", 0x29C80C, 1, 16, 16);
            public static readonly OverworldObject MAGIC_HAT_UNBEATEN = new OverworldObject("magicHatUnbeaten", 0x29C894, 3, 32, 32);
            public static readonly OverworldObject MAGIC_HAT_BEATEN = new OverworldObject("magicHatBeaten", 0x29CA9C, 3, 32, 32);

            public static IEnumerable<OverworldObject> Values
            {
                get
                {
                    yield return CONNECTOR_DOT;
                    yield return BLUE_DOT;
                    yield return ORANGE_DOT;
                    yield return STAR;
                    yield return MAGIC_HAT_UNBEATEN;
                    yield return MAGIC_HAT_BEATEN;
                }
            }
            OverworldObject(string name, int offset, int paletteIndex, int width, int height) =>
                (Name, Offset, PaletteIndex, Width, Height) = (name, offset, paletteIndex, width, height);

            public string Name { get; private set; }
            public int Offset { get; private set; }
            public int PaletteIndex { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
        }

        FileStream rom;
        ParadiseLevelEntry[] level_entries;
        ParadiseOverworldMarkerEntry[] overworld_marker_entries;

        public struct LevelInfo
        {
            public int DataBaseAddress;
            public int DataUncompressedSize;
            public int ObjectsBaseAddress;
            public int ObjectsSize;
            public int GraphicalBaseAddress;
            public int GraphicalUncompressedSize;
            public int Graphical2BaseAddress;
            public int Graphical2UncompressedSize;
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
            public byte[] CompressedGraphical2;
            public byte[] RawGraphical2;
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
                int[] toTest = new int[] { e.addr13, e.addr14, e.addr16, e.addr17 };
                int k = 0;
                foreach (int addr in toTest)
                {
                    if (addr >= 0x08000000)
                    {
                        try
                        {
                            rom.Seek(addr - 0x08000000, SeekOrigin.Begin);
                            Console.WriteLine(l.ToString() + "." + k.ToString() + ":" + "\t" + (addr - 0x08000000).ToString("X6") + "\t" + reader.ReadInt32());
                        }
                        catch { }
                    }
                    k++;
                }
                Console.ReadLine();
            }*/
            /*List<int> addrs = new List<int>();
            Dictionary<int, string> infos = new Dictionary<int, string>();
            for (int l = 0; l < level_entries.Length; l++)
            {
                ParadiseLevelEntry e = level_entries[l];
                int[] toTest = new int[] { e.addr00, e.addr01, e.addr02, e.addr03, e.level_data_offset, e.graphical_data_offset, e.graphical2_data_offset, e.background_data_offset,
                    e.addr08, e.addr09, e.addr10, e.minimap_offset, e.flags, e.addr13, e.addr14, e.object_data_offset, e.addr16, e.addr17, e.addr18 };
                int k = 0;
                foreach (int addr in toTest)
                {
                    if (addr >= 0x08000000)
                    {
                        addrs.Add(addr - 0x08000000);
                        infos[addr - 0x08000000] = k.ToString();
                    }
                    k++;
                }
            }
            addrs.Sort();
            foreach (int addr in addrs.Distinct())
            {
                Console.WriteLine(addr.ToString("X6") + "\t" + infos[addr]);
            }
            Console.ReadLine();*/
            /*for (int l = 0; l < level_entries.Length; l++)
            {
                ParadiseLevelEntry e = level_entries[l];
                Console.WriteLine(l.ToString("D2") + ": " + Convert.ToString(e.flags, 2).PadLeft(24, '0'));
            }
            Console.ReadLine();*/
        }

        static byte[] objectsEndDelimiter = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
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
            while (!reader.ReadBytes(objectsEndDelimiter.Length).SequenceEqual(objectsEndDelimiter))
                res.ObjectsSize += objectsEndDelimiter.Length;

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

            base_addr = level_entries[level].Graphical2DataOffset;
            rom.Seek(base_addr, SeekOrigin.Begin);
            res.Graphical2UncompressedSize = reader.ReadInt32();
            res.Graphical2BaseAddress = (int)rom.Position;

            base_addr = level_entries[level].BackgroundDataOffset;
            if (base_addr == 0)
            {
                res.BackgroundUncompressedSize = 0;
                res.BackgroundBaseAddress = 0;
            }
            else
            {
                rom.Seek(base_addr, SeekOrigin.Begin);
                res.BackgroundUncompressedSize = reader.ReadInt32();
                res.BackgroundBaseAddress = (int)rom.Position;
            }

            res.MinimapBaseAddress = level_entries[level].MinimapOffset;
            res.MinimapUncompressedSize = MINIMAP_SIZE;

            res.Flags = level_entries[level].flags;

            return res;
        }

        const ushort GR2_CHALLENGE_WIDTH = 0x10;
        const ushort GR2_CHALLENGE_HEIGHT = 0x10;

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

            rom.Seek(info.Graphical2BaseAddress, SeekOrigin.Begin);
            startPos = rom.Position;
            res.RawGraphical2 = LzCompression.Decompress(rom, info.Graphical2UncompressedSize);
            length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedGraphical2 = reader.ReadBytes(length);
            if (level >= FIRST_CHALLENGE)
            {
                byte[] newRaw = new byte[res.RawGraphical2.Length + 4];
                BinaryWriter bw = new BinaryWriter(new MemoryStream(newRaw));
                bw.Write(GR2_CHALLENGE_WIDTH);
                bw.Write(GR2_CHALLENGE_HEIGHT);
                bw.Write(res.RawGraphical2);
                bw.Close();
                res.RawGraphical2 = newRaw;
            }

            rom.Seek(info.BackgroundBaseAddress, SeekOrigin.Begin);
            startPos = rom.Position;
            res.RawBackground = LzCompression.Decompress(rom, info.BackgroundUncompressedSize);
            length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedBackground = reader.ReadBytes(length);

            rom.Seek(info.MinimapBaseAddress, SeekOrigin.Begin);
            startPos = rom.Position;
            res.RawMinimap = LzCompression.Decompress(rom, info.MinimapUncompressedSize);
            length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedMinimap = reader.ReadBytes(length);

            return res;
        }

        int floorToMultiple(int v, int multiple)
        {
            return v - (v % multiple);
        }
        int ceilToMultiple(int v, int multiple)
        {
            if (v % multiple == 0) return v;
            return floorToMultiple(v, multiple) + multiple;
        }

        int WriteDataWithCompression(int level, byte[] original_raw, byte[] original_compressed, byte[] new_raw, int baseAddr, int endAddr)
        {
            rom.Seek(baseAddr, SeekOrigin.Begin);
            if (new_raw == null && (endAddr < 0 || baseAddr + original_compressed.Length <= endAddr))
            {
                rom.Write(original_compressed);
                return original_raw.Length;
            }
            else if (new_raw == null)
                new_raw = original_raw;
            int uncompressed_length_written = LzCompression.Compress(rom, new_raw, endAddr);
            if (uncompressed_length_written < new_raw.Length)
                Console.WriteLine(string.Format("Warning: The new level {0} has been truncated.", level.ToString()));
            Debug.Assert(uncompressed_length_written <= new_raw.Length);
            return floorToMultiple(uncompressed_length_written, 4);
            //return new_raw.Length;
        }
        void WriteSizeAndDataWithCompression(int level, byte[] original_raw, byte[] original_compressed, byte[] new_raw, int baseAddr, int endAddr)
        {
            int size = WriteDataWithCompression(level, original_raw, original_compressed,new_raw, baseAddr + sizeof(int), endAddr);
            long position_bkp = rom.Position;
            rom.Seek(baseAddr, SeekOrigin.Begin);
            (new BinaryWriter(rom)).Write(size);
            rom.Seek(position_bkp, SeekOrigin.Begin);
        }
        byte[] StripFirstBytes(byte[] data, int nb)
        {
            nb = Math.Min(nb, data.Length);
            byte[] res = new byte[data.Length - nb];
            Array.Copy(data, nb, res, 0, res.Length);
            return res;
        }
        public bool AlterLevelData(int level, byte[] new_data, byte[] new_objects, byte[] new_graphical, byte[] new_graphical2, byte[] new_background, byte[] new_minimap)
        {
            RawMapData original = ExtractLevelData(level);
            if (new_data != null && original.RawData.SequenceEqual(new_data))
                new_data = null;
            if (new_objects != null && original.RawObjects.SequenceEqual(new_objects))
                new_objects = null;
            if (new_graphical != null && original.RawGraphical.SequenceEqual(new_graphical))
                new_graphical = null;
            if (new_graphical2 != null && original.RawGraphical2.SequenceEqual(new_graphical2))
                new_graphical2 = null;
            if (new_background != null && original.RawBackground.SequenceEqual(new_background))
                new_background = null;
            if (new_minimap != null && original.RawMinimap.SequenceEqual(new_minimap))
                new_minimap = null;

            if (new_data == null && new_objects == null && new_graphical == null && new_graphical2 == null  && new_background == null && new_minimap == null)
                return false;

            // Write compressed data
            int pos = ceilToMultiple((int)rom.Length, 4);
            level_entries[level].LevelDataOffset = pos;
            WriteSizeAndDataWithCompression(level, original.RawData, original.CompressedData, new_data, pos, -1);

            if ((new_graphical == null && original.RawGraphical.Length == 0) ||
                (new_graphical != null && new_graphical.Length == 0))
            {
                level_entries[level].GraphicalDataOffset = 0;
            }
            else
            {
                pos = ceilToMultiple((int)rom.Position, 4);
                level_entries[level].GraphicalDataOffset = pos;
                WriteSizeAndDataWithCompression(level, original.RawGraphical, original.CompressedGraphical, new_graphical, pos, -1);
            }

            pos = ceilToMultiple((int)rom.Position, 4);
            if (level >= FIRST_CHALLENGE)
            {
                original.RawGraphical2 = StripFirstBytes(original.RawGraphical2, 4);
                if (new_graphical2 != null)
                    new_graphical2 = StripFirstBytes(new_graphical2, 4);
            }
            level_entries[level].Graphical2DataOffset = pos;
            WriteSizeAndDataWithCompression(level, original.RawGraphical2, original.CompressedGraphical2, new_graphical2, pos, -1);

            if ((new_background == null && original.RawBackground.Length == 0) ||
                (new_background != null && new_background.Length == 0))
            {
                level_entries[level].BackgroundDataOffset = 0;
            }
            else
            {
                pos = ceilToMultiple((int)rom.Position, 4);
                level_entries[level].BackgroundDataOffset = pos;
                WriteSizeAndDataWithCompression(level, original.RawBackground, original.CompressedBackground, new_background, pos, -1);
            }

            pos = ceilToMultiple((int)rom.Position, 4);
            level_entries[level].MinimapOffset = pos;
            WriteDataWithCompression(level, original.RawMinimap, original.CompressedMinimap, new_minimap, pos, -1);

            pos = ceilToMultiple((int)rom.Position, 4);
            level_entries[level].ObjectDataOffset = pos;
            byte[] objects = new_objects == null ? original.RawObjects : new_objects;
            rom.Seek(pos, SeekOrigin.Begin);
            rom.Write(objects, 0, objects.Length);
            rom.Write(objectsEndDelimiter, 0, objectsEndDelimiter.Length);

            // Update LevelEntry structure
            rom.Seek(ParadiseLevelEntry.BASE_ADDRESS, SeekOrigin.Begin);
            BinaryWriter writer = new BinaryWriter(rom);
            foreach (ParadiseLevelEntry entry in level_entries)
                Utils.TypeToByte(writer, entry);

            return true;
        }

        const int PHYSICAL_TILES_FLAGS_POS = 8;
        const int GRAPHICAL_TILES_FLAGS_POS = 10;
        const int GRAPHICAL2_TILES_FLAGS_POS = 12;
        const int BACKGROUND_TILES_FLAGS_POS = 14;

        const int SPECIAL_MODE_8BPP_FLAGS = 0b001011;
        const int SPECIAL_MODE_8BPP_MASK = 0b111111;
        const int SPECIAL_MODE_8BPP_FLAGS_POS = PHYSICAL_TILES_FLAGS_POS;

        public bool AreGraphical2Tiles8bpp(int level)
        {
            LevelInfo info = GetLevelInfo(level);
            return ((info.Flags >> SPECIAL_MODE_8BPP_FLAGS_POS) & SPECIAL_MODE_8BPP_MASK) == SPECIAL_MODE_8BPP_FLAGS;
        }

        int GetTilesetNbFromFlag(int flag, int pos)
        {
            return (flag >> pos) & 0x3;
        }
        byte[] DecompressWorldData(int addr, int uncompressed_size)
        {
            if (addr == 0)
                return null;
            rom.Seek(addr - ParadiseLevelEntry.ROM_MEMORY_DOMAIN, SeekOrigin.Begin);
            return LzCompression.Decompress(rom, uncompressed_size);
        }
        byte[] ReadWorldData(int addr, int size)
        {
            if (addr == 0)
                return null;
            rom.Seek(addr - ParadiseLevelEntry.ROM_MEMORY_DOMAIN, SeekOrigin.Begin);
            return (new BinaryReader(rom)).ReadBytes(size);
        }

        public byte[][] ExtractTilesData(int level)
        {
            // Extract tilesets
            ParadiseLevelEntry ple = level_entries[level];
            byte[] ts0 = DecompressWorldData(ple.addr00, 0x4000);
            byte[] ts1 = DecompressWorldData(ple.addr01, 0x4000);
            byte[] ts2 = DecompressWorldData(ple.addr02, 0x4000);
            byte[] ts3 = DecompressWorldData(ple.addr03, 0x2000);

            byte[] ext0 = new byte[0x8000];
            if (ts0 != null)
                Array.Copy(ts0, ext0, 0x4000);
            if (ts1 != null)
                Array.Copy(ts1, 0, ext0, 0x4000, 0x4000);

            byte[] ext1 = new byte[0x8000];
            if (ts1 != null)
                Array.Copy(ts1, ext1, 0x4000);
            if (ts2 != null)
                Array.Copy(ts2, 0, ext1, 0x4000, 0x4000);

            byte[] ext2 = new byte[0x6000];
            if (ts2 != null)
                Array.Copy(ts2, ext2, 0x4000);
            if (ts3 != null)
                Array.Copy(ts3, 0, ext2, 0x4000, 0x2000);

            byte[][] tilesets = new byte[][] { ext0, ext1, ext2, ts3 };

            // Permutations according to the flags...
            LevelInfo info = GetLevelInfo(level);
            int physical = GetTilesetNbFromFlag(info.Flags, PHYSICAL_TILES_FLAGS_POS);
            int graphical = GetTilesetNbFromFlag(info.Flags, GRAPHICAL_TILES_FLAGS_POS);
            int graphical2 = GetTilesetNbFromFlag(info.Flags, GRAPHICAL2_TILES_FLAGS_POS);
            int background = GetTilesetNbFromFlag(info.Flags, BACKGROUND_TILES_FLAGS_POS);

            byte[][] res = new byte[6][];
            res[0] = tilesets[graphical];
            res[1] = tilesets[graphical2];
            res[2] = tilesets[background];
            res[3] = tilesets[physical];
            res[4] = DecompressWorldData(ple.addr08, 512);
            res[5] = ReadWorldData(ple.addr10, COLORSET_SIZE);

            return res;
        }

        public byte[] ExtractGraphicalData(int offset, int size)
        {
            byte[] res = new byte[size];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(res));
            BinaryReader reader = new BinaryReader(rom);
            rom.Seek(offset, SeekOrigin.Begin);
            writer.Write(reader.ReadBytes(res.Length));
            return res;
        }

        public byte[] ExtractOverworldBGTiles()
        {
            byte[] res = new byte[OVERWORLD_BG_HEIGHT * OVERWORLD_BG_WIDTH];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(res));
            BinaryReader reader = new BinaryReader(rom);
            rom.Seek(OVERWORLD_BG_TILES, SeekOrigin.Begin);
            writer.Write(reader.ReadBytes(res.Length));
            return res;
        }

        public byte[] ExtractCommonPaletteData()
        {
            byte[] res = new byte[5 * COLORSET_SIZE];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(res));
            BinaryReader reader = new BinaryReader(rom);
            rom.Seek(COMMON_PALETTE_11, SeekOrigin.Begin);
            writer.Write(reader.ReadBytes(COLORSET_SIZE));
            rom.Seek(COMMON_PALETTE_12, SeekOrigin.Begin);
            writer.Write(reader.ReadBytes(COLORSET_SIZE));
            rom.Seek(COMMON_PALETTE_13, SeekOrigin.Begin);
            writer.Write(reader.ReadBytes(COLORSET_SIZE));
            rom.Seek(COMMON_PALETTE_14, SeekOrigin.Begin);
            writer.Write(reader.ReadBytes(COLORSET_SIZE));
            rom.Seek(COMMON_PALETTE_15, SeekOrigin.Begin);
            writer.Write(reader.ReadBytes(COLORSET_SIZE));
            writer.Close();
            return res;
        }
        
        public byte[] ExtractOverworldPaletteData(bool background)
        {
            byte[] res = new byte[16 * COLORSET_SIZE];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(res));
            BinaryReader reader = new BinaryReader(rom);
            rom.Seek(background ? OVERWORLD_BG_PALETTE : OVERWORLD_FG_PALETTE, SeekOrigin.Begin);
            writer.Write(reader.ReadBytes(res.Length));
            writer.Close();
            return res;
        }

        public byte[] GetNumberAreasTable()
        {
            byte[] res = new byte[NUMBER_AREAS_TABLE_SIZE];
            rom.Seek(NUMBER_AREAS_TABLE_OFFSET, SeekOrigin.Begin);
            rom.Read(res, 0, res.Length);
            return res;
        }

        public void SetNumberAreasTable(byte[] table)
        {
            rom.Seek(NUMBER_AREAS_TABLE_OFFSET, SeekOrigin.Begin);
            rom.Write(table, 0, table.Length);
        }

        public ushort[,] GetTimesTable()
        {
            ushort[,] res = new ushort[TIMES_TABLE_HEIGHT, TIMES_TABLE_WIDTH];
            BinaryReader reader = new BinaryReader(rom);
            rom.Seek(TIMES_TABLE_OFFSET, SeekOrigin.Begin);
            for (int j = 0; j < TIMES_TABLE_HEIGHT; j++)
            {
                for (int i = 0; i < TIMES_TABLE_WIDTH; i++)
                {
                    res[j, i] = reader.ReadUInt16();
                }
            }
            return res;
        }
        public void SetTimesTable(ushort[,] table)
        {
            BinaryWriter writer = new BinaryWriter(rom);
            rom.Seek(TIMES_TABLE_OFFSET, SeekOrigin.Begin);
            for (int j = 0; j < TIMES_TABLE_HEIGHT; j++)
            {
                for (int i = 0; i < TIMES_TABLE_WIDTH; i++)
                {
                    writer.Write(table[j, i]);
                }
            }
        }
        public ushort[,] GetOverworldMarkers()
        {
            ushort[,] res = new ushort[OVERWORLD_MARKER_COUNT, OVERWORLD_MARKER_PROPERTY_COUNT];
            BinaryReader reader = new BinaryReader(rom);
            rom.Seek(OVERWORLD_MARKER_CONNECTIONS_OFFSET, SeekOrigin.Begin);
            for (int i = 0; i < OVERWORLD_MARKER_COUNT; i++)
            {
                res[i, 0] = reader.ReadByte();
                res[i, 1] = reader.ReadByte();
                res[i, 2] = reader.ReadByte();
                res[i, 3] = reader.ReadByte();
            }
            rom.Seek(OVERWORLD_MARKER_PROGRESSION_OFFSET, SeekOrigin.Begin);
            for (int i = 0; i < OVERWORLD_MARKER_COUNT; i++)
            {
                res[i, 4] = reader.ReadByte();
                res[i, 5] = reader.ReadByte();
            }
            rom.Seek(OVERWORLD_MARKER_COORDS_OFFSET, SeekOrigin.Begin);
            for (int i = 0; i < OVERWORLD_MARKER_COUNT; i++)
            {
                res[i, 6] = reader.ReadUInt16();
                res[i, 7] = reader.ReadUInt16();
            }
            rom.Seek(OVERWORLD_MARKER_DOOR_KEY_ASSOCIATIONS_OFFSET, SeekOrigin.Begin);
            for (int i = 0; i < OVERWORLD_MARKER_COUNT; i++)
            {
                res[i, 8] = reader.ReadByte();
            }
            return res;
        }
        public void SetOverworldMarkers(ushort[,] table)
        {
            BinaryWriter writer = new BinaryWriter(rom);
            rom.Seek(OVERWORLD_MARKER_CONNECTIONS_OFFSET, SeekOrigin.Begin);
            for (int i = 0; i < OVERWORLD_MARKER_COUNT; i++)
            {
                writer.Write((byte) table[i, 0]);
                writer.Write((byte) table[i, 1]);
                writer.Write((byte) table[i, 2]);
                writer.Write((byte) table[i, 3]);
            }
            rom.Seek(OVERWORLD_MARKER_PROGRESSION_OFFSET, SeekOrigin.Begin);
            for (int i = 0; i < OVERWORLD_MARKER_COUNT; i++)
            {
                writer.Write((byte) table[i, 4]);
                writer.Write((byte) table[i, 5]);
            }
            rom.Seek(OVERWORLD_MARKER_COORDS_OFFSET, SeekOrigin.Begin);
            for (int i = 0; i < OVERWORLD_MARKER_COUNT; i++)
            {
                writer.Write(table[i, 6]);
                writer.Write(table[i, 7]);
            }
            rom.Seek(OVERWORLD_MARKER_DOOR_KEY_ASSOCIATIONS_OFFSET, SeekOrigin.Begin);
            for (int i = 0; i < OVERWORLD_MARKER_COUNT; i++)
            {
                writer.Write((byte) table[i, 8]);
            }
        }

        public void Dispose()
        {
            rom.Close();
        }
    }
}
