using ImportExport.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExport.Service.Interfaces
{
    public interface ILicenseService
    {
        List<ProductLicenseModel> ReadData(string productsExcelPath);
        bool ExportIntoFolder(List<ProductLicenseModel> productLicenses, string sourceFolderPath, string outputFolder);
    }
}
