using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Role
    {
        public Role()
        {
            LinkRolesMenus = new HashSet<LinkRolesMenu>();
        }

        public long RoleId { get; set; }
        public string ArRoleName { get; set; }
        public string EnRoleName { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateAdd { get; set; }
        public long? UserIdUpdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? Locked { get; set; }
        public long? UserIdLock { get; set; }
        public DateTime? DateLock { get; set; }

        public virtual ICollection<LinkRolesMenu> LinkRolesMenus { get; set; }
    }
}
