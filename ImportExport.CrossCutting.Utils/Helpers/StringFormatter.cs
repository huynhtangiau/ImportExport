using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExport.CrossCutting.Utils.Helpers
{
    public static class StringFormatter
    {
        public static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };
        public static string ToDate(this string input) =>
            DateTime.ParseExact(input, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
        public static string ToDate(this string input, string[] formats) =>
            DateTime.ParseExact(input, formats, CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
        public static string ToDateFull(this string input) =>
            input.Trim() == "0" || string.IsNullOrEmpty(input)? string.Empty
            :DateTime.ParseExact(input, new string[] { "dd/MM/yyyy HH:mm:ss", "dd/M/yyyy", "d/M/yyyy", "M/d/yyyy" }, CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
    }
    public static class StringHelper
    {
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            var indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
        public static IEnumerable<string> Split(this string str, int chunkSize)
        {
            if(str.Length < chunkSize)
            {
                return new List<string>() { str};
            }
            var result = new List<string>();
            for (int i = 0; i < str.Length; i += chunkSize)
                result.Add(str.Substring(i, Math.Min(chunkSize, str.Length - i)));
            return result;
        }
        // Enumerate by nearest space
        // Split String value by closest to length spaces
        // e.g. for length = 3 
        // "abcd efghihjkl m n p qrstsf" -> "abcd", "efghihjkl", "m n", "p", "qrstsf" 
        public static IEnumerable<String> EnumByNearestSpace(this String value, int length)
        {
            if (String.IsNullOrEmpty(value))
                yield break;

            int bestDelta = int.MaxValue;
            int bestSplit = -1;

            int from = 0;

            for (int i = 0; i < value.Length; ++i)
            {
                var Ch = value[i];

                if (Ch != ' ')
                    continue;

                int size = (i - from);
                int delta = (size - length > 0) ? size - length : length - size;

                if ((bestSplit < 0) || (delta < bestDelta))
                {
                    bestSplit = i;
                    bestDelta = delta;
                }
                else
                {
                    yield return value.Substring(from, bestSplit - from);

                    i = bestSplit;

                    from = i + 1;
                    bestSplit = -1;
                    bestDelta = int.MaxValue;
                }
            }

            // String's tail
            if (from < value.Length)
            {
                if (bestSplit >= 0)
                {
                    if (bestDelta < value.Length - from)
                        yield return value.Substring(from, bestSplit - from);

                    from = bestSplit + 1;
                }

                if (from < value.Length)
                    yield return value.Substring(from);
            }
        }
    }
}
