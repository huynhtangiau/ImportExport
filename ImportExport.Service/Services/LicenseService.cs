using DocumentFormat.OpenXml.Spreadsheet;
using ImportExport.Core.CrossCutting.Settings;
using ImportExport.Core.Models;
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
    public class LicenseService : BaseService, ILicenseService
    {
        private readonly IOptions<LicenseSettings> _licenseSettings;
        private readonly ColumnIndexSettings columnIndex;
        public LicenseService(IOptions<LicenseSettings> licenseSettings)
        {
            _licenseSettings = licenseSettings;
            columnIndex = _licenseSettings.Value.ColumnIndex;
        }
        private List<ProductModel> RemoveSetPCS(string products)
        {
            var pcsPatterns = new string[]
            {
                @"SET.*[0-9\s]+PCS.*",
                @".*[0-9\s]+PCS.*",
                @"set.*[0-9\s]+pcs.*",
                @".*[0-9\s]+pcs.*"
            };
            return products.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Where(q => !pcsPatterns.Any(a => Regex.IsMatch(q, a)) && !string.IsNullOrWhiteSpace(q))
                    .Select(s => new ProductModel { ProductName = s.Trim(), RawName = s.Trim() })
                    .ToList();
        }
        private string RemoveInvalidCharacters(string productName)
        {
            var removePatterns = new string[]
            {
                @"[0-9]+[\s]*ML.*",
                @"NO\.[0-9]+.*",
                @"\(.*\)$"
            };
            foreach(var pattern in removePatterns)
            {
                productName = Regex.Replace(productName, pattern, string.Empty);
            }
            var replacers = new string[]
            {
                "(SAMPLE)",
                @"/"
            };
            foreach(var pattern in replacers)
            {
                productName = productName.Replace(pattern, string.Empty);
            }
            return productName.Trim();
        }
        private  ProductLicenseModel TransformItem(ExcelWorksheet worksheet, int rowIndex)
        {
            var productLicense = new ProductLicenseModel();
            productLicense.ProductNo = worksheet.Cells[rowIndex, columnIndex.ProductNo].Text;
            var items = worksheet.Cells[rowIndex, columnIndex.Product].Text;
            if (!string.IsNullOrWhiteSpace(items))
            {
                productLicense.Items = RemoveSetPCS(items);

                var licenseDates = worksheet.Cells[rowIndex, columnIndex.LicenseDate].Text.Replace("NGÀY", string.Empty, StringComparison.CurrentCultureIgnoreCase)
                    .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Where(q => !string.IsNullOrWhiteSpace(q))
                    .ToArray();

                var licenseNos = worksheet.Cells[rowIndex, columnIndex.LicenseNo].Text
                    .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Where(q => !string.IsNullOrWhiteSpace(q))
                    .ToArray();

                var index = 0;
                foreach (var product in productLicense.Items)
                {
                    product.ProductName = RemoveInvalidCharacters(product.ProductName);
                    product.ProductNameV1 = product.ProductName.Replace(":", " ");
                    product.ProductName = product.ProductName.Replace(":", string.Empty);
                    var productItems = productLicense.Items
                        .Where(q => !string.IsNullOrEmpty(q.LicenseNo))
                        .ToList();
                    if (productItems.Count <= licenseNos.Length)
                    {
                        product.LicenseNo = licenseNos[index].Trim();
                        product.LicenseDate = licenseDates[index].Trim().ToDateFull();
                        index++;
                    }
                    product.ProductName = product.ProductName.Trim();
                    product.ProductNameV1 = product.ProductNameV1.Trim();
                }
            }
            return productLicense;
        }
        public List<ProductLicenseModel> ReadData(string productsExcelPath)
        {
            var productLicenses = new List<ProductLicenseModel>();
            using (var package = new ExcelPackage(productsExcelPath))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.End.Row;
                for (var i = 2; i < rowCount; i++)
                {
                    var productLicense = TransformItem(worksheet, i);
                    productLicenses.Add(productLicense);
                }

            }
            return productLicenses;
        }
        private SearchFileModel FindByLicenseNoAndDate(ProductModel product, List<FileModel> searchFiles)
        {
            for(var i = 0; i < searchFiles.Count; i++)
            {
                var file = searchFiles[i];
                if (string.IsNullOrEmpty(product.LicenseNo))
                {
                    return new SearchFileModel
                    {
                        ResultKey = SearchResultKey.FirstItem,
                        File = searchFiles.FirstOrDefault()
                    };
                }
                var signatureContent = file.Path.ReadSignatureContent();
                var signatureArrays = signatureContent.Split(
                    new string[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );
                if(signatureArrays.Length < 3)
                {
                    continue;
                }
                var pdfLicenseNo = signatureArrays[3].Trim().ToLower();
                var pdfLicenseDate = signatureArrays[1].Replace("NGÀY", string.Empty, StringComparison.CurrentCultureIgnoreCase).Trim().ToDateFull();
                if(product.LicenseNo.ToLower() == pdfLicenseNo && product.LicenseDate == pdfLicenseDate)
                {
                    return new SearchFileModel { 
                        ResultKey = SearchResultKey.Found,
                        File = file
                    };
                }
            }
            return new SearchFileModel
            {
                ResultKey = SearchResultKey.FirstItem,
                File = searchFiles.FirstOrDefault()
            };
        }
        private void FindByProductName(ProductLicenseModel productLicense, List<FileModel> files, string outputFolder)
        {
            if (productLicense.Items.Count == 0)
            {
                return;
            }
            var index = 1;
            var newPCBFolder = Path.Combine(outputFolder, $"PCB_{DateTime.Now.ToString("yyyyMMdd")}");
            if (!Directory.Exists(newPCBFolder)) {
                Directory.CreateDirectory(newPCBFolder);
            }

            foreach (var product in productLicense.Items)
            {
                var fileSearchPaths = files.Where(q => q.FileName.ToLower().Contains(product.ProductName.ToLower())
                    || (!string.IsNullOrEmpty(product.ProductNameV1) && q.FileName.ToLower().Contains(product.ProductNameV1.ToLower())))
                    .OrderByDescending(o => o.CreatedDate)
                    .ToList();
                if (fileSearchPaths.Count > 0)
                {
                    var productLicensePath = newPCBFolder;// Path.Combine(outputFolder, productLicense.ProductNo);
                    var search = FindByLicenseNoAndDate(product, fileSearchPaths);
                    if(search.File == null)
                    {
                        continue;
                    }
                    var newFileName = string.Empty;
                    if(search.ResultKey == SearchResultKey.FirstItem)
                    {
                        newFileName = $"{productLicense.ProductNo}_backup_{index}.pdf";
                    }
                    else if (search.ResultKey == SearchResultKey.Found)
                    {
                        newFileName = $"{productLicense.ProductNo}_{index}.pdf";
                    }
                    search.File.Path.AddTextToPdf(Path.Combine(productLicensePath, newFileName), productLicense.ProductNo,
                        500);

                    index++;

                }
            }
        }
        private void CombineIntoOne(string outputFolder)
        {
            var files = outputFolder.GetFiles();
            var filePaths = files
                .OrderBy(o => o.CreatedDate)
                .Select(s => s.Path)
                .ToArray();
            if(filePaths.Length > 0)
            {
                filePaths.MergePDFs($"{outputFolder}/AllInOne_{DateTime.Now.ToString("ddMMyyyy")}.pdf");
            }
        }
        public bool ExportIntoFolder(List<ProductLicenseModel> productLicenses, string sourceFolderPath, string outputFolder)
        {
            outputFolder.DeleteFiles();
            var files = sourceFolderPath.GetFiles();
            foreach (var productLicense in productLicenses)
            {
                FindByProductName(productLicense, files, outputFolder);
            }
            outputFolder.Compress($"PCB_{DateTime.Now.ToString("yyyyMMdd")}.zip", "*.pdf");
            return true;
        }
    }
}
