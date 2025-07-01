using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Location
    {
        public long LocationId { get; set; }
        public string LocationCode { get; set; }
        public long? SapLocationId { get; set; }
        public string SapLocationCode { get; set; }
    }
}
