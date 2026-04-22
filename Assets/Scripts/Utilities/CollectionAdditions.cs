using System.Collections.Generic;
using System;
using System.Linq;

namespace ITF.Utilities
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns a random element of the collection based on given weights
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="weightSelector"></param>
        /// <returns></returns>
        public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector)
        {
            float totalWeight = sequence.Sum(weightSelector);
            // The weight we are after...
            float itemWeightIndex = (float)new Random().NextDouble() * totalWeight;
            float currentWeightIndex = 0;

            foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) })
            {
                currentWeightIndex += item.Weight;

                // If we've hit or passed the weight we are after for this item then it's the one we want....
                if (currentWeightIndex >= itemWeightIndex)
                    return item.Value;

            }

            return default(T);

        }

        public static IEnumerable<T> ExceptWhere<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            return source.Where(x => !predicate(x));
        }

        public static IEnumerable<T> RemoveAllNull<T>(this IEnumerable<T> source)
        {
            return source.ExceptWhere(x => x == null);
        }

        public static IEnumerable<T> AddIfMissing<T>(this IEnumerable<T> source, T element)
        {
            return source.Contains(element) ? source : source.Append(element);
        }
    }
}