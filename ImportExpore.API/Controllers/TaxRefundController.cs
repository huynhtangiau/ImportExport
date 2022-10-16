using ImportExport.API.Helpers;
using ImportExport.CrossCutting.Utils.Helpers;
using ImportExport.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace ImportExport.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaxRefundController : ControllerBase
    {
        private readonly IRefundService _refundService;
        public TaxRefundController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        [HttpPost]
        public async Task<ActionResult> DeclareTaxRefund(
            string rootFolder = @"C:\Users\giau.huynh.STS\Giau\Support\TaxRefund",
            string refundId = "104612655450",
            string refundExcelFile = @"ToKhaiHQ7N_QDTQ_104612655450.xlsx", 
            string amaExcelFile = @"ToKhaiAMA_104612655450_2.xlsx",
            string outputFolder = @"C:\Users\giau.huynh.STS\Giau\Support\TaxRefund",
            string taxGovPDFFile = @"GNTThue (33).pdf")
        {
            rootFolder = Path.Combine(rootFolder, refundId);
            var taxGovPDFFileFullPath = Path.Combine(rootFolder,taxGovPDFFile);

            rootFolder.ConvertXLSX();

            var taxDeclaration = await _refundService.ReadData(Path.Combine(rootFolder, refundExcelFile),
                Path.Combine(rootFolder, amaExcelFile));

            var pdfContent =  taxGovPDFFileFullPath.ReadPdfContent();
            _refundService.TranformData(taxDeclaration, pdfContent);

            _refundService.ExportData(taxDeclaration, outputFolder);
            return Ok();
        }
        
        
    }
}
