using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImportExport.API.Models
{
    public class PCPFileModel: FileModel
    {
        public DateTime DateIssue { get; set; }
        public string ProductName { get; set; }

    }
}
