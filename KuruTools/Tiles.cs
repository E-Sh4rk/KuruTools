using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace KuruTools
{
    class Tiles
    {
        const int WIDTH = 256;
        public static Bitmap PreviewOfTilesData(byte[] data)
        {
            int length = data.Length * 2;
            int height = length / WIDTH;

            var b = new Bitmap(WIDTH, height, PixelFormat.Format4bppIndexed);

            ColorPalette ncp = b.Palette;
            for (int i = 0; i < 16; i++)
                ncp.Entries[i] = Color.FromArgb(255, i*16, i*16, i*16);
            b.Palette = ncp;

            var BoundsRect = new Rectangle(0, 0, WIDTH, height);
            BitmapData bmpData = b.LockBits(BoundsRect,
                                            ImageLockMode.WriteOnly,
                                            b.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            int bytes = bmpData.Stride * b.Height;
            var rgbValues = new byte[bytes];

            int nb_tiles_per_row = WIDTH / 8;
            for (int i = 0; i < data.Length / 32; i++)
            {
                int x = (i % nb_tiles_per_row) * 4;
                int y = (i / nb_tiles_per_row) * 8;
                for (int j = 0; j < 32; j++)
                {
                    int x2 = x + (j % 4);
                    int y2 = y + (j / 4);
                    rgbValues[y2 * WIDTH/2 + x2] = data[i*32+j];
                }
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            b.UnlockBits(bmpData);
            return b;

        }
    }
}
