using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigAdapter.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Executes an action for every element
        /// in the sequence.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="source">Sequence</param>
        /// <param name="f">Action</param>
        public static void ForEach<T> (this IEnumerable<T> source, Action<T> f)
        {
            foreach (var x in source)
                f(x);
        }

        /// <summary>
        /// Converts an individual element
        /// into a sequence of one element.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="source">Element</param>
        /// <returns>Sequence</returns>
        public static IEnumerable<T> Enumerate<T> (this T source)
        {
            yield return source;
        }

        /// <summary>
        /// Parses a string into a double using
        /// an invariant culture.
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Double</returns>
        public static double ToDouble(this string str) => double.Parse(str.Replace(",", "."), CultureInfo.InvariantCulture);

        /// <summary>
        /// Parses a string into a float using
        /// an invariant culture.
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Float</returns>
        public static float ToFloat(this string str) => float.Parse(str.Replace(",", "."), CultureInfo.InvariantCulture);
    }
}
