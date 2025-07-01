using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class StockLocation
    {
        public long StockLocationId { get; set; }
        public long? StockId { get; set; }
        public string PlantCode { get; set; }
        public string StorageLocationCode { get; set; }
        public string LaneCode { get; set; }
        public string ProductCode { get; set; }
        public DateTime? ProductionDate { get; set; }
        public long? Qty { get; set; }
    }
}
