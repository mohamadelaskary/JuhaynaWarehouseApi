using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class CloseBatch
    {
        public long CloseBatchId { get; set; }
        public long? SapOrderId { get; set; }
        public long? Qty { get; set; }
        public string Uom { get; set; }
        public DateTime? DateofManufacture { get; set; }
        public DateTime? PostingDateintheDocument { get; set; }
        public string ProductCode { get; set; }
        public string BatchNumber { get; set; }
        public string PlantCode { get; set; }
        public string Storagelocation { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
        public string MessageCode { get; set; }
        public string MessageText { get; set; }
        public string MessageType { get; set; }
        public string MaterialDocumentYear { get; set; }
        public string Message { get; set; }
        public string NumberofMaterialDocument { get; set; }
        public bool? IsCreatedOnSap { get; set; }
    }
}
