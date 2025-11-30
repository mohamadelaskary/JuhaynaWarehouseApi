using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class User
    {
        public long UserId { get; set; }
        public string SapUserCode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public long? EmployeeId { get; set; }
        public long? RoleId { get; set; }
        public bool? IsBackOffice { get; set; }
        public bool? IsMobileOffice { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateAdd { get; set; }
        public long? UserIdUpdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public long? CountryId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsExcessProductionReceiving { get; set; }
        public bool? IsBoproductionReceiving { get; set; }
        public bool? IsBoprintPalletCode { get; set; }
        public bool? IsBoincubation { get; set; }
        public bool? IsMobWarehouseReceiving { get; set; }
        public bool? IsMobWarehousePutaway { get; set; }
        public bool? IsMobWarehousePalletsInWarehouse { get; set; }
        public bool? IsMobWarehousePalletReport { get; set; }
        public bool? IsMobWarehouseChangePalletQty { get; set; }
        public bool? IsMobProcessOrderCreate { get; set; }
        public bool? IsMobProcessOrderAddDetails { get; set; }
        public bool? IsMobProcessOrderReleaseBatch { get; set; }
        public bool? IsMobProcessOrderRunningBatch { get; set; }
        public bool? IsMobProcessOrderCloseBatch { get; set; }
        public bool? IsMobPickingStart { get; set; }
        public bool? IsMobPickingList { get; set; }
        public bool? IsMobProductionApproveWarehouseQty { get; set; }
        public bool? IsMobProductionStartReceiving { get; set; }
        public bool? IsMobProductionCancelReceiving { get; set; }
        public bool? IsMobShipping { get; set; }
        public bool? IsMobUpdateOrder { get; set; }
        public bool? IsMobCloseOrder { get; set; }

    }
}
