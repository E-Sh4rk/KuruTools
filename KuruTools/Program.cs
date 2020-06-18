using System;
using System.IO;

namespace KuruTools
{
    // TODO: Document public functions
    class Program
    {
        /// <summary>
        /// Extract and edit Kururin ROM map data.
        /// </summary>
        /// <param name="input">The path to the input ROM</param>
        /// <param name="output">The target path for the altered ROM</param>
        /// <param name="workspace">The path to the directory containing the level data</param>
        /// <param name="debug">Print debug information</param>
        static void Main(string input = "input.gba", string output = "output.gba", string workspace = "levels", bool debug = false)
        {
            Console.WriteLine("=== Kuru Kuru Kururin Tools ===");
            Console.WriteLine("");
            File.Copy(input, output, true);
            Levels levels = new Levels(output);

            // Some tests and experiments...
            /*Levels.RawMapData raw_lvl = levels.extractLevelData(new Levels.LevelIdentifier(Levels.World.TRAINING, 0));
            File.WriteAllBytes("training1_compressed.bin", raw_lvl.CompressedData);*/

            // Print all level infos
            if (debug)
            {
                foreach (Levels.World w in Enum.GetValues(typeof(Levels.World)))
                {
                    Console.WriteLine(string.Format("===== {0} =====", Enum.GetName(typeof(Levels.World), w)));
                    for (int l = 0; l < Levels.NumberOfLevels(w); l++)
                    {
                        Levels.LevelInfo info = levels.GetLevelInfo(new Levels.LevelIdentifier(w, l));
                        Console.WriteLine(string.Format("Level {1} Data Base Address: {0:X}", info.DataBaseAddress, l + 1));
                        Console.WriteLine(string.Format("Level {1} Uncompressed Size: {0:X}", info.DataUncompressedSize, l + 1));
                    }
                    Console.WriteLine("");
                }
            }

            Directory.CreateDirectory(workspace);
            foreach (Levels.LevelIdentifier level in Levels.AllLevels()) // TODO: Decrypt the minimap format and export it in a more convenient one
            {
                string filename = Path.Combine(workspace, level.ShortName() + ".txt");
                string filename_graphical = Path.Combine(workspace, level.ShortName() + "_graphical.txt");
                string filename_background = Path.Combine(workspace, level.ShortName() + "_background.txt");
                string filename_minimap = Path.Combine(workspace, level.ShortName() + "_minimap.bin");

                byte[] p = null;
                byte[] g = null;
                byte[] b = null;
                byte[] m = null;
                if (File.Exists(filename))
                    p = Map.Parse(File.ReadAllLines(filename), Map.Type.PHYSICAL).ToByteData();
                if (File.Exists(filename_graphical))
                    g = Map.Parse(File.ReadAllLines(filename_graphical), Map.Type.GRAPHICAL).ToByteData();
                if (File.Exists(filename_background))
                    b = Map.Parse(File.ReadAllLines(filename_background), Map.Type.BACKGROUND).ToByteData();
                if (File.Exists(filename_minimap))
                    m = File.ReadAllBytes(filename_minimap);

                // Export map if not already present
                if (p == null || g == null || b == null || m == null)
                {
                    Levels.RawMapData raw = levels.ExtractLevelData(level);
                    if (p == null)
                    {
                        Map mp = new Map(raw.RawData, Map.Type.PHYSICAL);
                        File.WriteAllText(filename, mp.ToString());
                    }
                    if (g == null)
                    {
                        Map mg = new Map(raw.RawGraphical, Map.Type.GRAPHICAL);
                        File.WriteAllText(filename_graphical, mg.ToString());
                    }
                    if (b == null)
                    {
                        Map mb = new Map(raw.RawBackground, Map.Type.BACKGROUND);
                        File.WriteAllText(filename_background, mb.ToString());
                    }
                    if (m == null)
                    {
                        File.WriteAllBytes(filename_minimap, raw.RawMinimap);
                    }
                    Console.WriteLine("Missing components for " + level.ToString() + ". Missing data has been exported.");
                }

                // Alter map in the ROM
                if (levels.AlterLevelData(level, p, g, b, m))
                    Console.WriteLine("Changes detected in " + level.ToString() + ". The ROM has been updated.");
            }

            // Some tests and experiments...
            /*Levels.RawMapData raw_gl = levels.extractLevelData(new Levels.LevelIdentifier(Levels.World.GRASS, 0));
            File.WriteAllBytes("grassland1_raw.bin", raw_gl.RawData);
            Map m = new Map(raw_gl.RawData);
            File.WriteAllText("grassland1.txt", m.toString());
            File.WriteAllBytes("grassland1_raw_2.bin", m.toByteData());
            File.WriteAllBytes("grassland1_compressed.bin", raw_gl.CompressedData);
            FileStream f = File.OpenWrite("grassland1_compressed_2.bin");
            LzCompression.compress(f, raw_gl.RawData);
            f.Close();*/
            /*Map lvl = Map.parse(File.ReadAllLines("levels/training_1.txt"));
            FileStream f = File.OpenWrite("training1_compressed_2.bin");
            LzCompression.compress(f, lvl.toByteData());
            f.Close();
            f = File.OpenRead("training1_compressed_2.bin");
            byte[] raw = LzCompression.decompress(f, 0x4004);
            f.Close();
            lvl = new Map(raw);
            File.WriteAllText("training1_reparsed.txt", lvl.toString());*/

            Console.WriteLine("All tasks terminated.");
            levels.Dispose();
        }
    }
}
