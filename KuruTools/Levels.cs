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
        int dummy1;

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
        public int dummy1; // Next section. Starts with bytes 03 80 00 40 and seems to contain graphical informations.
        [FieldOffset(12)]
        int dummy2;
        [FieldOffset(16)]
        int dummy3;
        [FieldOffset(20)]
        int dummy4;
        [FieldOffset(24)]
        int dummy5;
        [FieldOffset(28)]
        int dummy6;
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
            public int NextSectionBaseAddress;
        }
        public struct RawMapData
        {
            public byte[] CompressedData;
            public byte[] RawData;
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
            LevelInfo res;
            res.DataBaseAddress = world_entries[w].WorldDataBaseAddress + level_entries[w][l].level_data_offset;
            res.DataUncompressedSize = level_entries[w][l].level_uncompressed_size;
            res.NextSectionBaseAddress = world_entries[w].WorldDataBaseAddress + level_entries[w][l].dummy1;
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
            return res;
        }

        public bool AlterLevelData(LevelIdentifier level, byte[] new_raw_data)
        {
            RawMapData original = ExtractLevelData(level);
            if (original.RawData.SequenceEqual(new_raw_data))
                return false;
            LevelInfo info = GetLevelInfo(level);
            rom.Seek(info.DataBaseAddress, SeekOrigin.Begin);
            if (LzCompression.Compress(rom, new_raw_data, info.NextSectionBaseAddress) < new_raw_data.Length)
                Console.WriteLine(string.Format("Warning: The new level {0} has been truncated.", level.ToString()));
            /*Console.WriteLine(string.Format("Warning: The new level {0} overlaps next section ({1:X} > {2:X}). It might result in a ROM corruption.",
                level.ToString(), rom.Position, info.NextSectionBaseAddress));*/
            // TODO: Also alter the level info data (in case the length of the map has changed, due to the provided map or due to truncature)
            return true;
        }

        public void Dispose()
        {
            rom.Close();
        }
    }
}
