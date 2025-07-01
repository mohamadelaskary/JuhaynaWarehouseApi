using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PalletIncubationsHistory
    {
        public long PalletIncubationsHistoryId { get; set; }
        public long? PalletIncubationsId { get; set; }
        public string PalletCode { get; set; }
        public long? ProductionOrderId { get; set; }
        public long? LocationId { get; set; }
        public long? SaporderId { get; set; }
        public string PlantCode { get; set; }
        public long? ProductionLineId { get; set; }
        public string BatchNo { get; set; }
        public string ProductCode { get; set; }
        public DateTime? ProductionDate { get; set; }
        public long? PalletQty { get; set; }
        public long? UserCheckIn { get; set; }
        public long? UserCheckOut { get; set; }
        public string DeviceSerialNoCheckIn { get; set; }
        public string DeviceSerialNoCheckOut { get; set; }
        public DateTime? DateTimeCheckIn { get; set; }
        public DateTime? DateTimeCheckOut { get; set; }
        public DateTime? TransDateCheckIn { get; set; }
        public DateTime? TransDateCheckOut { get; set; }
    }
}
