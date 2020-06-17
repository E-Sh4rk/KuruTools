using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace KuruTools
{
    public static class LzCompression
    {
        const int prefixMinLen = 3;
        static int prefixLength(byte[] data, int leftCursor, int rightCursor)
        {
            if (leftCursor >= rightCursor)
                return 0;
            int i = 0;
            while (rightCursor < data.Length && i < 0x7F + prefixMinLen)
            {
                if (data[leftCursor] != data[rightCursor])
                    break;
                i++;
                leftCursor++;
                rightCursor++;
            }
            return i;
        }
        static int findLongestPrefixOffset(byte[] data, int cursor)
        {
            int maxL = 0;
            int res = 0;
            int leftCursor = cursor-1;
            while (leftCursor >= 0 && cursor - leftCursor < 0xFF)
            {
                int l = prefixLength(data, leftCursor, cursor);
                if (l > maxL)
                {
                    maxL = l;
                    res = leftCursor - cursor;
                }
                leftCursor--;
            }
            return res;
        }
        public static void compress(FileStream rom, byte[] data)
        {
            BinaryWriter writer = new BinaryWriter(rom);
            int cursor = 0;
            while(cursor < data.Length)
            {
                int offset = findLongestPrefixOffset(data, cursor);
                int len = prefixLength(data, cursor + offset, cursor);
                if (len < prefixMinLen)
                {
                    len = 1;
                    while (len < 0x80 &&
                        prefixLength(data, cursor + len + findLongestPrefixOffset(data, cursor+len), cursor + len) < prefixMinLen)
                        len++;
                    writer.Write((byte)(len - 1));
                    for (; len > 0; len--)
                    {
                        writer.Write(data[cursor]);
                        cursor++;
                    }
                }
                else
                {
                    writer.Write((byte)(0x80 + len - prefixMinLen));
                    writer.Write((byte)(-offset));
                    cursor += len;
                }
            }
        }
        public static byte[] decompress(FileStream rom, int uncompressedSize)
        {
            BinaryReader reader = new BinaryReader(rom);
            byte[] res = new byte[uncompressedSize];
            int cursor = 0;
            int size = uncompressedSize;
            while (size > 0)
            {
                int len = reader.ReadByte();
                if (len < 0x80)
                {
                    len++;
                    size -= len;
                    for (; len > 0; len--)
                    {
                        res[cursor] = reader.ReadByte();
                        cursor++;
                    }
                }
                else
                {
                    int offset = -reader.ReadByte();
                    len = (len & 0x7F) + 3;
                    size -= len;
                    for (; len > 0; len--)
                    {
                        res[cursor] = res[cursor + offset];
                        cursor++;
                    }
                }
            }
            return res;
        }
    }
}
