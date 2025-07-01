using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Batch
    {
        public long Id { get; set; }
        public long? BatchNo { get; set; }
        public string ProductionLineCode { get; set; }
        public DateTime? Dt { get; set; }
    }
}
