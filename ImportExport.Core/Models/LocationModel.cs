using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExport.Core.Models
{
    public class LocationModel
    {
        public float Height { get; set; }
        public float Width { get; set; }
        public int PageIndex { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
