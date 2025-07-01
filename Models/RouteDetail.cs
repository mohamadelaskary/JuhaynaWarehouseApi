using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class RouteDetail
    {
        public long RouteDetailId { get; set; }
        public long? RouteId { get; set; }
        public string DeparturePoint { get; set; }
        public string DestinationPoint { get; set; }
        public string Staging { get; set; }
    }
}
