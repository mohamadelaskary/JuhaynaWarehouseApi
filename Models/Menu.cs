using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class Menu
    {
        public Menu()
        {
            LinkRolesMenus = new HashSet<LinkRolesMenu>();
        }

        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string MenuUrl { get; set; }
        public int MenuParentId { get; set; }

        public virtual ICollection<LinkRolesMenu> LinkRolesMenus { get; set; }
    }
}
