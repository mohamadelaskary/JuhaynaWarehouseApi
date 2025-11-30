using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ProductionOrderReceiving
    {
        public long ReceivingId { get; set; }
        public long? ProductionOrderId { get; set; }
        public long? SaporderId { get; set; }
        public string PlantCode { get; set; }
        public long? ProductionLineId { get; set; }
        public DateTime? ProductionDate { get; set; }
        public string BatchNo { get; set; }
        public string ProductCode { get; set; }
        public long? Qty { get; set; }
        public string PalletCode { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
        public string DeviceSerialNoReceived { get; set; }
        public decimal? NumeratorforConversionPal { get; set; }
        public decimal? DenominatorforConversionPal { get; set; }
        public decimal? NumeratorforConversionPac { get; set; }
        public decimal? DenominatorforConversionPac { get; set; }
        public bool? IsWarehouseLocation { get; set; }
        public long? ProductionQtyCheckIn { get; set; }
        public long? UserIdCheckIn { get; set; }
        public DateTime? DateTimeCheckIn { get; set; }
        public string DeviceSerialNoCheckIn { get; set; }
        public long? ProductionQtyCheckOut { get; set; }
        public long? UserIdCheckOut { get; set; }
        public DateTime? DateTimeCheckOut { get; set; }
        public string DeviceSerialNoCheckOut { get; set; }
        public bool? IsWarehouseReceived { get; set; }
        public string StorageLocationCode { get; set; }
        public long? UserIdWarehouse { get; set; }
        public DateTime? DateTimeWarehouse { get; set; }
        public string DeviceSerialNoWarehouse { get; set; }
        public long? WarehouseReceivingPackage { get; set; }
        public long? WarehouseReceivingCartoonReceivedQty { get; set; }
        public long? WarehouseReceivingQty { get; set; }
        public string LaneCode { get; set; }
        public long? UserIdPutAway { get; set; }
        public DateTime? DateTimePutAway { get; set; }
        public string DeviceSerialNoPutAway { get; set; }
        public bool? IsExcessProductionReceiving { get; set; }
        public bool? IsAddedInSap { get; set; }
    }
}
