using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PalletTransferToPalletTransaction
    {
        public long PalletTransferToPalletTransactionId { get; set; }
        public string PalletCodeFrom { get; set; }
        public string PalletCodeTo { get; set; }
        public long? Qty { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
    }
}
