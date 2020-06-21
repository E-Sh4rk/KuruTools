using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    class Utils
    {
        public static T[,] ResizeArray<T>(T[,] original, int rows, int cols)
        {
            var newArray = new T[rows, cols];
            int minRows = Math.Min(rows, original.GetLength(0));
            int minCols = Math.Min(cols, original.GetLength(1));
            for (int i = 0; i < minRows; i++)
                for (int j = 0; j < minCols; j++)
                    newArray[i, j] = original[i, j];
            return newArray;
        }
        public static T[,] FlipVertically<T>(T[,] arr)
        {
            int h = arr.GetLength(0);
            int w = arr.GetLength(1);
            var res = new T[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                    res[y, x] = arr[h - y - 1, x];
            }
            return res;
        }
        public static T[,] FlipHorizontally<T>(T[,] arr)
        {
            int h = arr.GetLength(0);
            int w = arr.GetLength(1);
            var res = new T[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                    res[y, x] = arr[y, w - x - 1];
            }
            return res;
        }
    }
}
