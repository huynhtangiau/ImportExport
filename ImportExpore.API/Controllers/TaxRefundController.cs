using ImportExport.CrossCutting.Utils.Helpers;
using ImportExport.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
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

        [HttpGet("TaxRefunds")]
        public async Task<ActionResult> DeclareTaxRefunds(
            string rootFolder = @"C:\Users\giau.huynh.STS\Giau\Support\TaxRefund",
            string outputFolder = @"C:\Users\giau.huynh.STS\Giau\Support\TaxRefund")
        {
            var refundIds = Directory.GetDirectories(rootFolder);
            foreach (var refundId in refundIds)
            {
                rootFolder = Path.Combine(rootFolder, refundId);

                rootFolder.ConvertXLSX();
                var refundExcelFile = Directory.GetFiles(rootFolder, "ToKhaiHQ7N_QDTQ*.xlsx", SearchOption.TopDirectoryOnly);
                var amaExcelFile = Directory.GetFiles(rootFolder, "ToKhaiAMA_*.xlsx", SearchOption.TopDirectoryOnly);
                var taxGovPDFFile = Directory.GetFiles(rootFolder, "GNT*.pdf", SearchOption.TopDirectoryOnly);
                if (taxGovPDFFile.Length == 0 || amaExcelFile.Length == 0 || refundExcelFile.Length == 0)
                {
                    continue;
                }

                var taxDeclaration = await _refundService.ReadData(Path.Combine(rootFolder, refundExcelFile[0]),
                    Path.Combine(rootFolder, amaExcelFile[0]));

                var taxGovPDFFileFullPath = Path.Combine(rootFolder, taxGovPDFFile[0]);
                var pdfContent = taxGovPDFFileFullPath.ReadPdfContent();
                _refundService.TranformData(taxDeclaration, pdfContent);

                _refundService.ExportData(taxDeclaration, outputFolder);
                outputFolder.Compress($"TaxRefund_{DateTime.Now.ToString("yyyyMMdd")}.zip");
            }
            return Ok();
        }
    }
}
