﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExport.Core.Models
{
    public class ProductLicenseModel
    {
        public ProductLicenseModel()
        {
            Items = new List<ProductModel>();
        }
        public string ProductNo { get; set; }
        public List<ProductModel> Items { get; set; }
    }
}
