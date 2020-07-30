using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KuruLevelEditor
{
    class CustomInventories
    {
        const string DIR = "inventories";
        Dictionary<Levels.MapType, EditableGrid> grids;

        EditableGrid EGFromPath(Rectangle bounds, string path)
        {
            return new EditableGrid(bounds, Levels.GetGridFromLines(File.ReadAllLines(path), 0), new Point(-8, -8), 16);
        }
        public CustomInventories(Rectangle bounds)
        {
            grids = new Dictionary<Levels.MapType, EditableGrid>();
            string defaultDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
            defaultDir = Path.Combine(defaultDir, DIR);
            string dir = Path.Combine(Levels.LEVELS_DIR, DIR);

            // Load inventories
            string pathP = Path.Combine(dir, "physical.txt");
            string pathG = Path.Combine(dir, "graphical.txt");
            string pathG2 = Path.Combine(dir, "graphical2.txt");
            string pathB = Path.Combine(dir, "background.txt");
            string pathM = Path.Combine(dir, "minimap.txt");
            if (Settings.Paradise)
            {
                if (!File.Exists(pathP))
                    pathP = Path.Combine(defaultDir, "physical_paradise.txt");
                grids[Levels.MapType.Physical] = EGFromPath(bounds, pathP);
                if (!File.Exists(pathG))
                    pathG = Path.Combine(defaultDir, "graphical_paradise.txt");
                grids[Levels.MapType.Graphical] = EGFromPath(bounds, pathG);
                if (!File.Exists(pathG2))
                    pathG2 = Path.Combine(defaultDir, "graphical2_paradise.txt");
                grids[Levels.MapType.Graphical2] = EGFromPath(bounds, pathG2);
                if (!File.Exists(pathB))
                    pathB = Path.Combine(defaultDir, "background_paradise.txt");
                grids[Levels.MapType.Background] = EGFromPath(bounds, pathB);
                if (!File.Exists(pathM))
                    pathM = Path.Combine(defaultDir, "minimap_paradise.txt");
                grids[Levels.MapType.Minimap] = EGFromPath(bounds, pathM);
            }
            else
            {
                if (!File.Exists(pathP))
                    pathP = Path.Combine(defaultDir, "physical.txt");
                grids[Levels.MapType.Physical] = EGFromPath(bounds, pathP);
                if (!File.Exists(pathG))
                    pathG = Path.Combine(defaultDir, "graphical.txt");
                grids[Levels.MapType.Graphical] = EGFromPath(bounds, pathG);
                if (!File.Exists(pathB))
                    pathB = Path.Combine(defaultDir, "background.txt");
                grids[Levels.MapType.Background] = EGFromPath(bounds, pathB);
                if (!File.Exists(pathM))
                    pathM = Path.Combine(defaultDir, "minimap.txt");
                grids[Levels.MapType.Minimap] = EGFromPath(bounds, pathM);
            }
        }

        void SaveGrid(string path, int[,] grid)
        {
            File.WriteAllLines(path, Levels.GetLinesFromGrid(grid, 4, true, 0));
        }
        public void Save()
        {
            string dir = Path.Combine(Levels.LEVELS_DIR, DIR);
            Directory.CreateDirectory(dir);
            string pathP = Path.Combine(dir, "physical.txt");
            SaveGrid(pathP, grids[Levels.MapType.Physical].Grid);
            string pathG = Path.Combine(dir, "graphical.txt");
            SaveGrid(pathG, grids[Levels.MapType.Graphical].Grid);
            if (Settings.Paradise)
            {
                string pathG2 = Path.Combine(dir, "graphical2.txt");
                SaveGrid(pathG2, grids[Levels.MapType.Graphical2].Grid);
            }
            string pathB = Path.Combine(dir, "background.txt");
            SaveGrid(pathB, grids[Levels.MapType.Background].Grid);
            string pathM = Path.Combine(dir, "minimap.txt");
            SaveGrid(pathM, grids[Levels.MapType.Minimap].Grid);
        }

        public EditableGrid GetInventory(Levels.MapType type)
        {
            return grids[type];
        }

    }
}
