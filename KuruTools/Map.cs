using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KuruTools
{
    public class Map
    {
        ushort width;
        ushort height;
        ushort[,] data;

        public Map(byte[] raw)
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
        }

        Map(ushort width, ushort height, ushort[,] data)
        {
            this.width = width;
            this.height = height;
            this.data = data;
        }

        public static Map parse(string[] lines)
        {
            string[] headers = lines[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            ushort xl = Convert.ToUInt16(headers[0]);
            ushort yl = Convert.ToUInt16(headers[1]);
            ushort[,] map = new ushort[yl, xl];
            for (ushort i = 0; i < yl; i++)
            {
                string[] line = lines[i + 1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (ushort j = 0; j < xl; j++)
                    map[i, j] = Convert.ToUInt16(line[j]);
            }
            return new Map(xl, yl, map);
        }

        public byte[] toByteData()
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

        public string toString()
        {
            string res = width.ToString() + " " + height.ToString() + "\n";
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    res += data[y, x].ToString() + " ";
                res += "\n";
            }
            return res;
        }
    }
}
