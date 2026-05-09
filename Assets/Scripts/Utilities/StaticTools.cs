using System.Collections.Generic;

namespace ITF.Utilities
{
    /// <summary>
    /// Some static functions
    /// </summary>
    public class StaticTools
    {

        /// <summary>
        /// Using binary search to find the insertion index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="e">element to be inserted</param>
        /// <param name="order">order or reverse order</param>
        /// <param name="start">start index</param>
        /// <param name="end">end index, -1 represents the array length</param>
        /// <returns>returns the index where the element should be inserted, -1 indicates search failure</returns>
        public static int BinaryIndex<T>(List<T> array, T e, bool order = true, int start = 0, int end = -1) where T : IComparer<T>
        {
            if (end < 0) end = array.Count;

            if (end == start) return start;
            else if (end < start) return -1;

            int index = (start + end) / 2;

            if (e.Compare(e, array[index]) > 0)
            {
                if (order) start = index + 1;
                else end = index;
            }
            else
            {
                if (order) end = index;
                else start = index + 1;
            }

            return BinaryIndex(array, e, order, start, end);
        }

    }

}