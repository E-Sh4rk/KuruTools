﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace KuruRomExtractor
{
    class Tiles
    {
        static byte PermuteHalfBytes(byte b)
        {
            return (byte)((b >> 4) + ((b & 0xF) << 4));
        }
        const int WIDTH = 256;
        public static Bitmap PreviewOfTilesData(byte[] data, int width, Color[] palette = null, Color? treatFirstColorAs = null)
        {
            int height = data.Length * 2 / width;
            var b = new Bitmap(width, height, PixelFormat.Format4bppIndexed);

            ColorPalette ncp = b.Palette;
            if (palette == null)
            {
                for (int i = 0; i < 16; i++)
                    ncp.Entries[i] = Color.FromArgb(255, i * 16, i * 16, i * 16);
            }
            else
            {
                for (int i = 0; i < 16; i++)
                    ncp.Entries[i] = palette[i];
            }
            if (treatFirstColorAs != null)
                ncp.Entries[0] = treatFirstColorAs.Value;
            b.Palette = ncp;

            var BoundsRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = b.LockBits(BoundsRect,
                                            ImageLockMode.WriteOnly,
                                            b.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            int bytes = bmpData.Stride * b.Height;
            var rgbValues = new byte[bytes];

            int nb_tiles_per_row = width / 8;
            for (int i = 0; i < data.Length / 32; i++)
            {
                int x = (i % nb_tiles_per_row) * 4;
                int y = (i / nb_tiles_per_row) * 8;
                for (int j = 0; j < 32; j++)
                {
                    int x2 = x + (j % 4);
                    int y2 = y + (j / 4);
                    rgbValues[y2 * width/ 2 + x2] = PermuteHalfBytes(data[i*32+j]);
                }
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            b.UnlockBits(bmpData);
            return b;
        }

        public static Bitmap PreviewOfTilesData(byte[] data, Color[] palette = null, Color? treatFirstColorAs = null)
        {
            return PreviewOfTilesData(data, WIDTH, palette, treatFirstColorAs);
        }

        public static Bitmap PreviewOf8bppTilesData(byte[] data, int width, Color[] palette = null, Color? treatFirstColorAs = null)
        {
            int height = data.Length / width;
            var b = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            ColorPalette ncp = b.Palette;
            if (palette == null)
            {
                for (int i = 0; i < 256; i++)
                    ncp.Entries[i] = Color.FromArgb(255, i, i, i);
            }
            else
            {
                for (int i = 0; i < 256; i++)
                    ncp.Entries[i] = palette[i];
            }
            if (treatFirstColorAs != null)
                ncp.Entries[0] = treatFirstColorAs.Value;
            b.Palette = ncp;

            var BoundsRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = b.LockBits(BoundsRect,
                                            ImageLockMode.WriteOnly,
                                            b.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            int bytes = bmpData.Stride * b.Height;
            var rgbValues = new byte[bytes];

            int nb_tiles_per_row = width / 8;
            for (int i = 0; i < data.Length / 64; i++)
            {
                int x = (i % nb_tiles_per_row) * 8;
                int y = (i / nb_tiles_per_row) * 8;
                for (int j = 0; j < 64; j++)
                {
                    int x2 = x + (j % 8);
                    int y2 = y + (j / 8);
                    rgbValues[y2 * width + x2] = data[i * 64 + j];
                }
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            b.UnlockBits(bmpData);
            return b;
        }

        public static Bitmap PreviewOf8bppTilesData(byte[] data, Color[] palette = null, Color? treatFirstColorAs = null)
        {
            return PreviewOf8bppTilesData(data, WIDTH, palette, treatFirstColorAs);
        }

    }
    class Palette
    {
        public Palette(byte[] data)
        {
            if (data.Length != 512)
                throw new FormatException();
            if (data == null)
            {
                Colors = new Color[0][];
                return;
            }
            Colors = new Color[16][];
            BinaryReader reader = new BinaryReader(new MemoryStream(data));
            for (int i = 0; i < Colors.Length; i++)
            {
                Color[] color = new Color[16];
                for (int j = 0; j < color.Length; j++)
                {
                    // 0b0BBB BBGG GGGR RRRR
                    int code = reader.ReadInt16();
                    int r = code & 0b11111;
                    int g = (code & 0b1111100000) >> 5;
                    int b = (code & 0b111110000000000) >> 10;
                    color[j] = Color.FromArgb(255, r << 3, g << 3, b << 3);
                }
                Colors[i] = color;
            }
            reader.Close();
        }
        public Color[][] Colors { get; private set; }
    }
}
