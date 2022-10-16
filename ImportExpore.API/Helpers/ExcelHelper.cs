using OfficeOpenXml;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImportExpore.API.Helpers
{
    public static class ExcelHelper
    {
        public static string GetMergedRangeAddress(this ExcelRange @this)
        {
            if (@this.Merge)
            {
                var idx = @this.Worksheet.GetMergeCellId(@this.Start.Row, @this.Start.Column);
                return @this.Worksheet.MergedCells[idx - 1]; //the array is 0-indexed but the mergeId is 1-indexed...
            }
            else
            {
                return @this.Address;
            }
        }
        public static string GetText(this ExcelWorksheet worksheet, ExcelRange range)
        {
            return worksheet.Cells[range.GetMergedRangeAddress()].Text;
        }
        public static void ConvertXLSX(this string filesFolder)
        {
            var files = Directory.GetFiles(filesFolder, "*.xls");

            foreach (var file in files)
            {
                var workbook = new Workbook();
                workbook.LoadFromFile(file);
                workbook.SaveToFile($"{Path.ChangeExtension(file, null)}.xlsx", ExcelVersion.Version2013);
            }
        }
    }
}
