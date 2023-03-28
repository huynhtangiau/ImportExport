using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExport.Core.Models
{
    public class FileModel
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public DateTime CreatedDate { get; set; }
        public byte[] FileStream { get; set; }
    }
}
