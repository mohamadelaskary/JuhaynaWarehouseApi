using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class CancelPallet
    {
        public long Id { get; set; }
        public string PalletCode { get; set; }
        public string Reason { get; set; }
        public long? UserId { get; set; }
        public DateTime? Dt { get; set; }
    }
}
