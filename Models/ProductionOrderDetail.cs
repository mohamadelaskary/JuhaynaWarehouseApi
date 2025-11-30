using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ProductionOrderDetail
    {
        public long OrderDetailsId { get; set; }
        public long ProductionOrderId { get; set; }
        public long SapOrderId { get; set; }
        public long? Qty { get; set; }
        public string BatchNo { get; set; }
        public string PlantCode { get; set; }
        public DateTime? ProductionDate { get; set; }
        public string ProductCode { get; set; }
        public long? ProductionLineId { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
        public string DeviceSerialNo { get; set; }
        public long? UserIdUpdate { get; set; }
        public DateTime? DateTimeUpdate { get; set; }
        public string DeviceSerialNoUpdate { get; set; }
        public bool IsClosedBatch { get; set; } = false;
        public string BatchStatus { get; set; }
        public bool? IsReleased { get; set; }
        public long? UserIdRelease { get; set; }
        public DateTime? DateTimeRelease { get; set; }
        public string DeviceSerialNoRelease { get; set; }
        public bool? IsReceived { get; set; }
        public long? ReceivedQty { get; set; }
    }
}
