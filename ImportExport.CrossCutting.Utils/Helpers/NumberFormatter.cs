using Humanizer;
using System;
using System.Globalization;

namespace ImportExport.CrossCutting.Utils.Helpers
{
    public static class NumberFormatter
    {
        public static string ToComma(this string numStr)
        {
            var number = int.Parse(numStr.Replace(".", string.Empty));
            return string.Format("{0:n0}", number);
        }
        public static int ToInt(this string numStr)
        {
            return int.Parse(numStr.Replace(".", string.Empty).Replace(",", string.Empty));
        }
        public static string ToComma(this int num)
        {
            return string.Format("{0:n0}", num);
        }
        public static string ToVietnameseText(this int inputNumber, bool suffix = true)
        {
            var additionalCurrency = suffix ? " đồng" : string.Empty;
            var cul = CultureInfo.GetCultureInfo("vi-VN");
            return $"{inputNumber.ToWords(cul)} {additionalCurrency}";
        }

    }
}
