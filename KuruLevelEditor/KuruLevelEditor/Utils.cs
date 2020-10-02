using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
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
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
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

        static string TimeToStr(uint cs)
        {
            uint min = cs / 6000;
            cs = cs % 6000;
            uint sec = cs / 100;
            cs = cs % 100;
            return string.Format("{0:D2}'{1:D2}''{2:D2}", min, sec, cs);
        }

        static uint StrToTime(string t)
        {
            t = t.Replace("\"", "''");
            int i1 = t.IndexOf("'");
            int i2 = t.LastIndexOf("'");
            string min = t.Substring(0, i1);
            string sec = t.Substring(i1 + 1, i2 - i1 - 2);
            string cs = t.Substring(i2 + 1);
            return Convert.ToUInt32(min) * 6000 + Convert.ToUInt32(sec) * 100 + Convert.ToUInt32(cs);
        }

        public static string UintTableToString(uint[,] table, bool timeNotation)
        {
            StringBuilder res = new StringBuilder();
            for (int j = 0; j < table.GetLength(0); j++)
            {
                for (int i = 0; i < table.GetLength(1); i++)
                {
                    if (timeNotation)
                        res.Append(TimeToStr(table[j, i]) + "   ");
                    else
                        res.Append(table[j, i].ToString().PadLeft(5, ' ') + " ");
                }
                res.Append("\n");
            }
            return res.ToString();
        }

        public static string[] SplitNonEmptyLines(string txt)
        {
            return txt.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static uint[,] LinesToUintTable(string[] lines, int height, int width, bool timeNotation)
        {
            uint[,] res = new uint[height, width];
            for (int j = 0; j < Math.Min(lines.Length, height); j++)
            {
                string[] elts = lines[j].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Math.Min(elts.Length, width); i++) {
                    try
                    {
                        if (timeNotation)
                            res[j, i] = StrToTime(elts[i]);
                        else
                            res[j, i] = Convert.ToUInt32(elts[i]);
                    }
                    catch { }
                }
            }
            return res;
        }

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
