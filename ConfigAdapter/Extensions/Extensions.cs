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
