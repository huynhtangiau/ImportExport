using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExport.Core.CrossCutting.Settings
{
    public class ColumnIndexGrossWeightSettings
    {
        public ColumnIndexGrossWeightSettings() { }
        public int SAPCode { get; set; } = 1;
        public int Gross { get; set; } = 2;
        public int Name { get; set; } = 3;
    }
    public class GrossWeightSettings
    {
        public GrossWeightSettings() { ColumnIndex = new ColumnIndexGrossWeightSettings(); }
        public ColumnIndexGrossWeightSettings ColumnIndex { get; set; }
    }
}
