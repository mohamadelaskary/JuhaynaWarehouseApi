using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class VwPurchaseRequisition
    {
        public long PurchaseRequisitionId { get; set; }
        public long? PurchaseRequisitionDetailId { get; set; }
        public string PurchaseRequisitionNo { get; set; }
        public DateTime? PurchaseRequisitionReleaseDate { get; set; }
        public long? PurchaseRequisitionQty { get; set; }
        public long? PlantIdSource { get; set; }
        public string PlantCodeSource { get; set; }
        public string PlantDescSource { get; set; }
        public long? StorageLocationIdSource { get; set; }
        public string StorageLocationCodeSource { get; set; }
        public string StorageLocationDescSource { get; set; }
        public long? PlantIdDest { get; set; }
        public string PlantCodeDest { get; set; }
        public string PlantDescDest { get; set; }
        public long? StorageLocationIdDest { get; set; }
        public string StorageLocationCodeDest { get; set; }
        public string StorageLocationDescDest { get; set; }
        public string UserNameAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
        public bool? IsShipment { get; set; }
        public string PurchaseRequisitionStatus { get; set; }
        public bool? IsInProgress { get; set; }
        public bool? IsClosed { get; set; }
        public string UserNameClosed { get; set; }
        public DateTime? DateClosed { get; set; }
        public long? ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string ProductDescAr { get; set; }
        public long? Qty { get; set; }
        public long? PickupQty { get; set; }
        public string Uom { get; set; }
        public long? LineNumber { get; set; }
    }
}
