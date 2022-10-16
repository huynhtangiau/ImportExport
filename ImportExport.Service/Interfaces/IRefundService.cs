using System;
using System.Threading.Tasks;
using ImportExport.Core.Models;

namespace ImportExport.Service.Interfaces
{
    public interface IRefundService
    {
        Task<RefundTaxDeclarationModel> ReadData(string refundpath, string amaPath);
        void TranformData(RefundTaxDeclarationModel refundTaxDeclaration
            , string govTaxContent);
        void ExportData(RefundTaxDeclarationModel taxDeclaration, string outputFolder);
    }
}
