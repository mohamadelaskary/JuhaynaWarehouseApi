using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PalletWip
    {
        public long PalletWipid { get; set; }
        public string PalletCode { get; set; }
        public long? ProductionOrderId { get; set; }
        public long? SaporderId { get; set; }
        public string PlantCode { get; set; }
        public long? ProductionLineId { get; set; }
        public string BatchNo { get; set; }
        public string ProductCode { get; set; }
        public DateTime? ProductionDate { get; set; }
        public bool? IsWarehouseLocation { get; set; }
        public long? ReceivingQty { get; set; }
        public long? ProductionQtyCheckIn { get; set; }
        public long? ProductionQtyCheckOut { get; set; }
        public string DeviceSerialNoCheckIn { get; set; }
        public string DeviceSerialNoCheckOut { get; set; }
        public bool? IsWarehouseReceived { get; set; }
        public string StorageLocationCode { get; set; }
        public long? UserIdWarehouse { get; set; }
        public DateTime? DateTimeWarehouse { get; set; }
        public string DeviceSerialNoWarehouse { get; set; }
        public long? WarehouseReceivingQty { get; set; }
        public long? PickedupQtyFromPallet { get; set; }
        public string LaneCode { get; set; }
        public long? UserIdPutAway { get; set; }
        public DateTime? DateTimePutAway { get; set; }
        public string DeviceSerialNoPutAway { get; set; }
        public long? PickupQty { get; set; }
        public long? PickupCartoonQty { get; set; }
        public bool? IsPickup { get; set; }
        public bool? IsChangedQuantityByWarehouse { get; set; }
        public bool? IsProductionTakeAction { get; set; }
    }
}
