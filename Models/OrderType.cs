using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class OrderType
    {
        public long OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public string PlantCode { get; set; }
        public string OrderCategory { get; set; }
    }
}
