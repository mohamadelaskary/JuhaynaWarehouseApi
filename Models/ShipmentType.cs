using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ShipmentType
    {
        public long ShipmentTypeId { get; set; }
        public string ShipmentTypeCode { get; set; }
        public string ShipmentTypeDesc { get; set; }
    }
}
