using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KuruLevelEditor
{
    public class Levels
    {
        public enum MapType
        {
            Physical = 0, Graphical, Background, Minimap
        }
        static readonly string[] MAP_TYPE_STR = new string[] { "physical", "graphical", "background", "minimap" };

        const string DIR = "levels";
        const string TILES_DIR = "tiles";

        public static string[] AllLevels { get; private set; }
        public static string[] AllWorlds { get; private set; }
        public static void Init()
        {
            if (AllLevels != null) return;
            List<string> res = new List<string>();
            foreach (string file in Directory.EnumerateFiles(DIR)) {
                string name = Path.GetFileNameWithoutExtension(file);
                name = name.Substring(0, name.IndexOf('.'));
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
        }

        public static string GetLevelPath(string name, MapType type)
        {
            return Path.Combine(DIR, name + "." + MAP_TYPE_STR[(int)type] + ".txt");
        }
        public static string GetTilePath(string world, MapType type, int nb)
        {
            return Path.Combine(TILES_DIR, world + "." + MAP_TYPE_STR[(int)type] + "." + nb.ToString("D2") + ".png");
        }
        public static string GetWorldOfLevel(string level)
        {
            return level.Substring(0, level.LastIndexOf('_'));
        }

        public static int[,] GetGridFromLines(string[] lines, int w, int h)
        {
            int[,] res = new int[h, w];
            for (int y = 0; y < h; y++)
            {
                string[] elts = lines[y].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                for (int x = 0; x < w; x++)
                {
                    res[y, x] = Convert.ToInt32(elts[x], 16);
                }
            }
            return res;
        }
        public static int[,] GetGridFromLines(string[] lines)
        {
            string[] dims = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int w = Convert.ToInt32(dims[0], 16);
            int h = Convert.ToInt32(dims[1], 16);
            string[] lines2 = new string[lines.Length - 1];
            Array.Copy(lines, 1, lines2, 0, lines2.Length);
            return GetGridFromLines(lines2, w, h);
        }
        public static string[] GetLinesFromGrid(int[,] grid, int padding, bool includeDimensions)
        {
            List<string> res = new List<string>();
            if (includeDimensions)
                res.Add(string.Format("{0:X} {1:X}", grid.GetLength(1), grid.GetLength(0)));
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                StringBuilder b = new StringBuilder();
                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    b.Append(grid[y,x].ToString("X").PadLeft(padding, ' ') + " ");
                }
                res.Add(b.ToString());
            }
            return res.ToArray();
        }
    }
}
