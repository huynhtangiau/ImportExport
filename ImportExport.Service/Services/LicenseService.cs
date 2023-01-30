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
            productName = Regex.Replace(productName, @"\(.*\)$", string.Empty);
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
                var pcsPattern = @"SET.*[0-9\s]+PCS.*";
                productLicense.Items = items.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Where(q => !Regex.IsMatch(q, pcsPattern) && !string.IsNullOrWhiteSpace(q))
                    .Select(s => new ProductModel { ProductName = s.Trim(), RawName = s.Trim() })
                    .ToList();
                var pattern = @"[0-9]+[\s]*ML.*";
                var noPattern = @"NO\.[0-9]+.*";

                var licenseDates = worksheet.Cells[rowIndex, 4].Text.Replace("NGÀY", string.Empty)
                    .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Where(q => !string.IsNullOrWhiteSpace(q))
                    .ToArray();

                var licenseNos = worksheet.Cells[rowIndex, 3].Text
                    .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Where(q => !string.IsNullOrWhiteSpace(q))
                    .ToArray();

                var index = 0;
                foreach (var product in productLicense.Items)
                {
                    product.ProductName = Regex.Replace(product.ProductName, pattern, string.Empty)
                        .Trim();
                    product.ProductName = Regex.Replace(product.ProductName, noPattern, string.Empty).Trim();
                    product.ProductNameV1 = product.ProductName.Replace(":", " ");
                    product.ProductName = product.ProductName.Replace(":", string.Empty);
                    product.ProductNameV1 = ReplaceManual(product.ProductNameV1);
                    product.ProductName = ReplaceManual(product.ProductName);
                    if (productLicense.Items.Count <= licenseNos.Length + 1)
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
                var pdfLicenseDate = signatureArrays[1].Replace("NGÀY", string.Empty).Trim().ToDateFull();
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
            foreach (var product in productLicense.Items)
            {
                var fileSearchPaths = files.Where(q => q.FileName.ToLower().Contains(product.ProductName.ToLower())
                    || (!string.IsNullOrEmpty(product.ProductNameV1) && q.FileName.ToLower().Contains(product.ProductNameV1.ToLower())))
                    .OrderByDescending(o => o.CreatedDate)
                    .ToList();
                if (fileSearchPaths.Count > 0)
                {
                    var productLicensePath = outputFolder;// Path.Combine(outputFolder, productLicense.ProductNo);
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
                        new System.Drawing.Point(450,25));

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
        private void CombineIntoOne(string outputFolder)
        {
            var files = GetFiles(outputFolder);
            var filePaths = files
                .OrderBy(o => o.CreatedDate)
                .Select(s => s.Path)
                .ToArray();
            if(filePaths.Length > 0)
            {
                filePaths.MergePDFs($"{outputFolder}/AllInOne_{DateTime.Now.ToString("ddMMyyyy")}.pdf");
            }
        }
        public void ExportIntoFolder(List<ProductLicenseModel> productLicenses, string sourceFolderPath, string outputFolder)
        {
            var files = GetFiles(sourceFolderPath);
            foreach (var productLicense in productLicenses)
            {
                //var productLicensePath = Path.Combine(outputFolder, productLicense.ProductNo);
                //if (!Directory.Exists(productLicensePath))
                //{
                //    Directory.CreateDirectory(productLicensePath);
                //}
                FindByProductName(productLicense, files, outputFolder);
            }
            //CombineIntoOne(outputFolder);
        }
    }
}
