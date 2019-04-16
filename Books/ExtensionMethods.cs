﻿using System;
using System.Text;
using System.Collections.Generic;

namespace ExtensionMethods
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Inserts data into array after specified index
        /// Returns an array of length (this.Length + insertable.Length)
        /// Example:
        ///     int[] arr = new int[] { 0, 1, 2, 3, 4, 5 };
        ///     arr = arr.InsertRange(3, new int[] { 100, 101, 102 });
        ///     arr == [0, 1, 2, 3, 100, 101, 102, 4, 5};
        /// </summary>
        /// <returns></returns>
        public static T[] InsertRange<T>(this T[] array, int index, T[] insertable)
        {
            T[] output = new T[array.Length + insertable.Length];
            int pos = 0;

            for (var i = 0; i <= index; i++)
            {
                output[pos] = array[i];
                pos++;
            }

            foreach(T x in insertable)
            {
                output[pos] = x;
                pos++;
            }

            for (var i = index + 1; i < array.Length; i++)
            {
                output[pos] = array[i];
                pos++;
            }

            return output;
        }

        public static T[] Append<T>(this T[] array, T[] appendable)
        {
            T[] output = new T[array.Length + appendable.Length];
            Array.Copy(array, output, array.Length);
            for (int i = 0; i < appendable.Length; i++)
            {
                output[i + array.Length] = appendable[i];
            }
            return output;
        }


        /// <summary>
        /// Returns a sub-array of length starting at index
        /// </summary>
        /// <returns> Array<T></returns>
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static void ReplaceAt<T>(this T[] data, T[] newData, int index)
        {
            for (var i = 0; i < newData.Length; i++)
            {
                data[index + i] = newData[i];
            }
        }

        /// <summary>
        /// Converts byte array into UTF-8 string
        /// </summary>
        public static string Decode(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Converts byte array into UTF-8 string
        /// </summary>
        public static string Decode(this byte[] data, string encoding)
        {
            Encoding enc = Encoding.GetEncoding(encoding);
            if (enc == null) throw new Exception($"Encoding {encoding} not found");
            return enc.GetString(data);
        }
    }

    public static class StringExtensions
    {
        /// <summary>
        /// Converts strings into byte array
        /// </summary>
        public static byte[] Encode(this string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        /// <summary>
        /// Truncate a string to the provided maximum length in characters.
        /// </summary>
        public static string Truncate(this string data, int maxLength)
        {
            if (data.Length <= maxLength) return data;
            return data.Substring(0, maxLength);
        }
    }

    public static class DictExtensions
    {
        public static v Get<k, v>(this IDictionary<k, v> dict, k key)
        {
            return dict.TryGetValue(key, out v val) ? val : default(v);

        }
    }

    public static class HashSetExtensions
    {
        public static T[] ToArray<T>(this HashSet<T> hs)
        {
            T[] arr = new T[hs.Count];
            hs.CopyTo(arr);
            return arr;
        }
    }

    public static class ByteExtensions
    {
        public static string Decode(this byte b)
        {
            return ((char)b).ToString();
        }
    }
}