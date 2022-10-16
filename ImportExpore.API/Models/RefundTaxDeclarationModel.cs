using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImportExpore.API.Models
{
    public class RefundTaxDeclarationModel
    {
        public string RefundTaxId { get; set; }
        public string RefundTaxDate { get; set; }
        public string ImportAmount { get; set; }
        public string VATAmount { get; set; }
        public string TotalAmount { get; set; }
        public string ImportAmounted { get; set; }
        public string VATAmounted { get; set; }
        public string ImportMustPayAmount { get; set; }
        public string VATMustPayAmount { get; set; }

    }
}
