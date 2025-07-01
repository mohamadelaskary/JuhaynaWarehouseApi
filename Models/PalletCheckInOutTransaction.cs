using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PalletCheckInOutTransaction
    {
        public long PalletCheckInOutTransactionId { get; set; }
        public string PalletCode { get; set; }
        public long? ProductionOrderId { get; set; }
        public long? SaporderId { get; set; }
        public string PlantCode { get; set; }
        public long? ProductionLineId { get; set; }
        public string BatchNo { get; set; }
        public string ProductCode { get; set; }
        public DateTime? ProductionDate { get; set; }
        public string LocationCode { get; set; }
        public long? UserIdCheckIn { get; set; }
        public DateTime? DateTimeCheckIn { get; set; }
        public string DeviceSerialNoCheckIn { get; set; }
        public long? UserIdCheckOut { get; set; }
        public DateTime? DateTimeCheckOut { get; set; }
        public string DeviceSerialNoCheckOut { get; set; }
    }
}
