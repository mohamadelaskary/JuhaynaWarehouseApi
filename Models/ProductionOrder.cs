using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ProductionOrder
    {
        public long ProductionOrderId { get; set; }
        public long? SapOrderId { get; set; }
        public long? Qty { get; set; }
        public string Uom { get; set; }
        public DateTime? OrderDate { get; set; }
        public string ProductCode { get; set; }
        public string PlantCode { get; set; }
        public string OrderTypeCode { get; set; }
        public bool? IsMobile { get; set; }
        public string PlantCodePlanning { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string ProductionVersion { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
        public long? UserIdUpdate { get; set; }
        public DateTime? DateTimeUpdate { get; set; }
        public bool? IsCreatedOnSap { get; set; }
        public bool? IsCommingFromSap { get; set; }
        public bool? IsReleased { get; set; }
        public long? UserIdRelease { get; set; }
        public DateTime? DateTimeRelease { get; set; }
        public string DeviceSerialNoRelease { get; set; }
        public string DeviceSerialNo { get; set; }
        public string MessageCode { get; set; }
        public string MessageText { get; set; }
        public string Message { get; set; }
        public bool? IsClosed { get; set; }
        public long? UserIdClosed { get; set; }
        public DateTime? DateTimeClosed { get; set; }
    }
}
