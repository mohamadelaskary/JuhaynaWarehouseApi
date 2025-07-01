using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class VwPurchaseRequest
    {
        public long Prid { get; set; }
        public string Prno { get; set; }
        public string ShipmentDate { get; set; }
        public long? Qty { get; set; }
        public long? ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string ProductDescAr { get; set; }
        public long? PlantIdIssue { get; set; }
        public string PlantCodeIssue { get; set; }
        public string PlantDescIssue { get; set; }
        public long? PlantIdDest { get; set; }
        public string PlantCodeDest { get; set; }
        public string PlantDescDest { get; set; }
    }
}
