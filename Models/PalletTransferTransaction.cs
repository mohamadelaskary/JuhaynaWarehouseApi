using System;
using System.Collections.Generic;

namespace GBSWarehouse.Models
{
    public partial class PalletTransferTransaction
    {
        public long PalletTransferTransactionId { get; set; }
        public long? ProductionOrderIdFrom { get; set; }
        public long? SaporderIdFrom { get; set; }
        public string PlantCodeFrom { get; set; }
        public long? ProductionLineIdFrom { get; set; }
        public DateTime? ProductionDateFrom { get; set; }
        public string BatchNoFrom { get; set; }
        public long? UserIdReceived { get; set; }
        public DateTime? DateTimeReceived { get; set; }
        public string DeviceSerialNoReceived { get; set; }
        public string ProductCode { get; set; }
        public long? Qty { get; set; }
        public string PalletCode { get; set; }
        public long? ProductionOrderIdTo { get; set; }
        public long? SaporderIdTo { get; set; }
        public string PlantCodeTo { get; set; }
        public long? ProductionLineIdTo { get; set; }
        public DateTime? ProductionDateTo { get; set; }
        public string BatchNoTo { get; set; }
        public long? UserIdAdd { get; set; }
        public DateTime? DateTimeAdd { get; set; }
        public string DeviceSerialNoTransfered { get; set; }
        public decimal? NumeratorforConversionPal { get; set; }
        public decimal? DenominatorforConversionPal { get; set; }
        public decimal? NumeratorforConversionPac { get; set; }
        public decimal? DenominatorforConversionPac { get; set; }
    }
}
