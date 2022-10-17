using ImportExport.Core.Models;
using ImportExport.CrossCutting.Utils.Helpers;
using ImportExport.Service.Interfaces;
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
    public class LicenseService: BaseService, ILicenseService
    {
        public LicenseService()
        {

        }
        private string ReplaceManual(string productName)
        {
            return productName.Replace("(SAMPLE)", string.Empty)
                        .Replace(@"/", string.Empty);
        }
        private ProductLicenseModel TransformItem(ExcelWorksheet worksheet, int rowIndex)
        {
            var productLicense = new ProductLicenseModel();
            productLicense.ProductNo = worksheet.Cells[rowIndex, 2].Text;
            var items = worksheet.Cells[rowIndex, 7].Text;
            if (!string.IsNullOrWhiteSpace(items))
            {
                productLicense.Items = items.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Select(s => new ProductModel { ProductName = s.Trim(), RawName = s.Trim() })
                    .ToList();
                var pattern = @"[0-9]+[\s]*ML.*";
                var pcsPattern = @"SET.*[0-9]+PCS.*";
                var noPattern = @"NO\.[0-9]+.*";
                foreach (var product in productLicense.Items)
                {
                    if (Regex.IsMatch(product.ProductName, pcsPattern))
                    {
                        product.ProductNameV1 = string.Empty;
                        continue;
                    }
                    product.ProductName = Regex.Replace(product.ProductName, pattern, string.Empty)
                        .Trim();
                    product.ProductName = Regex.Replace(product.ProductName, noPattern, string.Empty).Trim();
                    product.ProductNameV1 = product.ProductName.Replace(":", " ");
                    product.ProductName = product.ProductName.Replace(":", string.Empty);
                    product.ProductNameV1 = ReplaceManual(product.ProductNameV1);
                    product.ProductName = ReplaceManual(product.ProductName);
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
        private void FindByProductName(ProductLicenseModel productLicense, List<FileModel> files, string outputFolder)
        {
            if (productLicense.Items.Count == 0)
            {
                return;
            }
            var index = 1;
            foreach (var product in productLicense.Items)
            {
                var fileSearchPaths = files.Where(q => q.FileName.ToLower().Contains(product.ProductName.ToLower())
                    || (!string.IsNullOrEmpty(product.ProductNameV1) && q.FileName.ToLower().Contains(product.ProductNameV1.ToLower())))
                    .OrderByDescending(o => o.CreatedDate)
                    .ToList();
                if (fileSearchPaths.Count > 0)
                {
                    var productLicensePath = Path.Combine(outputFolder, productLicense.ProductNo);
                    var file = fileSearchPaths.FirstOrDefault();
                    var pcbPDFContent = file.Path.ReadPdfContent();
                    System.IO.File.Copy(file.Path, Path.Combine(productLicensePath, $"{productLicense.ProductNo}_{index}.pdf"), true);

                    index++;

                }
            }
        }
        private List<FileModel> GetFiles(string sourceFolderPath)
        {
            var dir = new DirectoryInfo(sourceFolderPath);
            var files = dir.GetFiles("*.pdf", SearchOption.AllDirectories);
            return files.Select(s => new FileModel()
            {
                FileName = s.Name,
                Path = s.FullName,
                Extension = s.Extension,
                CreatedDate = s.CreationTime
            }).ToList();
        }
        public void ExportIntoFolder(List<ProductLicenseModel> productLicenses, string sourceFolderPath, string outputFolder)
        {
            var files = GetFiles(sourceFolderPath);
            foreach (var productLicense in productLicenses)
            {
                var productLicensePath = Path.Combine(outputFolder, productLicense.ProductNo);
                if (!Directory.Exists(productLicensePath))
                {
                    Directory.CreateDirectory(productLicensePath);
                }
                FindByProductName(productLicense, files, outputFolder);
            }
        }
    }
}
