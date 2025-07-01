using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Product
    {
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string ProductDescAr { get; set; }
        public string PlantCode { get; set; }
        public string Uom { get; set; }
        public decimal? NumeratorforConversionPal { get; set; }
        public decimal? DenominatorforConversionPal { get; set; }
        public decimal? NumeratorforConversionPac { get; set; }
        public decimal? DenominatorforConversionPac { get; set; }
        public bool? IsWarehouseLocation { get; set; }
    }
}
