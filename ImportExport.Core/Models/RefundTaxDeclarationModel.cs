using System;
namespace ImportExport.Core.Models
{
    public class RefundTaxDeclarationModel
    {
        public RefundTaxDeclarationModel()
        {
        }
        public string RefundTaxId { get; set; }
        public string RefundTaxDate { get; set; }
        public string ImportAmount { get; set; }
        public string VATAmount { get; set; }
        public string TotalAmount { get; set; }
        public string ImportAmounted { get; set; }
        public string VATAmounted { get; set; }
        public string ImportMustPayAmount { get; set; }
        public string VATMustPayAmount { get; set; }
        public string RegisterDate { get; set; }
    }
}
