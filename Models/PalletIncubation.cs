using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PalletIncubation
    {
        public long PalletIncubationsId { get; set; }
        public string PalletCode { get; set; }
        public long? LocationId { get; set; }
    }
}
