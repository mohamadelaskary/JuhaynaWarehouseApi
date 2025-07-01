using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PalletLocation
    {
        public long PalletLocationId { get; set; }
        public string PalletLocationCode { get; set; }
        public string PalletLocationName { get; set; }
    }
}
