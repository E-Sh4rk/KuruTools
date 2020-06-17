using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;

namespace KuruTools
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    struct WorldEntry
    {
        public const long BASE_ADDRESS = 0xC2E68;

        [FieldOffset(0)]
        int world_info_mem_address;
        [FieldOffset(4)]
        int world_data_mem_address;
        [FieldOffset(8)]
        int dummy1; // Seems to be linked to bonuses

        public int WorldInfoBaseAddress
        {
            get { return world_info_mem_address - 0x8000000; }
        }
        public int LevelInfosBaseAddress
        {
            get { return WorldInfoBaseAddress + 0x80; }
        }
        public int WorldDataBaseAddress
        {
            get { return world_data_mem_address - 0x8000000; }
        }
    }
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    struct LevelEntry
    {
        [FieldOffset(0)]
        public int level_data_offset;
        [FieldOffset(4)]
        public int level_uncompressed_size;
        [FieldOffset(8)]
        public int graphical_data_offset;
        [FieldOffset(12)]
        public int graphical_uncompressed_size;
        [FieldOffset(16)]
        public int background_data_offset;
        [FieldOffset(20)]
        public int background_uncompressed_size;
        [FieldOffset(24)]
        public int dummy1; // Offset of another compressed section
        [FieldOffset(28)]
        public int dummy2; // Uncompressed size of this section
    }
    public class Levels
    {
        public enum World
        {
            TRAINING = 0,
            GRASS,
            OCEAN,
            JUNGLE,
            CAKE,
            CAVE,
            CLOUD,
            STAR,
            ICE,
            MACHINE,
            GHOST,
            LAST,
            CHALLENGE,
        };
        static readonly int[] NUMBER_LEVELS = { 5, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 55 };
        public struct LevelInfo
        {
            public int DataBaseAddress;
            public int DataUncompressedSize;
            public int GraphicalBaseAddress;
            public int GraphicalUncompressedSize;
            public int BackgroundBaseAddress;
            public int BackgroundUncompressedSize;
            public int NextSectionBaseAddress;
        }
        public struct RawMapData
        {
            public byte[] CompressedData;
            public byte[] RawData;
            public byte[] CompressedGraphical;
            public byte[] RawGraphical;
            public byte[] CompressedBackground;
            public byte[] RawBackground;
        }
        public struct LevelIdentifier
        {
            public LevelIdentifier(World w, int l)
            {
                world = w;
                level = l;
            }
            public World world;
            public int level;
            public string ShortName()
            {
                return string.Format("{0}_{1}", Enum.GetName(typeof(World), world).ToLowerInvariant(), level+1);
            }
            public string ToString()
            {
                return string.Format("{0} {1}", Enum.GetName(typeof(World), world), level + 1);
            }
        }

        public static int NumberOfLevels(World world)
        {
            return NUMBER_LEVELS[(int)world];
        }

        public static LevelIdentifier[] AllLevels()
        {
            List<LevelIdentifier> res = new List<LevelIdentifier>();
            foreach (World w in Enum.GetValues(typeof(World)))
            {
                for (int l = 0; l < NumberOfLevels(w); l++)
                    res.Add(new LevelIdentifier(w, l));
            }
            return res.ToArray();
        }

        WorldEntry[] world_entries;
        LevelEntry[][] level_entries;
        FileStream rom;

        public Levels(string romPath)
        {
            rom = File.Open(romPath, FileMode.Open, FileAccess.ReadWrite);
            LoadLevelInfos();
        }

        void LoadLevelInfos()
        {
            BinaryReader reader = new BinaryReader(rom);

            // World entries
            world_entries = new WorldEntry[Enum.GetValues(typeof(World)).Length];
            rom.Seek(WorldEntry.BASE_ADDRESS, SeekOrigin.Begin);
            for (int w = 0; w < world_entries.Length; w++)
                world_entries[w] = Utils.ByteToType<WorldEntry>(reader);

            // Level entries
            level_entries = new LevelEntry[world_entries.Length][];
            for (int w = 0; w < world_entries.Length; w++)
            {
                WorldEntry we = world_entries[w];
                LevelEntry[] le = new LevelEntry[NUMBER_LEVELS[w]];
                rom.Seek(we.LevelInfosBaseAddress, SeekOrigin.Begin);
                for (int l = 0; l < le.Length; l++)
                    le[l] = Utils.ByteToType<LevelEntry>(reader);
                level_entries[w] = le;
            }
        }

        public LevelInfo GetLevelInfo(LevelIdentifier level)
        {
            int w = (int)level.world;
            int l = level.level;
            int base_addr = world_entries[w].WorldDataBaseAddress;
            LevelInfo res;
            res.DataBaseAddress = base_addr + level_entries[w][l].level_data_offset;
            res.DataUncompressedSize = level_entries[w][l].level_uncompressed_size;
            res.GraphicalBaseAddress = base_addr + level_entries[w][l].graphical_data_offset;
            res.GraphicalUncompressedSize = level_entries[w][l].graphical_uncompressed_size;
            res.BackgroundBaseAddress = base_addr + level_entries[w][l].background_data_offset;
            res.BackgroundUncompressedSize = level_entries[w][l].background_uncompressed_size;
            res.NextSectionBaseAddress = base_addr + level_entries[w][l].dummy1;
            return res;
        }

        public RawMapData ExtractLevelData(LevelIdentifier level)
        {
            RawMapData res;
            LevelInfo info = GetLevelInfo(level);

            rom.Seek(info.DataBaseAddress, SeekOrigin.Begin);
            long startPos = rom.Position;
            res.RawData = LzCompression.Decompress(rom, info.DataUncompressedSize);
            int length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedData = (new BinaryReader(rom)).ReadBytes(length);

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

        int WriteDataWithCompression(LevelIdentifier level, byte[] original_raw, byte[] original_compressed, byte[] new_raw, int baseAddr, int endAddr)
        {
            rom.Seek(baseAddr, SeekOrigin.Begin);
            int uncompressed_length_written;
            if (original_raw.SequenceEqual(new_raw))
            {
                rom.Write(original_compressed);
                uncompressed_length_written = original_raw.Length;
            }
            else
                uncompressed_length_written = LzCompression.Compress(rom, new_raw, endAddr);
            if (uncompressed_length_written < new_raw.Length)
                Console.WriteLine(string.Format("Warning: The new level {0} has been truncated.", level.ToString()));
            return uncompressed_length_written;
        }
        public bool AlterLevelData(LevelIdentifier level, byte[] new_data, byte[] new_graphical, byte[] new_background)
        {
            // TODO: Allow graphical and background sections to move to allow more space

            RawMapData original = ExtractLevelData(level);
            if (original.RawData.SequenceEqual(new_data) &&
                original.RawGraphical.SequenceEqual(new_graphical) &&
                original.RawBackground.SequenceEqual(new_background))
                return false;

            // Write compressed data
            int w = (int)level.world;
            int l = level.level;
            LevelInfo info = GetLevelInfo(level);
            WriteDataWithCompression(level, original.RawData, original.CompressedData, new_data, info.DataBaseAddress, info.GraphicalBaseAddress);
            level_entries[w][l].level_uncompressed_size = new_data.Length;//uncompressed_length_written;
            WriteDataWithCompression(level, original.RawGraphical, original.CompressedGraphical, new_graphical, info.GraphicalBaseAddress, info.BackgroundBaseAddress);
            level_entries[w][l].graphical_uncompressed_size = new_graphical.Length;//uncompressed_length_written;
            WriteDataWithCompression(level, original.RawBackground, original.CompressedBackground, new_background, info.BackgroundBaseAddress, info.NextSectionBaseAddress);
            level_entries[w][l].background_uncompressed_size = new_background.Length;//uncompressed_length_written;

            // Update LevelEntry structure
            rom.Seek(world_entries[w].LevelInfosBaseAddress, SeekOrigin.Begin);
            BinaryWriter writer = new BinaryWriter(rom);
            foreach (LevelEntry entry in level_entries[w])
                Utils.TypeToByte(writer, entry);

            return true;
        }

        public void Dispose()
        {
            rom.Close();
        }
    }
}
