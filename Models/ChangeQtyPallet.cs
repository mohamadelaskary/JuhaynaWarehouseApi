using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ChangeQtyPallet
    {
        public long Id { get; set; }
        public string PalletCode { get; set; }
        public long? Qty { get; set; }
        public long? NewQty { get; set; }
        public string Reason { get; set; }
        public long? UserId { get; set; }
        public DateTime? Dt { get; set; }
    }
}
