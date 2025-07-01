using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class ProductionLine
    {
        public long ProductionLineId { get; set; }
        public string ProductionLineCode { get; set; }
        public string ProductionLineDesc { get; set; }
        public string ProductionLineNumber { get; set; }
        public string PlantCode { get; set; }
        public string GroupCode { get; set; }
        public string ProductTypes { get; set; }
        public long? TheoriticalcapacityHour { get; set; }
        public string CapacityUnitofMeasure { get; set; }
        public long? NumberofHoursPerDay { get; set; }
        public long? NumberOfDaysPerMonth { get; set; }
        public long? CapacityUtilizationRate { get; set; }
        public string PlCode { get; set; }
    }
}
