using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ProductionLineWip
    {
        public long ProductionLineWipid { get; set; }
        public long ProductionOrderId { get; set; }
        public long OrderDetailsId { get; set; }
        public long SapOrderId { get; set; }
        public long? Qty { get; set; }
        public string BatchNo { get; set; }
        public string PlantCode { get; set; }
        public DateTime? ProductionDate { get; set; }
        public long? ProductionLineId { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
        public string DeviceSerialNo { get; set; }
    }
}
