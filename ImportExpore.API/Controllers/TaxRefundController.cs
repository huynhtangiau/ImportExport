using DocumentFormat.OpenXml.Packaging;
using ImportExpore.API.Helpers;
using ImportExpore.API.Models;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ImportExpore.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaxRefundController : ControllerBase
    {
        private RefundTaxDeclarationModel ReadData(string refundpath, string amaPath)
        {
            var refundTaxDeclaration = new RefundTaxDeclarationModel();
            using (var package = new ExcelPackage(refundpath))
            {
                var worksheet = package.Workbook.Worksheets[0];
                refundTaxDeclaration.RefundTaxId = worksheet.GetText(worksheet.Cells["E4"]);
                refundTaxDeclaration.RefundTaxDate = worksheet.GetText(worksheet.Cells["G83"]);
            }
            using (var package = new ExcelPackage(amaPath))
            {
                var worksheet = package.Workbook.Worksheets[0];
                refundTaxDeclaration.ImportAmount = worksheet.GetText(worksheet.Cells["P34"]);
                refundTaxDeclaration.VATAmount = worksheet.GetText(worksheet.Cells["P35"]);
            }
            return refundTaxDeclaration;
        }
        private void TranformData(ref RefundTaxDeclarationModel refundTaxDeclaration
            , string govTaxContent)
        {
            var pattern = @"((1901)|(1702))[\s]+[\d\,]+[\s]*VNĐ";
            var lstMatched = Regex.Matches(govTaxContent, pattern);
            var ImportAmounted = lstMatched[0].Value
                .Replace("1901", string.Empty)
                .Replace("VNĐ", string.Empty).Trim();
            var VATAmounted = lstMatched[1].Value
                .Replace("1702", string.Empty)
                .Replace("VNĐ", string.Empty).Trim();

            var date = DateTime.ParseExact(refundTaxDeclaration.RefundTaxDate, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
            refundTaxDeclaration.RefundTaxDate = date;
            var importAmount = refundTaxDeclaration.ImportAmount.ToInt();
            var vATAmount = refundTaxDeclaration.VATAmount.ToInt();
            var importAmounted = ImportAmounted.ToInt();
            var vATAmounted = VATAmounted.ToInt();
            refundTaxDeclaration.TotalAmount = (importAmount + vATAmount).ToComma();
            refundTaxDeclaration.ImportAmount = refundTaxDeclaration.ImportAmount.ToComma();
            refundTaxDeclaration.VATAmount = refundTaxDeclaration.VATAmount.ToComma();
            refundTaxDeclaration.ImportMustPayAmount = (importAmounted - importAmount ).ToComma();
            refundTaxDeclaration.VATMustPayAmount = (vATAmounted - vATAmount).ToComma();
            refundTaxDeclaration.ImportAmounted = importAmounted.ToComma();
            refundTaxDeclaration.VATAmounted = vATAmounted.ToComma();
        }
        private void ExportData(RefundTaxDeclarationModel taxDeclaration, string outputFolder)
        {
            var path = @"C:\Users\giau.huynh.STS\Giau\Support\ImportExpore.API\ImportExpore.API\Templates\Refund\TaxRefund.docx";
            var fileBytes = System.IO.File.ReadAllBytes(path);
            var memorystream = new MemoryStream(fileBytes);
            using (var wordDoc = WordprocessingDocument.Open(memorystream, true))
            {
                wordDoc.GetMergeFields("TaxRefundId").ReplaceWithText(taxDeclaration.RefundTaxId);
                wordDoc.GetMergeFields("TaxRefundDate").ReplaceWithText(taxDeclaration.RefundTaxDate);

                wordDoc.GetMergeFields("ImportAmount").ReplaceWithText(taxDeclaration.ImportAmount);
                wordDoc.GetMergeFields("VATAmount").ReplaceWithText(taxDeclaration.VATAmount);

                wordDoc.GetMergeFields("TotalAmount").ReplaceWithText(taxDeclaration.TotalAmount);
                var totalAmountVietnamese = taxDeclaration.TotalAmount.ToInt().ToVietnameseText().FirstCharToUpper();
                wordDoc.GetMergeFields("ToTalAmountVietnamese").ReplaceWithText(totalAmountVietnamese);

                wordDoc.GetMergeFields("AmountedImport").ReplaceWithText(taxDeclaration.ImportAmounted);
                wordDoc.GetMergeFields("AmountedVAT").ReplaceWithText(taxDeclaration.VATAmounted);

                wordDoc.GetMergeFields("ImportMustPayAmount").ReplaceWithText(taxDeclaration.ImportMustPayAmount);
                wordDoc.GetMergeFields("VATMustPayAmount").ReplaceWithText(taxDeclaration.VATMustPayAmount);

                wordDoc.MainDocumentPart.Document.Save();
                wordDoc.SaveAs(@$"{outputFolder}\CV HOÀN THUẾ_{taxDeclaration.RefundTaxId}.docx").Close();
            }
        }

        [HttpPost]
        public ActionResult DeclareTaxRefund(
            string rootFolder = @"C:\Users\giau.huynh.STS\Giau\Support\TaxRefund",
            string refundId = "104612655450",
            string refundExcelFile = @"ToKhaiHQ7N_QDTQ_104612655450.xlsx", 
            string amaExcelFile = @"ToKhaiAMA_104612655450_2.xlsx",
            string outputFolder = @"C:\Users\giau.huynh.STS\Giau\Support\TaxRefund",
            string taxGovPDFFile = @"GNTThue (33).pdf")
        {
            rootFolder = $@"{rootFolder}\{refundId}\";
            var taxGovPDFFileFullPath = $"{rootFolder}{taxGovPDFFile}";

            rootFolder.ConvertXLSX();

            var taxDeclaration =  ReadData($"{rootFolder}{refundExcelFile}", $"{rootFolder}{amaExcelFile}");

            var pdfContent =  taxGovPDFFileFullPath.ReadPdfContent();
            TranformData(ref taxDeclaration, pdfContent);

            ExportData(taxDeclaration, outputFolder);
            return Ok();
        }
        
        
    }
}
