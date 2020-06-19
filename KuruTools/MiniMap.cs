using System;
using System.Collections.Generic;
using System.Text;

namespace KuruTools
{
    class MiniMap
    {
        const ushort width = 8 * 8;
        const ushort height = 8 * 8;
        byte[,] data;

        public MiniMap(byte[] raw)
        {
            data = new byte[height, width];
            int i = 0;
            for (int y1 = 0; y1 < 8; y1++)
            {
                for (int x1 = 0; x1 < 8; x1++)
                {
                    for (int y2 = 0; y2 < 8; y2++)
                    {
                        for (int x2 = 0; x2 < 8; x2+=2)
                        {
                            data[y1 * 8 + y2, x1 * 8 + x2] = (byte)(raw[i] & 0x0F);
                            data[y1 * 8 + y2, x1 * 8 + x2 + 1] = (byte)((raw[i] & 0xF0) >> 4);
                            i++;
                        }
                    }
                }
            }
        }

        MiniMap(byte[,] data)
        {
            this.data = data;
        }

        public static MiniMap Parse(string[] lines)
        {
            byte[,] data = new byte[height, width];
            for (ushort i = 0; i < height; i++)
            {
                string[] line = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (ushort j = 0; j < width; j++)
                    data[i, j] = Convert.ToByte(line[j], 16);
            }
            return new MiniMap(data);
        }

        public byte[] ToByteData()
        {
            byte[] res = new byte[0x800];
            int i = 0;
            for (int y1 = 0; y1 < 8; y1++)
            {
                for (int x1 = 0; x1 < 8; x1++)
                {
                    for (int y2 = 0; y2 < 8; y2++)
                    {
                        for (int x2 = 0; x2 < 8; x2 += 2)
                        {
                            res[i] = (byte)(data[y1 * 8 + y2, x1 * 8 + x2] + (data[y1 * 8 + y2, x1 * 8 + x2 + 1] << 4));
                            i++;
                        }
                    }
                }
            }
            return res;
        }

        public string ToString()
        {
            StringBuilder res = new StringBuilder();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    res.Append(data[y, x].ToString("X") + " ");
                res.Append("\n");
            }
            return res.ToString();
        }
    }
}
