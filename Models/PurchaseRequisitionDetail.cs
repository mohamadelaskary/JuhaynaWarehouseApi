using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PurchaseRequisitionDetail
    {
        public long PurchaseRequisitionDetailId { get; set; }
        public long? PurchaseRequisitionId { get; set; }
        public string PurchaseRequisitionNo { get; set; }
        public string ProductCode { get; set; }
        public long? Qty { get; set; }
        public long? PickupQty { get; set; }
        public string Uom { get; set; }
        public long? LineNumber { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
        public bool? IsShipment { get; set; }
        public string LineStatus { get; set; }
        public bool? IsInProgress { get; set; }
        public bool? IsClosed { get; set; }
        public long? UserIdClosed { get; set; }
        public DateTime? DateClosed { get; set; }
    }
}
