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
        public ActionResult Test(string path = @"/Users/giauht/Downloads/123.pdf")
        {
            path.AddTextToPdf(path,"1", new System.Drawing.Point(15,450));
            return Ok();
        }
        [HttpGet("Compress")]
        public ActionResult Compress(string path = @"C:\Users\giau.huynh.STS\Downloads\HS hoàn thuế 730305688120. Signed.pdf")
        {
            path.CompressPDF(@"C:\Users\giau.huynh.STS\Downloads\test.pdf");
            return Ok();
        }
    }
}
