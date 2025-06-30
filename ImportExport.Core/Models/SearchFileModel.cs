using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExport.Core.Models
{
    public enum SearchResultKey { Found, FirstItem, LowConfidence }
    public class SearchFileModel
    {
        public SearchResultKey ResultKey { get; set; }
        public FileModel File { get; set; }
        public double SimilarityScore { get; set; }
    }
}
