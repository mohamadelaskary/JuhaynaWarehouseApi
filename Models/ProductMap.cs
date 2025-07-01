using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ProductMap
    {
        public long Id { get; set; }
        public long? ProductId { get; set; }
        public long? LineNumber { get; set; }
        public long? LocationId { get; set; }
    }
}
