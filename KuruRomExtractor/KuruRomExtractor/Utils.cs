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
    }
}
