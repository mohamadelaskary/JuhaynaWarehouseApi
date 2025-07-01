using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class VwStockDetail
    {
        public long StockId { get; set; }
        public long? StockLocationId { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string ProductDescAr { get; set; }
        public string Uom { get; set; }
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long? StorageLocationId { get; set; }
        public string StorageLocationCode { get; set; }
        public string StorageLocationDesc { get; set; }
        public DateTime? ProductionDate { get; set; }
        public long? Qty { get; set; }
        public long? LaneId { get; set; }
        public string LaneCode { get; set; }
        public string LaneDesc { get; set; }
    }
}
