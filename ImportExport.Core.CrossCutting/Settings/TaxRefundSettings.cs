using System;
namespace ImportExport.Core.CrossCutting.Settings
{
    public class TaxRefundSettings
    {
        public TaxRefundSettings()
        {
            Template = new TaxRefundTemplateSetting();
        }
        public TaxRefundTemplateSetting Template { get; set; }
    }
}
