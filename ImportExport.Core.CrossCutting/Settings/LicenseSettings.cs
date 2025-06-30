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
    public class SemanticSearchSettings
    {
        public bool Enabled { get; set; } = true;
        public double SimilarityThreshold { get; set; } = 0.7;
        public double LowConfidenceThreshold { get; set; } = 0.4;
        public string ModelPath { get; set; } = "all-MiniLM-L6-v2.onnx";
        public bool UseFilenameMatching { get; set; } = true;
    }

    public class LicenseSettings
    {
        public LicenseSettings() 
        { 
            ColumnIndex = new ColumnIndexSettings();
            SemanticSearch = new SemanticSearchSettings();
        }
        public ColumnIndexSettings ColumnIndex { get; set; }
        public SemanticSearchSettings SemanticSearch { get; set; }
    }
}
