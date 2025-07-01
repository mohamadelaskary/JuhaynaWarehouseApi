using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Lane
    {
        public long LaneId { get; set; }
        public string LaneCode { get; set; }
        public string LaneDesc { get; set; }
        public string PlantCode { get; set; }
        public string StorageLocationCode { get; set; }
        public long? NoOfPallets { get; set; }
    }
}
