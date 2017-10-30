using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlBase.ArrayExtensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Adds an array element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns></returns>
        public static T[] AddArrayElement<T>(this T[] array, T newValue)
        {
            List<T> list = array.ToList<T>();
            list.Add(newValue);
            return list.ToArray<T>();
        }

        /// <summary>
        /// Inserts an array element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static T[] InsertArrayElement<T>(this T[] array, T newValue, int index)
        {
            List<T> list = array.ToList<T>();
            list.Insert(index, newValue);
            return list.ToArray<T>();
        }

        /// <summary>
        /// Removes an array element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static T[] RemoveArrayElement<T>(this T[] array, int index)
        {
            List<T> list = array.ToList<T>();
            list.RemoveAt(index);
            return list.ToArray<T>();
        }

        /// <summary>
        /// Cuts an array in two parts.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <param name="cutAtIndex">Index of the cut.</param>
        /// <param name="array1">The array1.</param>
        /// <param name="array2">The array2.</param>
        public static void CutArray<T>(this T[] array, int cutAtIndex, out T[] array1, out T[] array2)
        {
            List<T> list = array.ToList<T>();
            array1 = list.GetRange(0, cutAtIndex).ToArray<T>();
            array2 = list.GetRange(cutAtIndex, list.Count() - cutAtIndex).ToArray<T>();
        }
    }
}
