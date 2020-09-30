using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;

namespace KuruRomExtractor
{
    public class Map
    {
        public enum Type
        {
            PHYSICAL, GRAPHICAL, BACKGROUND, OBJECTS
        }
        ushort[,] data;
        Type type;
        bool compact;

        public Map(byte[] raw, Type type, bool compact)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(raw));
            int width, height;
            int tileSize = compact ? 1 : 2;
            if (type == Type.OBJECTS)
            {
                width = 6;
                height = (ushort)(raw.Length / (6 * tileSize));
            }
            else
            {
                width = br.ReadUInt16();
                height = br.ReadUInt16();
            }
            data = new ushort[height, width];
            for (int y = 0; y < data.GetLength(0); y++)
            {
                for (int x = 0; x < data.GetLength(1); x++)
                {
                    if (tileSize == 1)
                        data[y, x] = br.ReadByte();
                    else
                        data[y, x] = br.ReadUInt16();
                }
            }
            br.Close();
            this.type = type;
            this.compact = compact;
        }

        Map(ushort[,] data, Type type, bool compact)
        {
            this.data = data;
            this.type = type;
            this.compact = compact;
        }

        public int NumberOfAreas()
        {
            if (type != Type.PHYSICAL) return 0;
            int[] HEALING_ZONE_IDS = new int[]{ 0xED, 0xEE, 0xEF };
            int max = -1;
            foreach(ushort d in data)
            {
                int id = d & 0x3FF;
                max = Math.Max(max, Array.IndexOf(HEALING_ZONE_IDS, id));
            }
            return max + 2;
        }

        static ushort CountLines(string[] lines)
        {
            ushort res = (ushort)lines.Length;
            while (string.IsNullOrWhiteSpace(lines[res - 1])) res--;
            return res;
        }

        public static Map Parse(string[] lines, Type type, bool compact)
        {
            ushort xl;
            ushort yl;
            int lineStart;
            if (type == Type.OBJECTS)
            {
                xl = 6;
                yl = CountLines(lines);
                lineStart = 0;
            }
            else
            {
                string[] headers = lines[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                xl = Convert.ToUInt16(headers[0], 16);
                yl = Convert.ToUInt16(headers[1], 16);
                lineStart = 1;
            }

            ushort[,] map = new ushort[yl, xl];
            for (ushort i = 0; i < map.GetLength(0); i++)
            {
                string[] line = lines[i + lineStart].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (ushort j = 0; j < map.GetLength(1); j++)
                    map[i, j] = Convert.ToUInt16(line[j], 16);
            }
            return new Map(map, type, compact);
        }

        public byte[] ToByteData()
        {
            int tileSize = compact ? 1 : 2;
            byte[] res = new byte[(type == Type.OBJECTS ? 0 : 4) + data.GetLength(1) * data.GetLength(0) * tileSize];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(res));
            if (type != Type.OBJECTS)
            {
                writer.Write((ushort)data.GetLength(1));
                writer.Write((ushort)data.GetLength(0));
            }
            for (int y = 0; y < data.GetLength(0); y++)
            {
                for (int x = 0; x < data.GetLength(1); x++)
                {
                    if (tileSize == 1)
                        writer.Write((byte)data[y, x]);
                    else
                        writer.Write(data[y, x]);
                }
            }
            writer.Close();
            return res;
        }

        public string ToString()
        {
            StringBuilder res = new StringBuilder();
            if (type != Type.OBJECTS)
                res.Append(data.GetLength(1).ToString("X") + " " + data.GetLength(0).ToString("X") + "\n");
            for (int y = 0; y < data.GetLength(0); y++)
            {
                for (int x = 0; x < data.GetLength(1); x++)
                    res.Append(data[y, x].ToString("X").PadLeft(4, ' ') + " ");
                res.Append("\n");
            }
            return res.ToString();
        }
    }
}
