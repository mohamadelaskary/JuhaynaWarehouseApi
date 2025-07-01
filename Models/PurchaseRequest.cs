using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PurchaseRequest
    {
        public long Prid { get; set; }
        public string Prno { get; set; }
        public string ProductCode { get; set; }
        public long? Qty { get; set; }
        public string PlantCodeIssue { get; set; }
        public string ShipmentDate { get; set; }
        public string PlantCodeDestination { get; set; }
    }
}
