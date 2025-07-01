using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Employee
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public long? SectionId { get; set; }
        public long? DepartmentId { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateAdd { get; set; }
        public long? UserIdUpdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
