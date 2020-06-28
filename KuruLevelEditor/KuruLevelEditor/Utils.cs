using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace KuruLevelEditor
{
    static class Utils
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
        public static T[,] CopyArray<T>(T[,] original)
        {
            int h = original.GetLength(0);
            int w = original.GetLength(1);
            var newArray = new T[h, w];
            Array.Copy(original, newArray, h*w);
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

        public static string Escape(this string cmd)
        {
            return cmd.Replace("\"", "\\\"");
        }

        public static string RunCommand(this string cmd)
        {
            var escapedArgs = cmd.Escape();

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Environment.OSVersion.Platform == PlatformID.Win32NT ? "PowerShell.exe" : "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }

        /*public static int IntPow(int x, uint pow)
        {
            int ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }*/
    }
    class OverflowingStack<T>
    {
        private T[] items;
        private int currentIndex;
        private int count;

        public OverflowingStack(int size)
        {
            this.items = new T[size];
            this.currentIndex = 0;
            this.count = 0;
        }
        public void Push(T item)
        {
            items[currentIndex] = item;
            currentIndex++;
            currentIndex %= items.Length;
            if (count < items.Length)
                count++;
        }
        public T Pop()
        {
            if (count == 0) throw new Exception("stack is empty");
            currentIndex--;
            if (currentIndex < 0) currentIndex += items.Length;
            count--;
            return items[currentIndex];
        }
        public int Count
        {
            get { return count; }
        }
    }

}
