using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KuruLevelEditor
{
    public class Levels
    {
        const string CHALLENGE_NAME = "12_challenge";
        const int CHALLENGE_BACKGROUND_OFFSET = 0x100;

        public enum MapType
        {
            Physical = 0, Graphical, Graphical2, Background, Minimap
        }
        static readonly string[] MAP_TYPE_STR = new string[] { "physical", "graphical", "graphical2", "background", "minimap" };

        public static string LEVELS_DIR = Settings.Paradise ? Path.GetFullPath("paradise_levels") : Path.GetFullPath("levels");
        public static string TILES_DIR = Settings.Paradise ? Path.GetFullPath("paradise_tiles") : Path.GetFullPath("tiles");

        public static string[] AllLevels { get; private set; }
        public static string[] AllWorlds { get; private set; }
        public static bool Init()
        {
            try
            {
                List<string> res = new List<string>();
                foreach (string file in Directory.EnumerateFiles(LEVELS_DIR))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    name = name.Substring(0, Math.Max(name.IndexOf('.'), 0));
                    if (string.IsNullOrEmpty(name))
                        continue;
                    res.Add(name);
                }
                res.Sort();
                res = res.Distinct().ToList();
                AllLevels = res.ToArray();
                for (int i = 0; i < res.Count; i++)
                    res[i] = GetWorldOfLevel(res[i]);
                res = res.Distinct().ToList();
                AllWorlds = res.ToArray();
                return true;
            }
            catch { }
            return false;
        }

        public static void DeleteAllLevels()
        {
            Directory.Delete(LEVELS_DIR, true);
        }
        public static string GetTimesPath()
        {
            return Settings.Paradise ? Path.Combine(LEVELS_DIR, "times_paradise.txt")
                : Path.Combine(LEVELS_DIR, "times.txt");
        }
        public static string GetLevelPath(string name, MapType type)
        {
            return Path.Combine(LEVELS_DIR, name + "." + MAP_TYPE_STR[(int)type] + ".txt");
        }
        public static string GetObjectsPath(string name)
        {
            if (!Settings.Paradise)
                return null;
            return Path.Combine(LEVELS_DIR, name + ".objects.txt");
        }
        public static string GetTilePath(string world, MapType type, int nb)
        {
            return Path.Combine(TILES_DIR, world + "." + MAP_TYPE_STR[(int)type] + "." + nb.ToString("D2") + ".png");
        }
        public static string GetWorldOfLevel(string level)
        {
            if (Settings.Paradise)
                return level;
            return level.Substring(0, level.LastIndexOf('_'));
        }

        public static int TilesOffset(string world, MapType type)
        {
            return type == MapType.Background && world == CHALLENGE_NAME ? CHALLENGE_BACKGROUND_OFFSET : 0;
        }

        public static int[,] GetGridFromLines(string[] lines, int w, int h, int tilesOffset)
        {
            int[,] res = new int[h, w];
            for (int y = 0; y < h; y++)
            {
                string[] elts = lines[y].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                for (int x = 0; x < w; x++)
                    res[y, x] = Convert.ToInt32(elts[x], 16) - tilesOffset;
            }
            return res;
        }
        public static int[,] GetGridFromLines(string[] lines, int tilesOffset)
        {
            string[] dims = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int w = Convert.ToInt32(dims[0], 16);
            int h = Convert.ToInt32(dims[1], 16);
            string[] lines2 = new string[lines.Length - 1];
            Array.Copy(lines, 1, lines2, 0, lines2.Length);
            return GetGridFromLines(lines2, w, h, tilesOffset);
        }
        public static string[] GetLinesFromGrid(int[,] grid, int padding, bool includeDimensions, int tilesOffset)
        {
            List<string> res = new List<string>();
            if (includeDimensions)
                res.Add(string.Format("{0:X} {1:X}", grid.GetLength(1), grid.GetLength(0)));
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                StringBuilder b = new StringBuilder();
                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    int v = grid[y, x] + tilesOffset;
                    b.Append(v.ToString("X").PadLeft(padding, ' ') + " ");
                }
                res.Add(b.ToString());
            }
            return res.ToArray();
        }
    }
}
