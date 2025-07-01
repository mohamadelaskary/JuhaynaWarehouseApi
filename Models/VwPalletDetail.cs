using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class VwPalletDetail
    {
        public long PalletWipid { get; set; }
        public string PalletCode { get; set; }
        public long? SapOrderId { get; set; }
        public long? ProductionOrderQty { get; set; }
        public DateTime? OrderDate { get; set; }
        public long? OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public long ProductionOrderId { get; set; }
        public long? PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long? ProductionLineId { get; set; }
        public string ProductionLineCode { get; set; }
        public string ProductionLineDesc { get; set; }
        public string ProductionLineNumber { get; set; }
        public string BatchNo { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string ProductDescAr { get; set; }
        public DateTime? ProductionDate { get; set; }
        public bool? IsWarehouseLocation { get; set; }
        public long? ReceivingQty { get; set; }
        public long? ProductionQtyCheckIn { get; set; }
        public long? ProductionQtyCheckOut { get; set; }
        public string DeviceSerialNoCheckIn { get; set; }
        public string DeviceSerialNoCheckOut { get; set; }
        public bool? IsWarehouseReceived { get; set; }
        public long? StorageLocationId { get; set; }
        public string StorageLocationCode { get; set; }
        public string StorageLocationDesc { get; set; }
        public string UserNameWarehouse { get; set; }
        public DateTime? DateTimeWarehouse { get; set; }
        public string DeviceSerialNoWarehouse { get; set; }
        public long? WarehouseReceivingQty { get; set; }
        public long? PickedupQtyFromPallet { get; set; }
        public long? LaneId { get; set; }
        public string LaneCode { get; set; }
        public string LaneDesc { get; set; }
        public string UserNamePutAway { get; set; }
        public DateTime? DateTimePutAway { get; set; }
        public string DeviceSerialNoPutAway { get; set; }
        public long? PickupQty { get; set; }
        public long? PickupCartoonQty { get; set; }
        public bool? IsPickup { get; set; }
        public long? PalletQty { get; set; }
        public long? BatchQty { get; set; }
    }
}
