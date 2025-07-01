using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Department
    {
        public long DepartmentId { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateAdd { get; set; }
        public long? UserIdUpdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
