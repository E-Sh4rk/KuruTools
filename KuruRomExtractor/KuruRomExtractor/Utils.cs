using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace KuruRomExtractor
{
    static class Utils
    {
        // Only for blittable types
        public static T ByteToType<T>(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T structure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();

            return structure;
        }
        // Only for blittable types
        public static void TypeToByte<T>(BinaryWriter writer, T structure)
        {
            byte[] arr = new byte[Marshal.SizeOf(typeof(T))];

            GCHandle handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
            Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);
            handle.Free();

            writer.Write(arr);
        }

        public static string Uint16TableToString(ushort[,] table)
        {
            StringBuilder res = new StringBuilder();
            for (int j = 0; j < table.GetLength(0); j++)
            {
                for (int i = 0; i < table.GetLength(1); i++)
                    res.Append(table[j,i].ToString().PadLeft(5, ' ') + " ");
                res.Append("\n");
            }
            return res.ToString();
        }

        public static ushort[,] LinesToUint16Table(string[] lines, int height, int width)
        {
            ushort[,] res = new ushort[height, width];
            try
            {
                for (int j = 0; j < Math.Min(lines.Length, height); j++)
                {
                    string[] elts = lines[j].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < Math.Min(elts.Length, width); i++)
                        res[j, i] = Convert.ToUInt16(elts[i]);
                }
            }
            catch { }
            return res;
        }
    }
}
