using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Country
    {
        public long CountryId { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateAdd { get; set; }
        public long? UserIdUpdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
