using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class LinkRolesMenu
    {
        public int RolesMenusId { get; set; }
        public long RolesId { get; set; }
        public int MenusId { get; set; }

        public virtual Menu Menus { get; set; }
        public virtual Role Roles { get; set; }
    }
}
