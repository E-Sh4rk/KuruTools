using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KuruRomExtractor
{
    // TODO: Function to normalize the level (remove useless orientation data) and thus improve compression.
    public class Map
    {
        public enum Type
        {
            PHYSICAL, GRAPHICAL, BACKGROUND
        }
        ushort width;
        ushort height;
        ushort[,] data;
        Type type;

        public Map(byte[] raw, Type type)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(raw));
            width = br.ReadUInt16();
            height = br.ReadUInt16();
            data = new ushort[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    data[y, x] = br.ReadUInt16();
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

        public static Map Parse(string[] lines, Type type)
        {
            string[] headers = lines[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            ushort xl = Convert.ToUInt16(headers[0], 16);
            ushort yl = Convert.ToUInt16(headers[1], 16);
            ushort[,] map = new ushort[yl, xl];
            for (ushort i = 0; i < yl; i++)
            {
                string[] line = lines[i + 1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (ushort j = 0; j < xl; j++)
                    map[i, j] = Convert.ToUInt16(line[j], 16);
            }
            return new Map(xl, yl, map, type);
        }

        public byte[] ToByteData()
        {
            byte[] res = new byte[4 + width * height * 2];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(res));
            writer.Write(width);
            writer.Write(height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    writer.Write(data[y, x]);
            }
            writer.Close();
            return res;
        }

        public string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.Append(width.ToString("X") + " " + height.ToString("X") + "\n");
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    res.Append(data[y, x].ToString("X").PadLeft(4, ' ') + " ");
                res.Append("\n");
            }
            return res.ToString();
        }
    }
}
