using System;
using System.Collections.Generic;
using System.IO;
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

        WorldEntry[] worldEntries;
        LevelEntry[][] levelEntries;
        FileStream rom;

        public Levels(string romPath)
        {
            rom = File.OpenRead(romPath);
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

        public byte[] extractLevelData(World world, int l, bool decompress = true)
        {
            LevelInfo info = getLevelInfo(world, l);
            rom.Seek(info.DataBaseAddress, SeekOrigin.Begin);
            long startPos = rom.Position;
            byte[] decompressed = LzCompression.decompress(rom, info.DataUncompressedSize);
            if (decompress)
                return decompressed;
            else
            {
                // TODO: Cleaner way to get the length of the compressed data?
                BinaryReader reader = new BinaryReader(rom);
                int length =(int)(rom.Position - startPos);
                rom.Seek(startPos, SeekOrigin.Begin);
                return reader.ReadBytes(length);
            }
        }

        public void dispose()
        {
            rom.Close();
        }
    }
}
