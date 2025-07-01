using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class VwProductionOrder
    {
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long ProductionOrderId { get; set; }
        public long? SapOrderId { get; set; }
        public long? Qty { get; set; }
        public DateTime? OrderDate { get; set; }
        public bool? IsMobile { get; set; }
        public bool? IsCreatedOnSap { get; set; }
        public bool? IsReleased { get; set; }
    }
}
