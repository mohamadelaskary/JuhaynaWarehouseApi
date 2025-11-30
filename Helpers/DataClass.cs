using GBSWarehouse.Models;
using System;
using System.Collections.Generic;

namespace GBSWarehouse.Helpers
{
    public class UploadFileParams
    {
        public byte[] ByteArray { get; set; }
        public string FileName { get; set; }
    }
    public class Token
    {
        public string exp { get; set; }
        public string email { get; set; }
        public int userId { get; set; }
    }
    public class SapProductGetListParam
    {
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapProductAddParam
    {
        public string PlantCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string ProductDescAr { get; set; }
        public string Uom { get; set; }
        public decimal? NumeratorforConversionPal { get; set; }
        public decimal? DenominatorforConversionPal { get; set; }
        public decimal? NumeratorforConversionPac { get; set; }
        public decimal? DenominatorforConversionPac { get; set; }
        public bool? IsWarehouseLocation { get; set; }
        public string applang { get; set; }
        public string token { get; set; }

    }
    public class ProductsParam
    {
        public string ErrorMsg { get; set; }
        public string PlantCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string ProductDescAr { get; set; }
        public string Uom { get; set; }
        public decimal? NumeratorforConversionPal { get; set; }
        public decimal? DenominatorforConversionPal { get; set; }
        public decimal? NumeratorforConversionPac { get; set; }
        public decimal? DenominatorforConversionPac { get; set; }
        public bool? IsWarehouseLocation { get; set; }
    }
    public class SapProductAddListParam
    {
        public List<ProductsParam> Products { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class RoutesParam
    {
        public string ErrorMsg { get; set; }
        public string RouteCode { get; set; }
        public string RouteDesc { get; set; }
        public string Staging { get; set; }
        public string DeparturePoint { get; set; }
        public string DestinationPoint { get; set; }
    }
    public class SapRoutesAddListParam
    {
        public List<RoutesParam> Routes { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class ShipmentTypesParam
    {
        public string ErrorMsg { get; set; }
        public string PlantCode { get; set; }
        public string ShipmentTypeCode { get; set; }
        public string ShipmentTypeDesc { get; set; }
    }
    public class SapShipmentTypesAddListParam
    {
        public List<ShipmentTypesParam> ShipmentTypes { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapPlantGetListParam
    {
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapPlantAddParam
    {
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class PlantsParam
    {
        public string ErrorMsg { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
    }
    public class SapPlantAddListParam
    {
        public List<PlantsParam> Plants { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapProductionLineGetListParam
    {
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapStorageLocationGetListParam
    {
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapLaneGetListParam
    {
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapOrderTypeGetListParam
    {
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class OrderTypesParam
    {
        public string ErrorMsg { get; set; }
        public string PlantCode { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
    }
    public class SapOrderTypeAddListParam
    {
        public List<OrderTypesParam> OrderTypes { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapOrderTypeAddParam
    {
        public string PlantCode { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapLaneAddParam
    {
        public string PlantCode { get; set; }
        public string StorageLocationCode { get; set; }
        public string LaneCode { get; set; }
        public string LaneDesc { get; set; }
        public long? NoOfPallets { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class LanesParam
    {
        public string ErrorMsg { get; set; }
        public string PlantCode { get; set; }
        public string StorageLocationCode { get; set; }
        public string LaneCode { get; set; }
        public string LaneDesc { get; set; }
        public long? NoOfPallets { get; set; }
    }
    public class SapLaneAddListParam
    {
        public List<LanesParam> Lanes { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapStorageLocationAddParam
    {
        public string PlantCode { get; set; }
        public string StorageLocationCode { get; set; }
        public string StorageLocationDesc { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class StorageLocationsParam
    {
        public string ErrorMsg { get; set; }
        public string PlantCode { get; set; }
        public string StorageLocationCode { get; set; }
        public string StorageLocationDesc { get; set; }
    }
    public class SapStorageLocationAddListParam
    {
        public List<StorageLocationsParam> StorageLocations { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapProductionLineAddParam
    {
        public string ProductionLineCode { get; set; }
        public string ProductionLineDesc { get; set; }
        public string ProductionLineNumber { get; set; }
        public string PlantCode { get; set; }
        public string GroupCode { get; set; }
        public string ProductTypes { get; set; }
        public string TheoriticalcapacityHour { get; set; }
        public string CapacityUnitofMeasure { get; set; }
        public long? NumberofHoursPerDay { get; set; }
        public long? NumberOfDaysPerMonth { get; set; }
        public long? CapacityUtilizationRate { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class SapProductionLineAddListParam
    {
        public List<ProductionLinesParam> ProductionLines { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class ProductionLinesParam
    {
        public string ErrorMsg { get; set; }
        public string ProductionLineCode { get; set; }
        public string ProductionLineDesc { get; set; }
        public string ProductionLineNumber { get; set; }
        public string PlantCode { get; set; }
        public string GroupCode { get; set; }
        public string ProductTypes { get; set; }
        public long? TheoriticalcapacityHour { get; set; }
        public string CapacityUnitofMeasure { get; set; }
        public long? NumberofHoursPerDay { get; set; }
        public long? NumberOfDaysPerMonth { get; set; }
        public long? CapacityUtilizationRate { get; set; }
    }
    public class SapAssignProductToProductionLineParam
    {
        public List<ProductionLineProductsParam> Products { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class ProductionLineProductsParam
    {
        public string ProductionLineCode { get; set; }
        public string ProductCode { get; set; }
        public string ErrorMsg { get; set; }
    }
    public class User
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string SapUserCode { get; set; }
        public string Pass { get; set; }
        public long RoleId { get; set; }
        public bool IsMobileApp { get; set; }
        public bool? IsProcessOrder { get; set; }
        public bool? IsProductionReceiving { get; set; }
        public bool? IsWarehouseReceiving { get; set; }
        public bool? IsPicking { get; set; }
        public bool? IsShipping { get; set; }
        public bool IsBackOfficeApp { get; set; }
        public bool IsActive { get; set; }
    }
    public class testTable
    {
        public testTable() { }
        public testTable(string t1, string t2)
        {
            EmployeeName = t1;
            EmployeeTitle = t2;
        }
        public string EmployeeName { get; set; }
        public string EmployeeTitle { get; set; }
    }
    public class UserInfo
    {
        public long UserId { get; set; }
        public string SapUserCode { get; set; }
        public Models.User user { get; set; }
        
    }
    public class ResponseStatus
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }

        public string ErrorMessage { get; set; }
    }
    public class ProductionLineParam
    {
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long ProductionLineId { get; set; }
        public string ProductionLineCode { get; set; }
        public string ProductionLineDesc { get; set; }
        public string ProductionLineNumber { get; set; }
        public string GroupCode { get; set; }
        public string ProductTypes { get; set; }
        public long? TheoriticalcapacityHour { get; set; }
        public string CapacityUnitofMeasure { get; set; }
        public long? NumberofHoursPerDay { get; set; }
        public long? NumberOfDaysPerMonth { get; set; }
        public long? CapacityUtilizationRate { get; set; }
        public string PlCode { get; set; }
    }
    public class StorageLocationParam
    {
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long StorageLocationId { get; set; }
        public string StorageLocationCode { get; set; }
        public string StorageLocationDesc { get; set; }
    }
    public class LaneParam
    {
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long StorageLocationId { get; set; }
        public string StorageLocationCode { get; set; }
        public string StorageLocationDesc { get; set; }
        public long LaneId { get; set; }
        public string LaneCode { get; set; }
        public string LaneDesc { get; set; }
        public long? NoOfPallets { get; set; }
    }
    public class OrderTypeParam
    {
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
    }
    public class ProcessOrdersParam
    {
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long ProductionOrderId { get; set; }
        public long? SapOrderId { get; set; }
        public long? Qty { get; set; }
        public long? QtyCartoon { get; set; }
        public DateTime? OrderDate { get; set; }
        public bool? IsMobile { get; set; }
        public bool? IsCreatedOnSap { get; set; }
        public bool? IsReleased { get; set; }
        public bool HasDetails { get; set; }
        public bool? IsClosed { get; set; }

        public long UserIdCreated { get; set; }
        public string? UserNameCreated { get; set; }
    }
    public class ProcessOrdersDetailsParam
    {

        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long ProductionOrderId { get; set; }
        public long? SapOrderId { get; set; }
        public long? Qty { get; set; }
        public long? QtyCartoon { get; set; }
        public DateTime? OrderDate { get; set; }
        public bool? IsMobile { get; set; }
        public bool? IsCreatedOnSap { get; set; }
        public bool? IsReleased { get; set; }
        public bool HasDetails { get; set; }
        public List<ProcessOrderDetails> processOrderDetails { get; set; }
    }
    public class ProcessOrdersInDetailsParam
    {

        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long ProductionOrderId { get; set; }
        public long? SapOrderId { get; set; }
        public long? Qty { get; set; }
        public DateTime? OrderDate { get; set; }
        public bool? IsMobile { get; set; }
        public bool? IsCreatedOnSap { get; set; }
        public bool? IsReleased { get; set; }
        public bool HasDetails { get; set; }
        public List<ProcessOrderInDetails> processOrderDetails { get; set; }
    }
    public class RoutePalletParam
    {
        public long PurchaseRequisitionId { get; set; }
        public string PurchaseRequisitionNo { get; set; }
        public string BatchNo { get; set; }
        public string PlantCodeDestination { get; set; }
        public string PlantDescDestination { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public DateTime? ProductionDate { get; set; }
        public long? PickupCartoonQty { get; set; }
        public string PalletCode { get; set; }
        public bool? IsShipped { get; set; }
    }
    public class ShipmentParams
    {
        public string ShipmentNo { get; set; }
        public string ShipmentTypeCode { get; set; }
        public string RouteCode { get; set; }
        public string RouteDesc { get; set; }
        public List<string> RouteDestinations { get; set; }
        public long? TruckCapacity { get; set; }
        public string VendorNo { get; set; }
        public string VendorName { get; set; }
        public long? RouteId { get; set; }
        public string PlantCodeDestination { get; set; }
    }
    public class PostShipmentParam
    {
        public long UserID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string ShipmentNo { get; set; }
        public string RouteCode { get; set; }
        public List<RoutePalletParam> Pallets { get; set; }
        public string TruckPlateNo { get; set; }
        public string DriverName { get; set; }
        public string applang { get; set; }
    }
    public class PurchaseRequestParam
    {
        public long PlantId_Issue { get; set; }
        public string PlantCode_Issue { get; set; }
        public string PlantDesc_Issue { get; set; }
        public long PlantId_Destination { get; set; }
        public string PlantCode_Destination { get; set; }
        public string PlantDesc_Destination { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long Prid { get; set; }
        public string Prno { get; set; }
        public long? Qty { get; set; }
        public string ShipmentDate { get; set; }
    }
    public class CreateProcessOrderParam
    {
        public long? SapOrderId { get; set; }
        public long UserID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string applang { get; set; }
        public long? QtyCartoon { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? BasicFinishDate { get; set; }
        public string ProductCode { get; set; }
        public string PlantCode { get; set; }
        public string OrderTypeCode { get; set; }
        public string BaseUnitofMeasure { get; set; }
        public string ProductionVersion { get; set; }
    }
    public class AddProcessOrderDetailsParam
    {
        public long UserID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string applang { get; set; }
        public long ProductionOrderId { get; set; }
        public List<BatchDetails> batchList { get; set; }
    }
    public class BatchDetails
    {
        public string ProductionLineCode { get; set; }
        public string BatchNo { get; set; }
        public DateTime? ProductionDate { get; set; }
        public long? QtyCartoon { get; set; }
    }
    public class ProcessOrderDetails
    {
        public long OrderDetailsId { get; set; }
        public long? Qty { get; set; }
        public long? QtyCartoon { get; set; }
        public string BatchNo { get; set; }
        public long ProductionLineId { get; set; }
        public string ProductionLineCode { get; set; }
        public DateTime? ProductionDate { get; set; }
        public bool? IsClosedBatch { get; set; }
        public long ProductionOrderId { get; set; }
        public bool? IsReleased { get; set; }
        public string BatchStatus { get; set; }
    }
    public class ProcessOrderInDetails
    {
        public long OrderDetailsId { get; set; }
        public long? BatchQty { get; set; }
        public long? BatchQtyCartoon { get; set; }
        public string BatchNo { get; set; }
        public long ProductionLineId { get; set; }
        public string ProductionLineCode { get; set; }
        public DateTime? ProductionDate { get; set; }
        public bool? IsClosedBatch { get; set; }
        public long ProductionOrderId { get; set; }
        public bool? IsReleased { get; set; }
        public string BatchStatus { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long PalletCapacity { get; set; }
        public List<BatchDataParam> ProductionOrderReceivingDetails { get; set; }
    }
    public class PurchaseRequisition_Details
    {
        public long PurchaseRequisitionId { get; set; }
        public string PurchaseRequisitionNo { get; set; }
        public DateTime? PurchaseRequisitionReleaseDate { get; set; }
        public long? PurchaseRequisitionQty { get; set; }
        public string PlantCodeSource { get; set; }
        public string StorageLocationCodeSource { get; set; }
        public string PlantCodeDestination { get; set; }
        public string StorageLocationCodeDestination { get; set; }
        public string PurchaseRequisitionStatus { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long? Qty { get; set; }
        public int NumberOfCartonsPerPallet { get; set; }
        public string Uom { get; set; }
        public long? LineNumber { get; set; }
        public string LineStatus { get; set; }
        public long? PickupQty { get; set; }
    }
    public class BatchDataParam
    {
        public long? PalletQty { get; set; }
        public string PalletCode { get; set; }
        public long NoCartoonPerPallet { get; set; }
        public long PalletCartoonQty { get; set; }
        public long WarehouseReceivingQty { get; set; }
        public long WarehouseReceivingCartoonQty { get; set; }
    }
    public class ProductionLineDetailsParam
    {
        public long OrderDetailsId { get; set; }
        public string BatchNo { get; set; }
        public bool? IsClosedBatch { get; set; }
        public long ProductionOrderId { get; set; }
        public bool? IsReleased { get; set; }
        public string BatchStatus { get; set; }
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long SapOrderId { get; set; }
        public long? ProductionOrderQty { get; set; }
        public long? ProductionOrderQtyCartoon { get; set; }
        public DateTime? OrderDate { get; set; }
        public long? BatchQty { get; set; }
        public long? BatchQtyCartoon { get; set; }
        public DateTime? ProductionDate { get; set; }
        public bool? IsReceived { get; set; }
        public long? ReceivedQty { get; set; }
        public long? ReceivedQtyCartoon { get; set; }
        public bool? IsWarehouseLocation { get; set; }
        public string ProductionLineCode { get; set; }
        public string ProductionLineDesc { get; set; }
        public long ProductionLineId { get; set; }
        public long NoCanPerCartoon { get; set; }
        public long NoCartoonPerPallet { get; set; }
        public long PalletCapacity { get; set; }
    }
    public class PalletDetailsParam
    {
        public string BatchNo { get; set; }
        public long ProductionOrderId { get; set; }
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long SapOrderId { get; set; }
        public long? ProductionOrderQty { get; set; }
        public long? ProductionOrderQtyCartoon { get; set; }
        public DateTime? OrderDate { get; set; }
        public long? PalletQty { get; set; }
        public decimal? NumeratorforConversionPac { get; set; }
        public decimal? NumeratorforConversionPal { get; set; }
        public decimal? DenominatorforConversionPal { get; set; }
        public decimal? DenominatorforConversionPac { get; set; }
        public bool? IsWarehouseLocation { get; set; }
        public long? ProductionQtyCheckIn { get; set; }
        public long? ProductionQtyCheckOut { get; set; }
        public bool? IsWarehouseReceived { get; set; }
        public string StorageLocationCode { get; set; }
        public long? WarehouseReceivingQty { get; set; }
        public decimal WarehouseReceivingCartoonQty { get; set; }
        public string LaneCode { get; set; }
        public string LaneDesc { get; set; }
        public decimal? PalletCartoonQty { get; set; }
        public decimal AvailableQty { get; set; }
        public decimal AvailableQtyCarton { get; set; }
        public string ProductionLineCode { get; set; }
        public string ProductionLineDesc { get; set; }
        public long? ProductionLineId { get; set; }
        public long? BatchQty { get; set; }
        public long? BatchQtyCartoon { get; set; }
        public DateTime? ProductionDate { get; set; }
        public decimal PalletCapacity { get; set; }
        public decimal NoCanPerCartoon { get; set; }
        public decimal NoCartoonPerPallet { get; set; }
        public long? ReceivedQty { get; set; }
        public long? ReceivedQtyCartoon { get; set; }
        public string PalletCode { get; set; }
        public long? PickedupQtyFromPallet { get; set; }
        public decimal? PickedupCartoonQtyFromPallet { get; set; }
        public DateTime? ReceivingDate { get; set; }
        public long? WarehouseReceivingPackage { get; set; }
        public long? WarehouseReceivingCartoonReceivedQty { get; set; }
        public bool? IsChangedQuantityByWarehouse { get; set; }
        public bool? IsProductionTakeAction { get; set; }
    }
    public class SignInParam
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string applang { get; set; }

    }
    public class ReceivedPalletDetailsNeedApprovalParam
    {
        public string BatchNo { get; set; }
        public long ProductionOrderId { get; set; }
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long? WarehouseReceivedQty { get; set; }
        public long? WarehouseReceivedCartoonQty { get; set; }
        public long ProductionReceivedCartonQty { get; set; }
        public long? ProductionLineId { get; set; }
        public string ProductionLineCode { get; set; }
        public string ProductionLineDesc { get; set; }
        public string PalletCode { get; set; }
        public DateTime? ProductionDate { get; set; }
    }
    public class ReceivedPalletDetailsParam
    {
        public string BatchNo { get; set; }
        public long ProductionOrderId { get; set; }
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long? PalletQty { get; set; }
        public decimal? PalletCartoonQty { get; set; }
        public long? ProductionLineId { get; set; }
        public string ProductionLineCode { get; set; }
        public string ProductionLineDesc { get; set; }
        public string PalletCode { get; set; }
        public bool? IsChangedQuantityByWarehouse { get; set; }
        public bool? IsProductionTakeAction { get; set; }
    }
    public class ReleaseProcessOrderParam
    {
        public long UserID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string applang { get; set; }
        public long ProductionOrderId { get; set; }
    }
    public class ReleaseBatchParam
    {
        public long UserID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string applang { get; set; }
        public long ProductionOrderId { get; set; }
        public string BatchNo { get; set; }
        public string ProductionLineCode { get; set; }
        public long? OrderDetailsId { get; set; }

    }
    public class CloseBatchParam
    {
        public long UserID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string applang { get; set; }
        public string ProductCode { get; set; }
        public long? SapOrderId { get; set; }
        public long? Qty { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? DateofManufacture { get; set; }
        public string Uom { get; set; }
        public string PlantCode { get; set; }
        public string Storagelocation { get; set; }
    }
    public class ProductParam
    {
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public decimal? NumeratorforConversionPac { get; set; }
        public decimal? DenominatorforConversionPac { get; set; }
        public decimal? NumeratorforConversionPal { get; set; }
        public decimal? DenominatorforConversionPal { get; set; }
        public string Uom { get; set; }
        public bool? IsWarehouseLocation { get; set; }
    }
    public class CreateSapOrderResponse
    {
        public string PLNBEZ { get; set; }//Material Number
        public string WERKS { get; set; }//Plant
        public string PWERK { get; set; }//Planning plant for the order
        public string AUART { get; set; }//Order Type
        public string GAMNG { get; set; }//Total order quantity
        public string GSTRP { get; set; }//Basic Start Date
        public string GLTRP { get; set; }//Basic finish date
        public string GMEIN { get; set; }//Base Unit of Measure
        public string VERID { get; set; }//Production Version
        public string AUFNR { get; set; }//Order Number
        public string messageCode { get; set; }//Message number
        public string messageText { get; set; }//Order Number Text (Created Or Failed)
        public string message { get; set; }//Message Text
        public string messageCode2 { get; set; }//Message number
        public string messageText2 { get; set; }//Order Number Text (Created Or Failed)
        public string message2 { get; set; }//Message Text
    }
    public class CreateSapOrderParameters
    {
        public string PLNBEZ { get; set; }//Material Number
        public string WERKS { get; set; }//Plant
        public string PWERK { get; set; }//Planning plant for the order
        public string AUART { get; set; }//Order Type
        public long GAMNG { get; set; }//Total order quantity
        public string GSTRP { get; set; }//Basic Start Date
        public string GLTRP { get; set; }//Basic finish date
        public string GMEIN { get; set; }//Base Unit of Measure
        public string VERID { get; set; }//Production Version
    }
    public class GetSap_ShipmentParam
    {
        public List<string> ShipmentTypeCode { get; set; }//SHTYP -> Shipment type
        public string PlannedDate { get; set; }//DPREG --> Planned date of check-in
        public string PlantCode { get; set; }//PLANT --> Plant
        public string applang { get; set; }
    }
    public class GetSap_PurchaseRequestParam
    {
        public string PurchaseRequisitionReleaseDate { get; set; }//FRGDT -> Purchase Requisition Release Date
        public string IssuingPlantCode { get; set; }//RESWK --> Supplying (issuing) plant in case of stock transport order
        public string ProductCode { get; set; }//MATNR -> Material Number
        public string PlantCode { get; set; }//WERKS -> Plant
        public string applang { get; set; }
    }
    public class GetSapPurchaseRequest_Request
    {
        public string FRGDT { get; set; }//Purchase Requisition Release Date
        public string RESWK { get; set; }//Supplying (issuing) plant in case of stock transport order
        public string MATNR { get; set; }//Material Number
        public string WERKS { get; set; }//Plant
    }
    public class SapPurchaseRequest_Response
    {
        public string WERKS { get; set; }//Plant
        public string BANFN { get; set; }//Purchase requisition number
        public string MATNR { get; set; }//Material Number
        public string FRGDT { get; set; }//Purchase Requisition Release Date
        public string BNFPO { get; set; }//Item number of purchase requisition
        public string MEINS { get; set; }//Purchase requisition unit of measure
        public string LGORT { get; set; }//Storage location
        public string messageCode { get; set; }//Message number
        public string messageText { get; set; }//PRs Text (Created Or Failed)
        public string message { get; set; }//Message Text
        public string messageType { get; set; }//Message Type
        public string MENGE { get; set; }//Purchase requisition quantity
        public string ErrorMsg { get; set; }
    }

    public class GetSapShipmentRequest_Request
    {
        public List<SHTYP_Value> SHIPMENTS_TYPES { get; set; }//Shipment types
        public string SHIPMENT_DATE { get; set; }//Planned date of check-in
        public string PLANT { get; set; }//Plant
    }
    public class SHTYP_Value
    {
        public string SHTYP { get; set; }//Shipment type
    }
    public class SapPostShipmentRequest_Request
    {
        public string TKNUM { get; set; }//Shipment Number	 
        public string BANFN { get; set; }//Purchase requisition number
        public long BNFPO { get; set; }//Item number of purchase requisition
        public string CHARG { get; set; }//Batch Number 
        public string MATNR { get; set; }//Material Number
        public long MENGE { get; set; }//Purchase requisition quantity
        public string ROUTE { get; set; }//Shipment route
        public string SIGNI { get; set; }//Container ID
        public string EXTI1 { get; set; }//External identification
        public string DPREG { get; set; }//Planned date of check-in
    }
    public class SapPostShipmentRequestList_Request
    {
        public List<SapPostShipmentRequest_Request> GetList { get; set; }
    }
    public class SapShipmentRequest_Response
    {
        public string TKNUM { get; set; }//Shipment Number
        public string TPBEZ { get; set; }//Description of Shipment
        public string SHTYP { get; set; }//Shipment type
        public string ROUTE { get; set; }//Shipment route
        public string routeName { get; set; }//Text, 40 Characters Long
        public string DPREG { get; set; }//Planned date of check-in
        public string SDABW { get; set; }//Special processing indicator
        public string indicatorName { get; set; }//Text, 40 Characters Long
        public string TDLNR { get; set; }//Number of forwarding agent
        public string NAME1 { get; set; }//Name 1
        public string ErrorMsg { get; set; }
    }

    public class SapPostShipmentRequest_Response
    {
        public string TKNUM { get; set; }//Shipment Number
        public string BANFN { get; set; }//Purchase requisition number
        public string BNFPO { get; set; }//Item number of purchase requisition
        public string CHARG { get; set; }//Batch Number
        public string msgCode { get; set; }//Message number
        public string errorText { get; set; }//Post Shipment (Created Or Failed)
    }

    public class CloseBatchResponse
    {
        public string MATNR { get; set; }//Material Number
        public string AUFNR { get; set; }//Order Number
        public string MENGE { get; set; }//Quantity
        public string CHARG { get; set; }//Batch Number 
        public string HSDAT { get; set; }//Date of Manufacture  
        public string MEINS { get; set; }//Base Unit of Measure 
        public string BUDAT { get; set; }//Posting Date in the Document 
        public string WERKS { get; set; }//Plant
        public string LGORT { get; set; }//Storage location   
        public string MBLNR { get; set; }//Number of Material Document
        public string MJAHR { get; set; }//Material Document Year
        public string messageCode { get; set; }//Message number
        public string messageText { get; set; }//Order Number Text (Created Or Failed)
        public string message { get; set; }//Message Text
        public string messageType { get; set; }//Message Type
    }
    public class CloseBatchParameters
    {
        public string MATNR { get; set; }//Material Number
        public string AUFNR { get; set; }//Order Number
        public string MENGE { get; set; }//Quantity
        public string CHARG { get; set; }//Batch Number
        public string HSDAT { get; set; }//Date of Manufacture
        public string MEINS { get; set; }//Base Unit of Measure
        public string BUDAT { get; set; }//Posting Date in the Document
        public string WERKS { get; set; }//Plant
        public string LGORT { get; set; }//Storage location
    }
    public class RouteDetailParam
    {
        public long RouteId { get; set; }
        public string RouteCode { get; set; }
        public string RouteDesc { get; set; }
        public string Staging { get; set; }
        public string DeparturePoint { get; set; }
        public string DestinationPoint { get; set; }
    }
    public class ShipmentTypeDetailParam
    {
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long ShipmentTypeId { get; set; }
        public string ShipmentTypeCode { get; set; }
        public string ShipmentTypeDesc { get; set; }
    }

    public class PurchaseRequisitionsParam
    {
        //Response
        //WERKS         Types	CHAR	4	0	Plant
        //BANFN         Types   CHAR    10  0   Purchase requisition number
        //MATNR         Types   CHAR    18  0   Material Number
        //FRGDT         Types   DATS    8   0   Purchase Requisition Release Date
        //BNFPO         Types   NUMC    5   0   Item number of purchase requisition
        //MEINS         Types   UNIT    3   0   Purchase requisition unit of measure
        //LGORT         Types   CHAR    4   0   Storage location
        //messageCode  Types   NUMC    3   0   Message number
        //messageText  Types   CHAR    10  0   PRs Text(Created Or Failed)
        //message       Types   CHAR    200 0   Message Text
        //messageType  Types   CHAR    3   0   Message Type
        //MENGE         Types   QUAN    13  3   Purchase requisition quantity
        public string PurchaseRequisitionNo { get; set; }
        public DateTime? PurchaseRequisitionReleaseDate { get; set; }
        public long? PurchaseRequisitionQty { get; set; }
        public string PlantCode_Source { get; set; }
        public string StorageLocationCode_Source { get; set; }
        public string PlantCode_Destination { get; set; }
        public string StorageLocationCode_Destination { get; set; }
        public long? LineNumber { get; set; }
        public string ProductCode { get; set; }
        public long? ProductQty { get; set; }
        public string Uom { get; set; }
        public string ErrorMsg { get; set; }
    }
    public class SapPurchaseRequisitionListParam
    {
        public List<PurchaseRequisitionsParam> PurchaseRequisitions { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }

    public class ShipmentsParam
    {
        //Response
        //TKNUM	Types	CHAR	10	0	Shipment Number
        //TPBEZ Types   CHAR    20  0   Description of Shipment
        //SHTYP   Types CHAR    4   0   Shipment type
        //ROUTE Types   CHAR    6   0   Shipment route
        //routeName Types   CHAR    40  0   Text, 40 Characters Long
        //DPREG Types   DATS    8   0   Planned date of check-in
        //SDABW Types   CHAR    4   0   Special processing indicator
        //indicatorName  Types CHAR    40  0   Text, 40 Characters Long
        //TDLNR Types   CHAR    10  0   Number of forwarding agent
        //NAME1 Types   CHAR    35  0   Name 1
        public string ShipmentNo { get; set; }
        public string ShipmentTypeCode { get; set; }
        public string RouteCode { get; set; }
        public string PlantCode_Destination { get; set; }
        public string ErrorMsg { get; set; }
    }
    public class SapShipmentAddListParam
    {
        public List<ShipmentsParam> Shipments { get; set; }
        public string applang { get; set; }
        public string token { get; set; }
    }
    public class PalletDetails2Param
    {
        public string PalletCode { get; set; }
        public long PlantId { get; set; }
        public string PlantCode { get; set; }
        public string PlantDesc { get; set; }
        public long OrderTypeId { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderTypeDesc { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public long ProductionOrderId { get; set; }
        public long SapOrderId { get; set; }
        public long? ProductionOrderQty { get; set; }
        public long? ProductionOrderQtyCartoon { get; set; }
        public DateTime? OrderDate { get; set; }
        public string BatchNo { get; set; }
        public long? PalletQty { get; set; }
        public decimal PalletCartoonQty { get; set; }
        public bool? IsWarehouseLocation { get; set; }
        public bool? IsWarehouseReceived { get; set; }
        public string StorageLocationCode { get; set; }
        public long? WarehouseReceivingQty { get; set; }
        public decimal WarehouseReceivingCartoonQty { get; set; }
        public string LaneCode { get; set; }
        public long ProductionLineId { get; set; }
        public string ProductionLineCode { get; set; }
        public string ProductionLineDesc { get; set; }
        public long? BatchQty { get; set; }
        public long? BatchQtyCartoon { get; set; }
        public long? ReceivedQty { get; set; }
        public long? ReceivedQtyCartoon { get; set; }
        public long? WarehouseReceivingPackage { get; set; }
        public long? WarehouseReceivingCartoonReceivedQty { get; set; }
    }
    public class BatchesListParam
    {
        public string BatchNo { get; set; }
        public long? ProductionOrderId { get; set; }
    }
    public class PalletTransferParam
    {
        public long UserID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string applang { get; set; }
        public long ProductionOrderId { get; set; }
        public string BatchNo { get; set; }
        public List<string> PalletsCode { get; set; }

    }
    public class PalletTransferToPalletParam
    {
        public long UserID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string applang { get; set; }
        public string PalletCodeFrom { get; set; }
        public string PalletCodeTo { get; set; }
        public long? CartoonReceivedQty { get; set; }

    }
    public class GenerateBatchNoParam
    {
        public string applang { get; set; }
        public string ProductionLineCode { get; set; }
    }
}
