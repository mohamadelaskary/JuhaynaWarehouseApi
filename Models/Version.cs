using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Version
    {
        public long VersionId { get; set; }
        public double? BackendVersion { get; set; }
        public double? FrondendVersion { get; set; }
        public double? Apiversion { get; set; }
    }
}
