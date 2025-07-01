using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class StorageLocation
    {
        public long StorageLocationId { get; set; }
        public string StorageLocationCode { get; set; }
        public string StorageLocationDesc { get; set; }
        public string PlantCode { get; set; }
    }
}
