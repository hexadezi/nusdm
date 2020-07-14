using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace nusdm
{
    public static class Utils
    {
		public static byte[] GetByteArrayFromHexString(string s)
		{
            for (int i = 2; i < s.Length; i += 3)
            {
                s = s.Insert(i, " ");
            }
            return s.Split(" ").Select(o => Convert.ToByte(o, 16)).ToArray();
        }

		public static UInt16 ToUInt16(this byte[] byteArray)
        {
            return BitConverter.ToUInt16(byteArray.FixEndianness(), 0);
        }

        public static UInt32 ToUInt32(this byte[] byteArray)
        {
            return BitConverter.ToUInt32(byteArray.FixEndianness(), 0);
        }

        public static UInt64 ToUInt64(this byte[] byteArray)
        {
            return BitConverter.ToUInt64(byteArray.FixEndianness(), 0);
        }

        public static byte[] FixEndianness(this byte[] byteArray)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(byteArray);

            return byteArray;
        }

        public static ulong GetFileLength(this string filePath)
        {
            return (ulong)(new FileInfo(filePath).Length);
        }
    }
}
