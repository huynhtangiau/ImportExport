using DocumentFormat.OpenXml.Drawing;
using ImportExport.Core.CrossCutting.Settings;
using ImportExport.Core.Models;
using ImportExport.Core.Models.Validation;
using ImportExport.CrossCutting.Utils.Helpers;
using ImportExport.Service.Interfaces;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImportExport.Service.Services
{
    public class ValidationService: BaseService, IValidationService
    {
        private readonly IOptions<GrossWeightSettings> _grossWeightSettings;
        private readonly ColumnIndexGrossWeightSettings columnIndex;
        public ValidationService(IOptions<GrossWeightSettings> grossWeightSettings) {
            _grossWeightSettings = grossWeightSettings;
            columnIndex = grossWeightSettings.Value.ColumnIndex;
        }

        public IEnumerable<GrossWeightModel> GetMasterData(string sourceFile)
        {
            var grossWeights = new List<GrossWeightModel>();
            using (var package = new ExcelPackage(sourceFile))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.End.Row+1;
                for (var i = 3; i < rowCount; i++)
                {
                    var grossWeight = TransformItem(worksheet, i);
                    grossWeights.Add(grossWeight);
                }
            }
            return grossWeights;
        }
        private GrossWeightModel TransformItem(ExcelWorksheet worksheet, int rowIndex)
        {
            var grossWeight = new GrossWeightModel
            {
                SAPCode = worksheet.Cells[rowIndex, columnIndex.SAPCode].Text,
                Gross = worksheet.Cells[rowIndex, columnIndex.Gross].Text,
                ProductName = worksheet.Cells[rowIndex, columnIndex.Name].Text
            };
            return grossWeight;
        }
        private bool ValidateGross(FileModel pdfFile, IEnumerable<GrossWeightModel> grossWeights, string outputPath)
        {
            var pdfDocument = pdfFile.FileStream.GetPdfDocument();
            var locationByKeys = new Dictionary<string, List<LocationModel>>();
            foreach (var grossWeightModel in grossWeights)
            {
                var search = $@"{grossWeightModel.Gross}[\s]+KGS";
                var locations = pdfDocument.FindLocations(search, grossWeightModel.Gross, grossWeightModel.ProductName);
                locationByKeys.Add(locations.Key,locations.Value);
            }
            var newFolderName = $"GrossWeight_{DateTime.Now.ToString("yyyyMMdd")}";
            var newFolder = System.IO.Path.Combine(outputPath, newFolderName);
            if (!Directory.Exists(newFolder))
            {
                Directory.CreateDirectory(newFolder);
            }
            pdfDocument.WriteTextsByLocationToPdf(System.IO.Path.Combine(newFolder, $"{pdfFile.FileName}"), locationByKeys);

            File.WriteAllBytes(System.IO.Path.Combine(outputPath, $"{newFolderName}.zip"), newFolder.GetFiles().Compress());
            return true;
        }
        public bool Validate(IEnumerable<GrossWeightModel> grossWeights, string inputPath, string outputPath)
        {
            if(!grossWeights.Any())
            {
                return true;
            }
            var pdfFiles = inputPath.GetFiles();
            foreach(var file in pdfFiles)
            {
                ValidateGross(file, grossWeights, outputPath);
            }
            return true;
        }
    }
}
