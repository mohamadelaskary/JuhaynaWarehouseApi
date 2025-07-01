using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ProductionStation
    {
        public long StationId { get; set; }
        public string PlantCode { get; set; }
        public string StationName { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
        public long? UserIdUpdate { get; set; }
        public DateTime? DateTimeUpdate { get; set; }
    }
}
