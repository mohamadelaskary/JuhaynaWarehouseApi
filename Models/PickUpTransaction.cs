using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PickUpTransaction
    {
        public long PickUpTransactionId { get; set; }
        public long? PurchaseRequisitionId { get; set; }
        public string PlantCode { get; set; }
        public string StorageLocationCode { get; set; }
        public string ProductCode { get; set; }
        public long? PickupQty { get; set; }
        public long? PickupCartoonQty { get; set; }
        public long? NoCanPerCartoon { get; set; }
        public DateTime? ProductionDate { get; set; }
        public string BatchNo { get; set; }
        public string PickUpTransactionStatus { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
        public string DeviceSerialNoAdd { get; set; }
        public string PalletCode { get; set; }
        public bool? IsShipped { get; set; }
    }
}
