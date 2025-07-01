using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ProductionTransaction
    {
        public long TransactionId { get; set; }
        public long? ProductionOrderId { get; set; }
        public long? SaporderId { get; set; }
        public string PlantCode { get; set; }
        public long? ProductionLineId { get; set; }
        public DateTime? ProductionDate { get; set; }
        public string BatchNo { get; set; }
        public string ProductCode { get; set; }
        public string PalletCode { get; set; }
        public long? QtyProduction { get; set; }
        public long? UserIdProduction { get; set; }
        public DateTime? DateTimeProduction { get; set; }
        public string StorageLocationCode { get; set; }
        public int? QtyWarehouse { get; set; }
        public long? UserIdWarehouse { get; set; }
        public DateTime? DateTimeWarehouse { get; set; }
        public string LaneCode { get; set; }
        public DateTime? DatePutAway { get; set; }
        public long? UserIdPutAway { get; set; }
        public bool? IsDispatch { get; set; }
    }
}
