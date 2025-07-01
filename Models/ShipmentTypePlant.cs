using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ShipmentTypePlant
    {
        public long ShipmentTypePlantId { get; set; }
        public long? ShipmentTypeId { get; set; }
        public string ShipmentTypeCode { get; set; }
        public string PlantCode { get; set; }
    }
}
