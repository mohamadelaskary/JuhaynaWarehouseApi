using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class BatchList
    {
        public long BatchId { get; set; }
        public long? SaporderId { get; set; }
        public string BatchNo { get; set; }
        public string PlantCode { get; set; }
        public long? ProductionLineId { get; set; }
        public DateTime? ProductionDate { get; set; }
        public string ProductCode { get; set; }
        public long? QtyWarehouse { get; set; }
        public long? QtyProduction { get; set; }
        public bool? IsClosed { get; set; }
        public long? UserIdClosed { get; set; }
        public DateTime? DateTimeClosed { get; set; }
    }
}
