using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ReceivingPalletsNeedApproval
    {
        public long ReceivingPalletsNeedApprovalId { get; set; }
        public string PalletCode { get; set; }
        public long? ProductionOrderId { get; set; }
        public long? SaporderId { get; set; }
        public string PlantCode { get; set; }
        public long? ProductionLineId { get; set; }
        public string BatchNo { get; set; }
        public string ProductCode { get; set; }
        public DateTime? ProductionDate { get; set; }
        public long? WarehouseReceivingQty { get; set; }
        public long? WarehouseCartoonReceivingQty { get; set; }
        public long? UserIdWarehouse { get; set; }
        public DateTime? DateTimeWarehouse { get; set; }
        public string DeviceSerialNoWarehouse { get; set; }
        public bool? IsProductionApproved { get; set; }
        public long? UserIdProductionApproved { get; set; }
        public DateTime? DateTimeProductionApproved { get; set; }
        public string DeviceSerialNoProductionApproved { get; set; }
        public string ProductionComment { get; set; }
    }
}
