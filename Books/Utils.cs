﻿using System;
using ExtensionMethods;
using System.Globalization;
using System.Collections.Generic;
using System.IO;

namespace Utils
{
    class PalmDoc
    {
        /// <summary>
        /// 
        /// </summary>
        /// 

        public static string decompress(byte[] buffer, int compressedLen)
        {
            byte[] output = new byte[decompressedLength(buffer, compressedLen)];
            int i = 0;
            int j = 0;
            while (i < compressedLen)
            {
                int c = buffer[i++];

                if (c >= 0xc0)
                {
                    output[j++] = (byte)' ';
                    output[j++] = (byte)(c & 0x7f);
                }
                else if (c >= 0x80)
                {
                    c = (c << 8) + buffer[i++];
                    int windowLen = (c & 0x0007) + 3;
                    int windowDist = (c >> 3) & 0x07FF;
                    int windowCopyFrom = j - windowDist;

                    windowLen = Math.Min(windowLen, output.Length - j);

                    while (windowLen-- > 0)
                    {
                        output[j++] = output[windowCopyFrom++];
                    }
                }
                else if (c >= 0x09)
                {
                    output[j++] = (byte)c;
                }
                else if (c >= 0x01)
                {
                    c = Math.Min(c, output.Length - j);
                    while (c-- > 0)
                    {
                        output[j++] = buffer[i++];
                    }
                }
                else
                {
                    output[j++] = (byte)c;
                }
            }
            return output.Decode();
        }

        private static int decompressedLength(byte[] buffer, int compressedLen)
        {
            int i = 0;
            int len = 0;

            while (i < compressedLen)
            {
                int c = buffer[i++] & 0x00ff;
                if (c >= 0x00c0)
                {
                    len += 2;
                }
                else if (c >= 0x0080)
                {
                    c = (c << 8) | (buffer[i++] & 0x00FF);
                    len += 3 + (c & 0x0007);
                }
                else if (c >= 0x0009)
                {
                    len++;
                }
                else if (c >= 0x0001)
                {
                    len += c;
                    i += c;
                }
                else
                {
                    len++;
                }
            }
            return len;
        }
    }

    class Metadata
    {
        private static CultureInfo culture = new CultureInfo("en-US");

        private static string[] dateFormats = new string[]{ "yyyy", "yyyy-MM", "yyyy-MM-dd" };

        /// <summary>
        /// Reorders author name for standard lastname-first sorting ie "Charles Dickens" becomes "Dickens, Charles"
        /// </summary>
        public static string SortAuthor(string author)
        {
            string[] splt = author.Split(' ');
            if (splt.Length == 1) return author;

            return splt[splt.Length - 1] + ", " + string.Join(" ", splt.SubArray(0, splt.Length - 1));
        }

        /// <summary>
        /// Converts date strings into epub standard yyyy-MM-dd (1950-01-01)
        /// </summary>
        public static string GetDate(string date)
        {
            return DateTime.ParseExact(date.Truncate(10), dateFormats, culture, DateTimeStyles.None).ToString("yyyy-MM-dd");
        }
    }

    class BitConverter
    {
        private static bool SwapEndian = false;
        private static bool _LittleEndian = System.BitConverter.IsLittleEndian;
        public static bool LittleEndian {
            get => _LittleEndian;
            set
            {
                _LittleEndian = value;
                if (value != System.BitConverter.IsLittleEndian)
                {
                    SwapEndian = true;
                }
            }
        }

        #region From Bytes
        public static short ToInt16(byte[] buffer, int offset)
        {
            if (!SwapEndian) return System.BitConverter.ToInt16(buffer, offset);

            byte[] slice = buffer.SubArray(offset, 0x2);
            Array.Reverse(slice);
            return System.BitConverter.ToInt16(slice, 0x0);
        }

        public static ushort ToUInt16(byte[] buffer, int offset)
        {
            if (!SwapEndian) return System.BitConverter.ToUInt16(buffer, offset);

            byte[] slice = buffer.SubArray(offset, 0x2);
            Array.Reverse(slice);
            return System.BitConverter.ToUInt16(slice, 0x0);
        }

        public static uint ToUInt32(byte[] buffer, int offset)
        {
            if (!SwapEndian) return System.BitConverter.ToUInt32(buffer, offset);

            byte[] slice = buffer.SubArray(offset, 0x4);
            Array.Reverse(slice);
            return System.BitConverter.ToUInt32(slice, 0x0);
        }

        public static int ToInt32(byte[] buffer, int offset)
        {
            if (!SwapEndian) return System.BitConverter.ToInt32(buffer, offset);

            byte[] slice = buffer.SubArray(offset, 0x4);
            Array.Reverse(slice);
            return System.BitConverter.ToInt32(slice, 0x0);
        }
        #endregion

        #region GetBytes
        //Why can't GetBytes be generic....
        public static byte[] GetBytes(short val)
        {
            byte[] output = System.BitConverter.GetBytes(val);
            if (SwapEndian) { Array.Reverse(output); }
            return output;
        }

        public static byte[] GetBytes(ushort val)
        {
            byte[] output = System.BitConverter.GetBytes(val);
            if (SwapEndian) { Array.Reverse(output); }
            return output;
        }

        public static byte[] GetBytes(int val)
        {
            byte[] output = System.BitConverter.GetBytes(val);
            if (SwapEndian) { Array.Reverse(output); }
            return output;
        }

        public static byte[] GetBytes(uint val)
        {
            byte[] output = System.BitConverter.GetBytes(val);
            if (SwapEndian) { Array.Reverse(output); }
            return output;
        }
        #endregion
    }
}
