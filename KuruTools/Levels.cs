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

        public int numberOfLevels(World world)
        {
            return NUMBER_LEVELS[(int)world];
        }

        public LevelInfo getLevelInfo(World world, int l)
        {
            int w = (int)world;
            LevelInfo res;
            res.DataBaseAddress = worldEntries[w].WorldDataBaseAddress + levelEntries[w][l].levelDataOffset;
            res.DataUncompressedSize = levelEntries[w][l].levelUncompressedSize;
            return res;
        }

        public RawMapData extractLevelData(World world, int l)
        {
            RawMapData res;
            LevelInfo info = getLevelInfo(world, l);
            rom.Seek(info.DataBaseAddress, SeekOrigin.Begin);
            long startPos = rom.Position;
            res.RawData = LzCompression.decompress(rom, info.DataUncompressedSize);
            int length = (int)(rom.Position - startPos);
            rom.Seek(startPos, SeekOrigin.Begin);
            res.CompressedData = (new BinaryReader(rom)).ReadBytes(length);
            return res;
        }

        public bool alterLevelData(World world, int l, byte[] newRawData)
        {
            RawMapData original = extractLevelData(world, l);
            if (original.RawData.SequenceEqual(newRawData))
                return false;
            LevelInfo info = getLevelInfo(world, l);
            rom.Seek(info.DataBaseAddress, SeekOrigin.Begin);
            long startPos = rom.Position;
            LzCompression.compress(rom, newRawData);
            int length = (int)(rom.Position - startPos);
            if (length > original.CompressedData.Length)
                Console.WriteLine("Warning: The new level takes more space than the original one. It might result in a ROM corruption.");
            return true;
        }

        public void dispose()
        {
            rom.Close();
        }
    }
}
