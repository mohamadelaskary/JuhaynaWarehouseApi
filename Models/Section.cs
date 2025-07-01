using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Section
    {
        public long SectionId { get; set; }
        public string SectionName { get; set; }
        public string SectionCode { get; set; }
        public long? DepartmentId { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateAdd { get; set; }
        public long? UserIdUpdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
