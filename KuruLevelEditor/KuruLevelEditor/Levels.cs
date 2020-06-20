using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KuruLevelEditor
{
    class Levels
    {
        public enum MapType
        {
            Physical = 0, Graphical, Background, Minimap
        }
        static readonly string[] MAP_TYPE_STR = new string[] { "physical", "graphical", "background", "minimap" };

        const string DIR = "levels";
        public static string[] GetLevelNames()
        {
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
            return res.ToArray();
        }

        public static string GetLevelPath(string name, MapType type)
        {
            return Path.Combine(DIR, name + "." + MAP_TYPE_STR[(int)type] + ".txt");
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
        public static string[] GetLinesFromGrid(int[,] grid, int padding)
        {
            List<string> res = new List<string>();
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
