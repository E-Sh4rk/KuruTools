using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace KuruRomExtractor
{
    // TODO: Function to normalize the level (remove useless orientation data) and thus improve compression.
    public class Map
    {
        public enum Type
        {
            PHYSICAL, GRAPHICAL, BACKGROUND, OBJECTS
        }
        ushort width;
        ushort height;
        ushort[,] data;
        Type type;

        public Map(byte[] raw, Type type)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(raw));
            if (type == Type.OBJECTS)
            {
                width = 6;
                height = (ushort)(raw.Length / (6*2));
            }
            else
            {
                width = br.ReadUInt16();
                height = br.ReadUInt16();
            }
            int remaining = (int)(raw.Length - br.BaseStream.Position);
            Debug.Assert(remaining % (2 * width) == 0);
            data = new ushort[remaining / (2*width), width];
            for (int y = 0; y < data.GetLength(0); y++)
            {
                for (int x = 0; x < data.GetLength(1); x++)
                {
                    data[y, x] = br.ReadUInt16();
                }
            }
            br.Close();
            this.type = type;
        }

        Map(ushort width, ushort height, ushort[,] data, Type type)
        {
            this.width = width;
            this.height = height;
            this.data = data;
            this.type = type;
        }

        static ushort CountLines(string[] lines)
        {
            ushort res = (ushort)lines.Length;
            while (string.IsNullOrWhiteSpace(lines[res - 1])) res--;
            return res;
        }

        public static Map Parse(string[] lines, Type type)
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

            ushort[,] map = new ushort[CountLines(lines) - lineStart, xl];
            for (ushort i = 0; i < map.GetLength(0); i++)
            {
                string[] line = lines[i + lineStart].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (ushort j = 0; j < map.GetLength(1); j++)
                    map[i, j] = Convert.ToUInt16(line[j], 16);
            }
            return new Map(xl, yl, map, type);
        }

        public byte[] ToByteData()
        {
            byte[] res = new byte[(type == Type.OBJECTS ? 0 : 4) + data.GetLength(1) * data.GetLength(0) * 2];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(res));
            if (type != Type.OBJECTS)
            {
                writer.Write(width);
                writer.Write(height);
            }
            for (int y = 0; y < data.GetLength(0); y++)
            {
                for (int x = 0; x < data.GetLength(1); x++)
                    writer.Write(data[y, x]);
            }
            writer.Close();
            return res;
        }

        public string ToString()
        {
            StringBuilder res = new StringBuilder();
            if (type != Type.OBJECTS)
                res.Append(width.ToString("X") + " " + height.ToString("X") + "\n");
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
