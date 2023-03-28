using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExport.Core.CrossCutting.Settings
{
    public class ColumnIndexSettings
    {
        public ColumnIndexSettings() { }
        public int ProductNo { get; set; } = 2;
        public int Product { get; set; } = 7;
        public int LicenseDate { get; set; } = 4;
        public int LicenseNo { get; set; } = 3;
    }
    public class LicenseSettings
    {
        public LicenseSettings() { ColumnIndex = new ColumnIndexSettings(); }
        public ColumnIndexSettings ColumnIndex { get; set; }
    }
}
