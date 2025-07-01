using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PalletSetting
    {
        public long PalletSettingId { get; set; }
        public string PalletCode { get; set; }
        public long? AutoNo { get; set; }
    }
}
