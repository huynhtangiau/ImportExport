using ImportExpore.API.Helpers;
using ImportExpore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImportExpore.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LicenseController : ControllerBase
    {
        

        private readonly ILogger<LicenseController> _logger;

        public LicenseController(ILogger<LicenseController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult ExportProductsIntoFolder(string productsExcelPath = @"C:\Users\giau.huynh.STS\Giau\Support\LICENSE\Products.xlsx",
            string sourceFolder = @"C:\Users\giau.huynh.STS\Giau\Support\LICENSE\Source",
            string outputFolder = @"C:\Users\giau.huynh.STS\Giau\Support\LICENSE\Test")
        {
            var productLicenses = ReadData(productsExcelPath);
            ExportIntoFolder(productLicenses, sourceFolder, outputFolder);
            return Ok();
        }
        private ProductLicenseModel TransformItem(ExcelWorksheet worksheet, int rowIndex)
        {
            var productLicense = new ProductLicenseModel();
            productLicense.ProductNo = worksheet.Cells[rowIndex, 2].Text;
            var items = worksheet.Cells[rowIndex, 7].Text;
            if (!string.IsNullOrWhiteSpace(items))
            {
                productLicense.Items = items.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Select(s => new ProductModel { ProductName = s.Trim() })
                    .ToList();
                var pattern = @"[0-9]+[\s]*ML.+";
                var pcsPattern = @"SET.+[0-9]+PCS.+";
             //   var specialPattern = @"[\+\/\*\)\(].+";
                foreach (var product in productLicense.Items) {
                    product.ProductName = Regex.Replace(product.ProductName, pattern, string.Empty)
                        .Trim();
                    product.ProductName = Regex.Replace(product.ProductName, pcsPattern, string.Empty).Trim();
                 //   product.ProductName = Regex.Replace(product.ProductName, specialPattern, string.Empty);
                    product.ProductName = product.ProductName.Replace(":", string.Empty)
                        .Replace("(SAMPLE)", string.Empty);
                }
            }
            return productLicense;
        }
        private List<ProductLicenseModel> ReadData(string productsExcelPath)
        {
            var productLicenses = new List<ProductLicenseModel>();
            using (var package = new ExcelPackage(productsExcelPath))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.End.Row;
                for(var i = 2; i < rowCount; i++)
                {
                    var productLicense = TransformItem(worksheet, i);
                    productLicenses.Add(productLicense);
                }
                
            }
            return productLicenses;
        }
        private void FindByProductName(ProductLicenseModel productLicense, List<FileModel> files, string outputFolder)
        {
            if(productLicense.Items.Count == 0)
            {
                return;
            }
            foreach(var product in productLicense.Items)
            {
                var fileSearchPaths = files.Where(q => 
                            q.FileName.ToLower().Contains(product.ProductName.ToLower())
                            ).ToList();
                if(fileSearchPaths.Count > 0)
                {
                    var productLicensePath = $@"{outputFolder}\{productLicense.ProductNo}";
                    var index = 1;
                    foreach(var file in fileSearchPaths)
                    {
                        var pcbPDFContent = file.Path.ReadPdfContent();
                            System.IO.File.Copy(file.Path, @$"{productLicensePath}\{productLicense.ProductNo}_{index}.pdf", true);
                        index++;
                    }
                    
                }
            }
        }
        private List<FileModel> GetFiles(string sourceFolderPath)
        {
            var files = Directory.GetFiles(sourceFolderPath, "*.pdf", SearchOption.AllDirectories);
            return files.Select(s => new FileModel() { 
                    FileName = Path.GetFileNameWithoutExtension(s), 
                    Path = s, 
                    Extension = Path.GetExtension(s)})
                .ToList();
        }
        private void ExportIntoFolder(List<ProductLicenseModel> productLicenses, string sourceFolderPath, string outputFolder)
        {
            var files = GetFiles(sourceFolderPath);
            foreach (var productLicense in productLicenses)
            {
                var productLicensePath = $@"{outputFolder}\{productLicense.ProductNo}";
                if (!Directory.Exists(productLicensePath))
                {
                    Directory.CreateDirectory(productLicensePath);
                }
                FindByProductName(productLicense, files, outputFolder);
            }
        }
    }
}
