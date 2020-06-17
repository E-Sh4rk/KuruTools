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
        public int worldInfoMemAddress;
        [FieldOffset(4)]
        public int worldDataMemAddress;
        [FieldOffset(8)]
        public int dummy;

        public int WorldInfoBaseAddress
        {
            get { return worldInfoMemAddress - 0x8000000; }
        }
        public int LevelInfosBaseAddress
        {
            get { return WorldInfoBaseAddress + 0x80; }
        }
        public int WorldDataBaseAddress
        {
            get { return worldDataMemAddress - 0x8000000; }
        }
    }
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    struct LevelEntry
    {
        [FieldOffset(0)]
        public int levelDataOffset;
        [FieldOffset(4)]
        public int levelUncompressedSize;
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
            public string shortName()
            {
                return string.Format("{0}_{1}", Enum.GetName(typeof(World), world).ToLowerInvariant(), level+1);
            }
            public string toString()
            {
                return string.Format("{0} {1}", Enum.GetName(typeof(World), world), level + 1);
            }
        }

        public static int numberOfLevels(World world)
        {
            return NUMBER_LEVELS[(int)world];
        }

        public static LevelIdentifier[] allLevels()
        {
            List<LevelIdentifier> res = new List<LevelIdentifier>();
            foreach (World w in Enum.GetValues(typeof(World)))
            {
                for (int l = 0; l < numberOfLevels(w); l++)
                    res.Add(new LevelIdentifier(w, l));
            }
            return res.ToArray();
        }

        WorldEntry[] worldEntries;
        LevelEntry[][] levelEntries;
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
            worldEntries = new WorldEntry[Enum.GetValues(typeof(World)).Length];
            rom.Seek(WorldEntry.BASE_ADDRESS, SeekOrigin.Begin);
            for (int w = 0; w < worldEntries.Length; w++)
                worldEntries[w] = Utils.ByteToType<WorldEntry>(reader);

            // Level entries
            levelEntries = new LevelEntry[worldEntries.Length][];
            for (int w = 0; w < worldEntries.Length; w++)
            {
                WorldEntry we = worldEntries[w];
                LevelEntry[] levelEntry = new LevelEntry[NUMBER_LEVELS[w]];
                rom.Seek(we.LevelInfosBaseAddress, SeekOrigin.Begin);
                for (int l = 0; l < levelEntry.Length; l++)
                    levelEntry[l] = Utils.ByteToType<LevelEntry>(reader);
                levelEntries[w] = levelEntry;
            }
        }

        public LevelInfo getLevelInfo(LevelIdentifier level)
        {
            int w = (int)level.world;
            int l = level.level;
            LevelInfo res;
            res.DataBaseAddress = worldEntries[w].WorldDataBaseAddress + levelEntries[w][l].levelDataOffset;
            res.DataUncompressedSize = levelEntries[w][l].levelUncompressedSize;
            return res;
        }

        public RawMapData extractLevelData(LevelIdentifier level)
        {
            RawMapData res;
            LevelInfo info = getLevelInfo(level);
            rom.Seek(info.DataBaseAddress, SeekOrigin.Begin);
            long startPos = rom.Position;
            res.RawData = LzCompression.decompress(rom, info.DataUncompressedSize);
            int length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedData = (new BinaryReader(rom)).ReadBytes(length);
            return res;
        }

        public bool alterLevelData(LevelIdentifier level, byte[] newRawData)
        {
            // TODO: Also alter the level info data (in case the length of the map has changed)
            // TODO: Investigate the issue with the modified training 1
            RawMapData original = extractLevelData(level);
            if (original.RawData.SequenceEqual(newRawData))
                return false;
            LevelInfo info = getLevelInfo(level);
            rom.Seek(info.DataBaseAddress, SeekOrigin.Begin);
            long startPos = rom.Position;
            LzCompression.compress(rom, newRawData);
            int length = (int)(rom.Position - startPos);
            if (length > original.CompressedData.Length)
                Console.WriteLine(string.Format("Warning: The new level {0} takes more space than the original one ({1:X} > {2:X}). It might result in a ROM corruption.",
                    level.toString(), length, original.CompressedData.Length));
            return true;
        }

        public void dispose()
        {
            rom.Close();
        }
    }
}
