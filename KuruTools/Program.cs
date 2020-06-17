using System;
using System.IO;

namespace KuruTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify the path to the ROM as argument.");
                return;
            }
            Console.WriteLine("=== Kuru Kuru Kururin Tools ===");
            Console.WriteLine("");
            string romPath = args[0];
            string destRomPath = args.Length >= 2 ? args[1] : "output.gba";
            File.Copy(romPath, destRomPath, true);
            Levels levels = new Levels(destRomPath);

            // Print all level infos
            foreach (Levels.World w in Enum.GetValues(typeof(Levels.World)))
            {
                Console.WriteLine(string.Format("===== {0} =====", Enum.GetName(typeof(Levels.World), w)));
                for (int l = 0; l < levels.numberOfLevels(w); l++)
                {
                    Levels.LevelInfo info = levels.getLevelInfo(w, l);
                    Console.WriteLine(string.Format("Level {1} Data Base Address: {0:X}", info.DataBaseAddress, l+1));
                    Console.WriteLine(string.Format("Level {1} Uncompressed Size: {0:X}", info.DataUncompressedSize, l+1));
                }
                Console.WriteLine("");
            }
            // Some tests and experiments...
            /*Levels.RawMapData raw_gl = levels.extractLevelData(Levels.World.GRASS, 0);
            File.WriteAllBytes("grassland1_raw.bin", raw_gl.RawData);
            Map m = new Map(raw_gl.RawData);
            File.WriteAllText("grassland1.txt", m.toString());
            File.WriteAllBytes("grassland1_raw_2.bin", m.toByteData());
            File.WriteAllBytes("grassland1_compressed.bin", raw_gl.CompressedData);
            FileStream f = File.OpenWrite("grassland1_compressed_2.bin");
            LzCompression.compress(f, raw_gl.RawData);
            f.Close();*/
            Map gl1 = Map.parse(File.ReadAllLines("grassland1.txt"));
            if (levels.alterLevelData(Levels.World.GRASS, 0, gl1.toByteData()))
                Console.WriteLine("Change detected in Grassland 1. The ROM has been updated.");

            Console.WriteLine("Tasks terminated.");
            Console.ReadLine();
            levels.dispose();
        }
    }
}
