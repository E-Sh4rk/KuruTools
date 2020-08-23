/*

KuruRom Extractor
Author: Mickael LAURENT

For documentation about the GBA (memory sections, tiled video mode, etc.),
you can refer to this awesome tutorial:

https://www.coranac.com/tonc/text/toc.htm

*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace KuruRomExtractor
{
    // TODO: Document public functions
    class Program
    {
        static string NameOfROM(string path)
        {
            if (!File.Exists(path)) return "";
            StringBuilder res = new StringBuilder();
            FileStream stream = File.OpenRead(path);
            stream.Seek(0xA0, SeekOrigin.Begin);
            int b;
            while ((b = stream.ReadByte()) > 0) {
                res.Append((char)b);
            }
            stream.Close();
            return res.ToString();
        }
        /// <summary>
        /// Extract and edit Kururin ROM map data.
        /// </summary>
        /// <param name="identifyOnly">Only identify the ROM, then exit.</param>
        /// <param name="input">The path to the input ROM</param>
        /// <param name="output">The target path for the altered ROM</param>
        /// <param name="workspace">The path to the directory containing the level data</param>
        /// <param name="relocate">Relocate the new maps at the end of the ROM (prevent overlapping, but increase ROM size)</param>
        /// <param name="extractTiles">Path to the directory where tiles will be extracted</param>
        static void Main(bool identifyOnly = false, string input = "input.gba", string output = "output.gba", string workspace = "levels",
            bool relocate = true, string extractTiles = null)
        {
            string name = NameOfROM(input);

            if (identifyOnly)
            {
                Console.WriteLine(name);
                return;
            }

            Console.WriteLine("=== Kururin ROM Extractor ===");
            Console.WriteLine("");
            Console.WriteLine("ROM detected: " + name);
            Console.WriteLine("");

            if (name == "KURUPARA")
            {
                if (!relocate) Console.WriteLine("Warning: Kururin Paradise mode only supports relocation.");
                File.Copy(input, output, true);
                ParadiseLevels levels = new ParadiseLevels(output);

                if (!string.IsNullOrEmpty(extractTiles))
                {
                    Console.WriteLine("Extracting tiles...");
                    Directory.CreateDirectory(extractTiles);
                    byte[] commonPaletteData = levels.ExtractCommonPaletteData();
                    foreach (int level in ParadiseLevels.AllLevels())
                    {
                        byte[][] data = levels.ExtractTilesData(level);
                        Array.Copy(data[5], 0, data[4], 0, Levels.COLORSET_SIZE);
                        Array.Copy(commonPaletteData, 0, data[4], data[4].Length - commonPaletteData.Length, commonPaletteData.Length);
                        Palette palette = new Palette(data[4]);
                        int lastLayer = 2;
                        ParadiseLevels.LevelInfo info = levels.GetLevelInfo(level);
                        if (info.BackgroundBaseAddress == 0)
                        {
                            lastLayer--;
                            if (info.Graphical2BaseAddress == 0)
                                lastLayer--;
                        }
                        for (int i = 0; i < data.Length; i++)
                        {
                            if (i < 4)
                            {
                                byte[] d = data[i];
                                if (d != null)
                                {
                                    string type = (new string[] { "graphical", "graphical2", "background", "physical" })[i];
                                    Color firstColor = i >= lastLayer && i <= 2 ? palette.Colors[0][0] : Color.Transparent;
                                    Bitmap bmp8bpp = null;
                                    if (i == 1 && levels.AreGraphical2Tiles8bpp(level))
                                    {
                                        List<Color> fullPalette = new List<Color>();
                                        foreach (Color[] colors in palette.Colors)
                                            fullPalette.AddRange(colors);
                                        bmp8bpp = Tiles.PreviewOf8bppTilesData(d, fullPalette.ToArray(), firstColor);
                                    }
                                    for (int j = 0; j < palette.Colors.Length; j++)
                                    {
                                        string filename_png = string.Format("{0:D2}.{1}.{2:D2}.png", level, type, j);
                                        if (bmp8bpp == null)
                                            Tiles.PreviewOfTilesData(d, palette.Colors[j], firstColor).Save(Path.Combine(extractTiles, filename_png));
                                        else
                                            bmp8bpp.Save(Path.Combine(extractTiles, filename_png));
                                    }
                                }
                            }
                        }
                    }
                }

                Directory.CreateDirectory(workspace);
                byte[] nbAreasTable = levels.GetNumberAreasTable();
                foreach (int level in ParadiseLevels.AllLevels())
                {
                    string filename = Path.Combine(workspace, level.ToString("D2") + ".physical.txt");
                    string filename_objects = Path.Combine(workspace, level.ToString("D2") + ".objects.txt");
                    string filename_graphical = Path.Combine(workspace, level.ToString("D2") + ".graphical.txt");
                    string filename_graphical2 = Path.Combine(workspace, level.ToString("D2") + ".graphical2.txt");
                    string filename_background = Path.Combine(workspace, level.ToString("D2") + ".background.txt");
                    string filename_minimap = Path.Combine(workspace, level.ToString("D2") + ".minimap.txt");
                    bool isGraphical2Compact = levels.AreGraphical2Tiles8bpp(level);

                    byte[] p = null;
                    byte[] o = null;
                    byte[] g = null;
                    byte[] g2 = null;
                    byte[] b = null;
                    byte[] m = null;
                    if (File.Exists(filename))
                    {
                        Map mp = Map.Parse(File.ReadAllLines(filename), Map.Type.PHYSICAL, false);
                        p = mp.ToByteData();
                        if (level < nbAreasTable.Length)
                            nbAreasTable[level] = (byte)mp.NumberOfAreas();
                    }
                    if (File.Exists(filename_objects))
                        o = Map.Parse(File.ReadAllLines(filename_objects), Map.Type.OBJECTS, false).ToByteData();
                    if (File.Exists(filename_graphical))
                        g = Map.Parse(File.ReadAllLines(filename_graphical), Map.Type.GRAPHICAL, false).ToByteData();
                    if (File.Exists(filename_graphical2))
                        g2 = Map.Parse(File.ReadAllLines(filename_graphical2), Map.Type.GRAPHICAL, isGraphical2Compact).ToByteData();
                    if (File.Exists(filename_background))
                        b = Map.Parse(File.ReadAllLines(filename_background), Map.Type.BACKGROUND, false).ToByteData();
                    if (File.Exists(filename_minimap))
                        m = MiniMap.Parse(File.ReadAllLines(filename_minimap)).ToByteData();

                    // Export map if not already present
                    if (p == null || o == null || g == null || g2 == null || b == null || m == null)
                    {
                        bool exported = false;
                        ParadiseLevels.RawMapData raw = levels.ExtractLevelData(level);
                        if (p == null)
                        {
                            Map mp = new Map(raw.RawData, Map.Type.PHYSICAL, false);
                            File.WriteAllText(filename, mp.ToString());
                            exported = true;
                        }
                        if (o == null)
                        {
                            Map mo = new Map(raw.RawObjects, Map.Type.OBJECTS, false);
                            File.WriteAllText(filename_objects, mo.ToString());
                            exported = true;
                        }
                        if (g == null && raw.RawGraphical.Length > 0)
                        {
                            Map mg = new Map(raw.RawGraphical, Map.Type.GRAPHICAL, false);
                            File.WriteAllText(filename_graphical, mg.ToString());
                            exported = true;
                        }
                        if (g2 == null)
                        {
                            Map mg2 = new Map(raw.RawGraphical2, Map.Type.GRAPHICAL, isGraphical2Compact);
                            File.WriteAllText(filename_graphical2, mg2.ToString());
                            exported = true;
                        }
                        if (b == null && raw.RawBackground.Length > 0)
                        {
                            Map mb = new Map(raw.RawBackground, Map.Type.BACKGROUND, false);
                            File.WriteAllText(filename_background, mb.ToString());
                            exported = true;
                        }
                        if (m == null)
                        {
                            MiniMap mm = new MiniMap(raw.RawMinimap);
                            File.WriteAllText(filename_minimap, mm.ToString());
                            exported = true;
                        }
                        if (exported)
                            Console.WriteLine("Missing components for " + level.ToString() + ". Missing data has been exported.");
                    }

                    // Alter map in the ROM
                    if (levels.AlterLevelData(level, p, o, g, g2, b, m))
                        Console.WriteLine("Changes detected in " + level.ToString() + ". The ROM has been updated.");
                }
                levels.SetNumberAreasTable(nbAreasTable);

                Console.WriteLine("All tasks terminated.");
                levels.Dispose();
            }
            else if (name == "KURURIN")
            {
                File.Copy(input, output, true);
                Levels levels = new Levels(output);

                if (!string.IsNullOrEmpty(extractTiles))
                {
                    Console.WriteLine("Extracting tiles...");
                    Directory.CreateDirectory(extractTiles);
                    byte[] physicalTilesData = levels.ExtractPhysicalTilesData();
                    //File.WriteAllBytes(Path.Combine(extractTiles, "physical_tiles.bin"), physicalTilesData);
                    byte[] challengePhysicalTilesData = levels.ExtractChallengePhysicalTilesData();
                    //File.WriteAllBytes(Path.Combine(extractTiles, "physical_tiles_challenge.bin"), challengePhysicalTilesData);
                    byte[] commonPaletteData = levels.ExtractCommonPaletteData();
                    //File.WriteAllBytes(Path.Combine(extractTiles, "common_palette.bin"), commonPaletteData);
                    foreach (Levels.World w in Enum.GetValues(typeof(Levels.World)))
                    {
                        byte[][] data = levels.ExtractWorldData(w);
                        if (data[2] == null)
                        {
                            if (w == Levels.World.CHALLENGE)
                                data[2] = challengePhysicalTilesData;
                            else
                                data[2] = physicalTilesData;
                        }

                        Array.Copy(data[6], 0, data[4], 0, Levels.COLORSET_SIZE);
                        Array.Copy(commonPaletteData, 0, data[4], data[4].Length - commonPaletteData.Length, commonPaletteData.Length);
                        Palette palette = new Palette(data[4]);
                        for (int i = 0; i < data.Length; i++)
                        {
                            byte[] d = data[i];
                            if (d != null)
                            {
                                //string filename_bin = string.Format("{0}.{1:D2}.bin", Levels.LevelIdentifier.WorldShortName(w), i);
                                //File.WriteAllBytes(Path.Combine(extractTiles, filename_bin), d);
                                if (i < 4)
                                {
                                    string type = (new string[] { "graphical", "background", "physical", "sprites" })[i];
                                    Color firstColor = i == 1 ? palette.Colors[0][0] : Color.Transparent;
                                    for (int j = 0; j < palette.Colors.Length; j++)
                                    {
                                        string filename_png = string.Format("{0}.{1}.{2:D2}.png", Levels.LevelIdentifier.WorldShortName(w), type, j);
                                        Tiles.PreviewOfTilesData(d, palette.Colors[j], firstColor).Save(Path.Combine(extractTiles, filename_png));
                                    }
                                }
                            }
                        }
                    }
                }

                Directory.CreateDirectory(workspace);
                byte[] nbAreasTable = levels.GetNumberAreasTable();
                int levelNb = 0;
                foreach (Levels.LevelIdentifier level in Levels.AllLevels())
                {
                    string filename = Path.Combine(workspace, level.ShortName() + ".physical.txt");
                    string filename_graphical = Path.Combine(workspace, level.ShortName() + ".graphical.txt");
                    string filename_background = Path.Combine(workspace, level.ShortName() + ".background.txt");
                    string filename_minimap = Path.Combine(workspace, level.ShortName() + ".minimap.txt");

                    byte[] p = null;
                    byte[] g = null;
                    byte[] b = null;
                    byte[] m = null;
                    if (File.Exists(filename))
                    {
                        Map mp = Map.Parse(File.ReadAllLines(filename), Map.Type.PHYSICAL, false);
                        p = mp.ToByteData();
                        if (levelNb < nbAreasTable.Length)
                            nbAreasTable[levelNb] = (byte)mp.NumberOfAreas();
                    }
                    if (File.Exists(filename_graphical))
                        g = Map.Parse(File.ReadAllLines(filename_graphical), Map.Type.GRAPHICAL, false).ToByteData();
                    if (File.Exists(filename_background))
                        b = Map.Parse(File.ReadAllLines(filename_background), Map.Type.BACKGROUND, false).ToByteData();
                    if (File.Exists(filename_minimap))
                        m = MiniMap.Parse(File.ReadAllLines(filename_minimap)).ToByteData();

                    // Export map if not already present
                    if (p == null || g == null || b == null || m == null)
                    {
                        Levels.RawMapData raw = levels.ExtractLevelData(level);
                        if (p == null)
                        {
                            Map mp = new Map(raw.RawData, Map.Type.PHYSICAL, false);
                            File.WriteAllText(filename, mp.ToString());
                        }
                        if (g == null)
                        {
                            Map mg = new Map(raw.RawGraphical, Map.Type.GRAPHICAL, false);
                            File.WriteAllText(filename_graphical, mg.ToString());
                        }
                        if (b == null)
                        {
                            Map mb = new Map(raw.RawBackground, Map.Type.BACKGROUND, false);
                            File.WriteAllText(filename_background, mb.ToString());
                        }
                        if (m == null)
                        {
                            MiniMap mm = new MiniMap(raw.RawMinimap);
                            File.WriteAllText(filename_minimap, mm.ToString());
                        }
                        Console.WriteLine("Missing components for " + level.ToString() + ". Missing data has been exported.");
                    }

                    // Alter map in the ROM
                    if (levels.AlterLevelData(level, p, g, b, m, relocate))
                        Console.WriteLine("Changes detected in " + level.ToString() + ". The ROM has been updated.");

                    levelNb++;
                }
                levels.SetNumberAreasTable(nbAreasTable);

                Console.WriteLine("All tasks terminated.");
                levels.Dispose();
            }
            else
                Console.WriteLine("Unsupported ROM. Exiting.");
        }
    }
}
