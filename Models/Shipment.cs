using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Shipment
    {
        public long ShipmentId { get; set; }
        public long SapshipmentId { get; set; }
        public string RouteCode { get; set; }
        public string VehiclePlateNo { get; set; }
        public string DriverCode { get; set; }
        public bool? IsRent { get; set; }
        public string PlantCodeDestination { get; set; }
        public string ShipmentNo { get; set; }
        public string ShipmentTypeCode { get; set; }
        public long? TruckCapacity { get; set; }
        public string VendorNo { get; set; }
        public string VendorName { get; set; }
        public bool? IsShipped { get; set; }
    }
}
