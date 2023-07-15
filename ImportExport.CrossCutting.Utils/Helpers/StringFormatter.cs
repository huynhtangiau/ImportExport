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
    }
}
