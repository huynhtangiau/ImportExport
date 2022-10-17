using ImportExport.CrossCutting.Utils.Helpers;
using ImportExport.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ImportExport.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LicenseController : ControllerBase
    {
        private readonly ILogger<LicenseController> _logger;
        private readonly ILicenseService _licenseService;
        public LicenseController(ILogger<LicenseController> logger,
            ILicenseService licenseService)
        {
            _logger = logger;
            _licenseService = licenseService;
        }

        [HttpPost]
        public ActionResult ExportProductsIntoFolder(string productsExcelPath = @"C:\Users\giau.huynh.STS\Giau\Support\LICENSE\Products.xlsx",
            string sourceFolder = @"C:\Users\giau.huynh.STS\Giau\Support\LICENSE\Source",
            string outputFolder = @"C:\Users\giau.huynh.STS\Giau\Support\LICENSE\Test")
        {
            var productLicenses = _licenseService.ReadData(productsExcelPath);
            _licenseService.ExportIntoFolder(productLicenses, sourceFolder, outputFolder);
            return Ok();
        }
        [HttpGet]
        public ActionResult Test(string path = @"C:\Users\giau.huynh.STS\Giau\Support\LICENSE\Test\1\1_1.pdf")
        {
            path.ReadSignatureContent();
            return Ok();
        }
    }
}
