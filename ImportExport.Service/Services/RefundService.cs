using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using ImportExport.Core.CrossCutting.Settings;
using ImportExport.Core.Models;
using ImportExport.CrossCutting.Utils.Helpers;
using ImportExport.Service.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace ImportExport.Service.Services
{
    public class RefundService: BaseService, IRefundService
    {
        private readonly IOptions<TaxRefundSettings> _taxRefundSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        public RefundService(IOptions<TaxRefundSettings> taxRefundSettings, IHostingEnvironment hostingEnvironment)
        {
            _taxRefundSettings = taxRefundSettings;
            _hostingEnvironment = hostingEnvironment;

        }

        public async  Task<RefundTaxDeclarationModel> ReadData(string refundpath, string amaPath)
        {
            var refundTaxDeclaration = new RefundTaxDeclarationModel();
            using (var package = new ExcelPackage(refundpath))
            {
                var worksheet = package.Workbook.Worksheets[0];
                refundTaxDeclaration.RefundTaxId = worksheet.GetText(worksheet.Cells["E4"]);
                refundTaxDeclaration.RefundTaxDate = worksheet.GetText(worksheet.Cells["G83"]);
                refundTaxDeclaration.RegisterDate = worksheet.GetText(worksheet.Cells["G8"]);
            }
            using (var package = new ExcelPackage(amaPath))
            {
                var worksheet = package.Workbook.Worksheets[0];
                refundTaxDeclaration.ImportAmount = worksheet.GetText(worksheet.Cells["P34"]);
                refundTaxDeclaration.VATAmount = worksheet.GetText(worksheet.Cells["P35"]);
            }
            return await Task.FromResult( refundTaxDeclaration);
        }
        public void TranformData(RefundTaxDeclarationModel refundTaxDeclaration
            , string govTaxContent)
        {
            var pattern = @"((1901)|(1702))[\s]+[\d\,]+.*VNĐ";
            var lstMatched = Regex.Matches(govTaxContent, pattern);
            var ImportAmounted = lstMatched[0].Value
                .Replace("1901", string.Empty)
                .Replace("VNĐ", string.Empty, StringComparison.CurrentCultureIgnoreCase).Trim();
            var VATAmounted = lstMatched[1].Value
                .Replace("1702", string.Empty)
                .Replace("VNĐ", string.Empty, StringComparison.CurrentCultureIgnoreCase).Trim();

            refundTaxDeclaration.RefundTaxDate = refundTaxDeclaration.RefundTaxDate.ToDate();
            refundTaxDeclaration.RegisterDate = refundTaxDeclaration.RegisterDate.ToDate();

            var importAmount = refundTaxDeclaration.ImportAmount.ToInt();
            var vATAmount = refundTaxDeclaration.VATAmount.ToInt();
            var importAmounted = ImportAmounted.ToInt();
            var vATAmounted = VATAmounted.ToInt();

            refundTaxDeclaration.TotalAmount = importAmount.ToComma();
            refundTaxDeclaration.ImportAmount = refundTaxDeclaration.ImportAmount.ToComma();
            refundTaxDeclaration.VATAmount = refundTaxDeclaration.VATAmount.ToComma();
            refundTaxDeclaration.ImportMustPayAmount = (importAmounted - importAmount).ToComma();
            refundTaxDeclaration.VATMustPayAmount = (vATAmounted - vATAmount).ToComma();
            refundTaxDeclaration.ImportAmounted = importAmounted.ToComma();
            refundTaxDeclaration.VATAmounted = vATAmounted.ToComma();
        }
        public void ExportData(RefundTaxDeclarationModel taxDeclaration, string outputFolder)
        {
            var path = Path.Combine(_hostingEnvironment.ContentRootPath, _taxRefundSettings.Value.Template.TaxDeclaration);
            var fileBytes = File.ReadAllBytes(path);
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

                wordDoc.GetMergeFields("RegisterDate").ReplaceWithText(taxDeclaration.RegisterDate);
                wordDoc.GetMergeFields("TodayDate").ReplaceWithText(DateTime.Now.ToString("yyyy/MM/dd"));

                wordDoc.MainDocumentPart.Document.Save();
                wordDoc.SaveAs(Path.Combine(outputFolder, $"CV HOÀN THUẾ_{taxDeclaration.RefundTaxId}.docx")).Close();
            }
        }
    }
}
