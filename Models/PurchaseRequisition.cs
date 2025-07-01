using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PurchaseRequisition
    {
        public long PurchaseRequisitionId { get; set; }
        public string PurchaseRequisitionNo { get; set; }
        public DateTime? PurchaseRequisitionReleaseDate { get; set; }
        public long? PurchaseRequisitionQty { get; set; }
        public string PlantCodeSource { get; set; }
        public string StorageLocationCodeSource { get; set; }
        public string PlantCodeDestination { get; set; }
        public string StorageLocationCodeDestination { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
        public bool? IsShipment { get; set; }
        public string PurchaseRequisitionStatus { get; set; }
        public bool? IsInProgress { get; set; }
        public bool? IsClosed { get; set; }
        public long? UserIdClosed { get; set; }
        public DateTime? DateClosed { get; set; }
    }
}
