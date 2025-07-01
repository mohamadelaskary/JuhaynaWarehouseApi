using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace GBSWarehouse.Models
{
    public partial class GBSWarehouseContext : DbContext
    {
        public GBSWarehouseContext()
        {
        }

        public GBSWarehouseContext(DbContextOptions<GBSWarehouseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Batch> Batchs { get; set; }
        public virtual DbSet<BatchList> BatchLists { get; set; }
        public virtual DbSet<CancelPallet> CancelPallets { get; set; }
        public virtual DbSet<CancelReason> CancelReasons { get; set; }
        public virtual DbSet<ChangeQtyPallet> ChangeQtyPallets { get; set; }
        public virtual DbSet<ChangeQtyReason> ChangeQtyReasons { get; set; }
        public virtual DbSet<CloseBatch> CloseBatchs { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Lane> Lanes { get; set; }
        public virtual DbSet<LinkRolesMenu> LinkRolesMenus { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<OrderType> OrderTypes { get; set; }
        public virtual DbSet<Pallet> Pallets { get; set; }
        public virtual DbSet<PalletCheckInOutTransaction> PalletCheckInOutTransactions { get; set; }
        public virtual DbSet<PalletIncubation> PalletIncubations { get; set; }
        public virtual DbSet<PalletIncubationsHistory> PalletIncubationsHistories { get; set; }
        public virtual DbSet<PalletLocation> PalletLocations { get; set; }
        public virtual DbSet<PalletSetting> PalletSettings { get; set; }
        public virtual DbSet<PalletTransferToPalletTransaction> PalletTransferToPalletTransactions { get; set; }
        public virtual DbSet<PalletTransferTransaction> PalletTransferTransactions { get; set; }
        public virtual DbSet<PalletWip> PalletWips { get; set; }
        public virtual DbSet<PickUpTransaction> PickUpTransactions { get; set; }
        public virtual DbSet<Plant> Plants { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductMap> ProductMaps { get; set; }
        public virtual DbSet<ProductionLine> ProductionLines { get; set; }
        public virtual DbSet<ProductionLineProduct> ProductionLineProducts { get; set; }
        public virtual DbSet<ProductionLineWip> ProductionLineWips { get; set; }
        public virtual DbSet<ProductionOrder> ProductionOrders { get; set; }
        public virtual DbSet<ProductionOrderDetail> ProductionOrderDetails { get; set; }
        public virtual DbSet<ProductionOrderReceiving> ProductionOrderReceivings { get; set; }
        public virtual DbSet<ProductionStation> ProductionStations { get; set; }
        public virtual DbSet<ProductionTransaction> ProductionTransactions { get; set; }
        public virtual DbSet<PurchaseRequest> PurchaseRequests { get; set; }
        public virtual DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
        public virtual DbSet<PurchaseRequisitionDetail> PurchaseRequisitionDetails { get; set; }
        public virtual DbSet<ReceivingPalletsNeedApproval> ReceivingPalletsNeedApprovals { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Route> Routes { get; set; }
        public virtual DbSet<RouteDetail> RouteDetails { get; set; }
        public virtual DbSet<Section> Sections { get; set; }
        public virtual DbSet<Shipment> Shipments { get; set; }
        public virtual DbSet<ShipmentType> ShipmentTypes { get; set; }
        public virtual DbSet<ShipmentTypePlant> ShipmentTypePlants { get; set; }
        public virtual DbSet<Stock> Stocks { get; set; }
        public virtual DbSet<StockLocation> StockLocations { get; set; }
        public virtual DbSet<StorageLocation> StorageLocations { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Version> Versions { get; set; }
        public virtual DbSet<VwPalletDetail> VwPalletDetails { get; set; }
        public virtual DbSet<VwProductionOrder> VwProductionOrders { get; set; }
        public virtual DbSet<VwPurchaseRequest> VwPurchaseRequests { get; set; }
        public virtual DbSet<VwPurchaseRequisition> VwPurchaseRequisitions { get; set; }
        public virtual DbSet<VwStockDetail> VwStockDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Scaffold-DbContext "Server=tcp:41.196.137.5,600;Initial Catalog=GBSWarehouse;Persist Security Info=False;User ID=sa;Password=passGBSword" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -force
            IConfigurationRoot configuration = new ConfigurationBuilder()
                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                            .AddJsonFile("appsettings.json")
                            .Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Batch>(entity =>
            {
                entity.Property(e => e.BatchNo).HasDefaultValueSql("((1))");

                entity.Property(e => e.Dt)
                    .HasColumnType("datetime")
                    .HasColumnName("DT")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ProductionLineCode).HasMaxLength(50);
            });

            modelBuilder.Entity<BatchList>(entity =>
            {
                entity.HasKey(e => e.BatchId);

                entity.ToTable("BatchList");

                entity.Property(e => e.BatchId).HasColumnName("BatchID");

                entity.Property(e => e.BatchNo).HasMaxLength(50);

                entity.Property(e => e.DateTimeClosed)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Closed");

                entity.Property(e => e.IsClosed).HasDefaultValueSql("((0))");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("date");

                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.QtyProduction)
                    .HasColumnName("Qty_Production")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.QtyWarehouse)
                    .HasColumnName("Qty_Warehouse")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.SaporderId).HasColumnName("SAPOrderID");

                entity.Property(e => e.UserIdClosed).HasColumnName("UserID_Closed");
            });

            modelBuilder.Entity<CancelPallet>(entity =>
            {
                entity.Property(e => e.Dt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PalletCode).HasMaxLength(50);
            });

            modelBuilder.Entity<ChangeQtyPallet>(entity =>
            {
                entity.Property(e => e.Dt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PalletCode).HasMaxLength(50);
            });

            modelBuilder.Entity<CloseBatch>(entity =>
            {
                entity.Property(e => e.CloseBatchId).HasColumnName("CloseBatchID");

                entity.Property(e => e.BatchNumber).HasMaxLength(50);

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add");

                entity.Property(e => e.DateofManufacture).HasColumnType("datetime");

                entity.Property(e => e.IsCreatedOnSap).HasDefaultValueSql("((0))");

                entity.Property(e => e.MaterialDocumentYear).HasMaxLength(50);

                entity.Property(e => e.Message).HasColumnName("MESSAGE");

                entity.Property(e => e.MessageCode)
                    .HasMaxLength(50)
                    .HasColumnName("MESSAGE_CODE");

                entity.Property(e => e.MessageText)
                    .HasMaxLength(50)
                    .HasColumnName("MESSAGE_TEXT");

                entity.Property(e => e.MessageType).HasMaxLength(50);

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.PostingDateintheDocument).HasColumnType("datetime");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.SapOrderId).HasColumnName("SapOrderID");

                entity.Property(e => e.Storagelocation).HasMaxLength(50);

                entity.Property(e => e.Uom)
                    .HasMaxLength(50)
                    .HasColumnName("UOM");

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Country");

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.CountryCode).HasMaxLength(50);

                entity.Property(e => e.CountryName).HasMaxLength(50);

                entity.Property(e => e.DateAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Add");

                entity.Property(e => e.DateUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Update");

                entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdUpdate).HasColumnName("UserID_Update");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Department");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.DateAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Add");

                entity.Property(e => e.DateUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Update");

                entity.Property(e => e.DepartmentCode).HasMaxLength(50);

                entity.Property(e => e.DepartmentName).HasMaxLength(50);

                entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdUpdate).HasColumnName("UserID_Update");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employee");

                entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");

                entity.Property(e => e.DateAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Add");

                entity.Property(e => e.DateUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Update");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.EmployeeName).HasMaxLength(50);

                entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.Mobile).HasMaxLength(50);

                entity.Property(e => e.SectionId).HasColumnName("SectionID");

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdUpdate).HasColumnName("UserID_Update");
            });

            modelBuilder.Entity<Lane>(entity =>
            {
                entity.Property(e => e.LaneId).HasColumnName("LaneID");

                entity.Property(e => e.LaneCode).HasMaxLength(50);

                entity.Property(e => e.LaneDesc).HasMaxLength(50);

                entity.Property(e => e.NoOfPallets).HasDefaultValueSql("((0))");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.StorageLocationCode).HasMaxLength(50);
            });

            modelBuilder.Entity<LinkRolesMenu>(entity =>
            {
                entity.HasKey(e => e.RolesMenusId)
                    .HasName("PK_link_roles_menus");

                entity.Property(e => e.RolesMenusId).HasColumnName("RolesMenusID");

                entity.Property(e => e.MenusId).HasColumnName("MenusID");

                entity.Property(e => e.RolesId).HasColumnName("RolesID");

                entity.HasOne(d => d.Menus)
                    .WithMany(p => p.LinkRolesMenus)
                    .HasForeignKey(d => d.MenusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_link_roles_menus_link_roles_menus");

                entity.HasOne(d => d.Roles)
                    .WithMany(p => p.LinkRolesMenus)
                    .HasForeignKey(d => d.RolesId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_link_roles_menus_roles");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(e => e.LocationId).HasColumnName("LocationID");

                entity.Property(e => e.LocationCode).HasMaxLength(50);

                entity.Property(e => e.SapLocationCode).HasMaxLength(50);

                entity.Property(e => e.SapLocationId).HasColumnName("SapLocationID");
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.Property(e => e.MenuId).HasColumnName("MenuID");

                entity.Property(e => e.MenuName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.MenuParentId).HasColumnName("MenuParentID");

                entity.Property(e => e.MenuUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("MenuURL");
            });

            modelBuilder.Entity<OrderType>(entity =>
            {
                entity.ToTable("OrderType");

                entity.Property(e => e.OrderTypeId).HasColumnName("OrderTypeID");

                entity.Property(e => e.OrderCategory).HasMaxLength(50);

                entity.Property(e => e.OrderTypeCode).HasMaxLength(50);

                entity.Property(e => e.OrderTypeDesc).HasMaxLength(50);

                entity.Property(e => e.PlantCode).HasMaxLength(50);
            });

            modelBuilder.Entity<Pallet>(entity =>
            {
                entity.Property(e => e.PalletId).HasColumnName("PalletID");

                entity.Property(e => e.PalletCode).HasMaxLength(50);
            });

            modelBuilder.Entity<PalletCheckInOutTransaction>(entity =>
            {
                entity.Property(e => e.PalletCheckInOutTransactionId).HasColumnName("PalletCheckInOutTransactionID");

                entity.Property(e => e.BatchNo).HasMaxLength(50);

                entity.Property(e => e.DateTimeCheckIn)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_CheckIn");

                entity.Property(e => e.DateTimeCheckOut)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_CheckOut");

                entity.Property(e => e.DeviceSerialNoCheckIn)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_CheckIn");

                entity.Property(e => e.DeviceSerialNoCheckOut)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_CheckOut");

                entity.Property(e => e.LocationCode).HasMaxLength(50);

                entity.Property(e => e.PalletCode).HasMaxLength(50);

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("datetime");

                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderID");

                entity.Property(e => e.SaporderId).HasColumnName("SAPOrderID");

                entity.Property(e => e.UserIdCheckIn).HasColumnName("UserID_CheckIn");

                entity.Property(e => e.UserIdCheckOut).HasColumnName("UserID_CheckOut");
            });

            modelBuilder.Entity<PalletIncubation>(entity =>
            {
                entity.HasKey(e => e.PalletIncubationsId);

                entity.Property(e => e.PalletIncubationsId).HasColumnName("PalletIncubationsID");

                entity.Property(e => e.LocationId).HasColumnName("LocationID");

                entity.Property(e => e.PalletCode).HasMaxLength(50);
            });

            modelBuilder.Entity<PalletIncubationsHistory>(entity =>
            {
                entity.ToTable("PalletIncubationsHistory");

                entity.Property(e => e.PalletIncubationsHistoryId).HasColumnName("PalletIncubationsHistoryID");

                entity.Property(e => e.BatchNo).HasMaxLength(50);

                entity.Property(e => e.DateTimeCheckIn)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_CheckIn")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateTimeCheckOut)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_CheckOut");

                entity.Property(e => e.DeviceSerialNoCheckIn)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_CheckIn");

                entity.Property(e => e.DeviceSerialNoCheckOut)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_CheckOut");

                entity.Property(e => e.LocationId).HasColumnName("LocationID");

                entity.Property(e => e.PalletCode).HasMaxLength(50);

                entity.Property(e => e.PalletIncubationsId).HasColumnName("PalletIncubationsID");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("datetime");

                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderID");

                entity.Property(e => e.SaporderId).HasColumnName("SAPOrderID");

                entity.Property(e => e.TransDateCheckIn)
                    .HasColumnType("datetime")
                    .HasColumnName("TransDate_CheckIn");

                entity.Property(e => e.TransDateCheckOut)
                    .HasColumnType("datetime")
                    .HasColumnName("TransDate_CheckOut");

                entity.Property(e => e.UserCheckIn).HasColumnName("User_CheckIn");

                entity.Property(e => e.UserCheckOut).HasColumnName("User_CheckOut");
            });

            modelBuilder.Entity<PalletLocation>(entity =>
            {
                entity.Property(e => e.PalletLocationId).HasColumnName("PalletLocationID");

                entity.Property(e => e.PalletLocationCode).HasMaxLength(50);
            });

            modelBuilder.Entity<PalletSetting>(entity =>
            {
                entity.Property(e => e.PalletSettingId).HasColumnName("PalletSettingID");

                entity.Property(e => e.AutoNo).HasDefaultValueSql("((1))");

                entity.Property(e => e.PalletCode).HasMaxLength(50);
            });

            modelBuilder.Entity<PalletTransferToPalletTransaction>(entity =>
            {
                entity.Property(e => e.PalletTransferToPalletTransactionId).HasColumnName("PalletTransferToPalletTransactionID");

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add");

                entity.Property(e => e.PalletCodeFrom).HasMaxLength(50);

                entity.Property(e => e.PalletCodeTo).HasMaxLength(50);

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");
            });

            modelBuilder.Entity<PalletTransferTransaction>(entity =>
            {
                entity.Property(e => e.PalletTransferTransactionId).HasColumnName("PalletTransferTransactionID");

                entity.Property(e => e.BatchNoFrom)
                    .HasMaxLength(50)
                    .HasColumnName("BatchNo_From");

                entity.Property(e => e.BatchNoTo)
                    .HasMaxLength(50)
                    .HasColumnName("BatchNo_To");

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add");

                entity.Property(e => e.DateTimeReceived)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Received");

                entity.Property(e => e.DenominatorforConversionPac)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("DenominatorforConversionPAC");

                entity.Property(e => e.DenominatorforConversionPal)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("DenominatorforConversionPAL");

                entity.Property(e => e.DeviceSerialNoReceived)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_Received");

                entity.Property(e => e.DeviceSerialNoTransfered)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_Transfered");

                entity.Property(e => e.NumeratorforConversionPac)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("NumeratorforConversionPAC");

                entity.Property(e => e.NumeratorforConversionPal)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("NumeratorforConversionPAL");

                entity.Property(e => e.PalletCode).HasMaxLength(50);

                entity.Property(e => e.PlantCodeFrom)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_From");

                entity.Property(e => e.PlantCodeTo)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_To");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDateFrom)
                    .HasColumnType("date")
                    .HasColumnName("ProductionDate_From");

                entity.Property(e => e.ProductionDateTo)
                    .HasColumnType("date")
                    .HasColumnName("ProductionDate_To");

                entity.Property(e => e.ProductionLineIdFrom).HasColumnName("ProductionLineID_From");

                entity.Property(e => e.ProductionLineIdTo).HasColumnName("ProductionLineID_To");

                entity.Property(e => e.ProductionOrderIdFrom).HasColumnName("ProductionOrderID_From");

                entity.Property(e => e.ProductionOrderIdTo).HasColumnName("ProductionOrderID_To");

                entity.Property(e => e.SaporderIdFrom).HasColumnName("SAPOrderID_From");

                entity.Property(e => e.SaporderIdTo).HasColumnName("SAPOrderID_To");

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdReceived).HasColumnName("UserID_Received");
            });

            modelBuilder.Entity<PalletWip>(entity =>
            {
                entity.ToTable("PalletWIP");

                entity.Property(e => e.PalletWipid).HasColumnName("PalletWIPID");

                entity.Property(e => e.BatchNo).HasMaxLength(50);

                entity.Property(e => e.DateTimePutAway)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_PutAway");

                entity.Property(e => e.DateTimeWarehouse)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Warehouse");

                entity.Property(e => e.DeviceSerialNoCheckIn)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_CheckIn");

                entity.Property(e => e.DeviceSerialNoCheckOut)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_CheckOut");

                entity.Property(e => e.DeviceSerialNoPutAway)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_PutAway");

                entity.Property(e => e.DeviceSerialNoWarehouse)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_Warehouse");

                entity.Property(e => e.IsChangedQuantityByWarehouse).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsPickup).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsProductionTakeAction).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsWarehouseLocation).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsWarehouseReceived).HasDefaultValueSql("((0))");

                entity.Property(e => e.LaneCode).HasMaxLength(50);

                entity.Property(e => e.PalletCode).HasMaxLength(50);

                entity.Property(e => e.PickedupQtyFromPallet).HasDefaultValueSql("((0))");

                entity.Property(e => e.PickupCartoonQty).HasDefaultValueSql("((0))");

                entity.Property(e => e.PickupQty).HasDefaultValueSql("((0))");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("datetime");

                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderID");

                entity.Property(e => e.ProductionQtyCheckIn)
                    .HasColumnName("ProductionQty_CheckIn")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.ProductionQtyCheckOut)
                    .HasColumnName("ProductionQty_CheckOut")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.ReceivingQty).HasDefaultValueSql("((0))");

                entity.Property(e => e.SaporderId).HasColumnName("SAPOrderID");

                entity.Property(e => e.StorageLocationCode).HasMaxLength(50);

                entity.Property(e => e.UserIdPutAway).HasColumnName("UserID_PutAway");

                entity.Property(e => e.UserIdWarehouse).HasColumnName("UserID_Warehouse");

                entity.Property(e => e.WarehouseReceivingQty).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<PickUpTransaction>(entity =>
            {
                entity.Property(e => e.PickUpTransactionId).HasColumnName("PickUpTransactionID");

                entity.Property(e => e.BatchNo).HasMaxLength(50);

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DeviceSerialNoAdd)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_Add");

                entity.Property(e => e.IsShipped).HasDefaultValueSql("((0))");

                entity.Property(e => e.NoCanPerCartoon).HasDefaultValueSql("((0))");

                entity.Property(e => e.PalletCode).HasMaxLength(50);

                entity.Property(e => e.PickUpTransactionStatus).HasMaxLength(150);

                entity.Property(e => e.PickupQty).HasDefaultValueSql("((0))");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("datetime");

                entity.Property(e => e.PurchaseRequisitionId).HasColumnName("PurchaseRequisitionID");

                entity.Property(e => e.StorageLocationCode).HasMaxLength(50);

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");
            });

            modelBuilder.Entity<Plant>(entity =>
            {
                entity.Property(e => e.PlantId).HasColumnName("PlantID");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.PlantDesc).HasMaxLength(50);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.DenominatorforConversionPac)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("DenominatorforConversionPAC");

                entity.Property(e => e.DenominatorforConversionPal)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("DenominatorforConversionPAL");

                entity.Property(e => e.IsWarehouseLocation).HasDefaultValueSql("((0))");

                entity.Property(e => e.NumeratorforConversionPac)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("NumeratorforConversionPAC");

                entity.Property(e => e.NumeratorforConversionPal)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("NumeratorforConversionPAL");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductDesc).HasMaxLength(50);

                entity.Property(e => e.ProductDescAr).HasMaxLength(50);

                entity.Property(e => e.Uom)
                    .HasMaxLength(50)
                    .HasColumnName("UOM");
            });

            modelBuilder.Entity<ProductMap>(entity =>
            {
                entity.ToTable("ProductMap");

                entity.Property(e => e.LineNumber).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<ProductionLine>(entity =>
            {
                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.CapacityUnitofMeasure).HasMaxLength(50);

                entity.Property(e => e.GroupCode).HasMaxLength(50);

                entity.Property(e => e.PlCode).HasMaxLength(50);

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductTypes).HasMaxLength(50);

                entity.Property(e => e.ProductionLineCode).HasMaxLength(50);

                entity.Property(e => e.ProductionLineDesc).HasMaxLength(50);

                entity.Property(e => e.ProductionLineNumber).HasMaxLength(50);
            });

            modelBuilder.Entity<ProductionLineProduct>(entity =>
            {
                entity.HasKey(e => new { e.ProductionLineId, e.ProductId });

                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.ProductId).HasColumnName("ProductID");
            });

            modelBuilder.Entity<ProductionLineWip>(entity =>
            {
                entity.ToTable("ProductionLineWIP");

                entity.Property(e => e.ProductionLineWipid).HasColumnName("ProductionLineWIPID");

                entity.Property(e => e.BatchNo).HasMaxLength(50);

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add");

                entity.Property(e => e.DeviceSerialNo).HasMaxLength(50);

                entity.Property(e => e.OrderDetailsId).HasColumnName("OrderDetailsID");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("date");

                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderID");

                entity.Property(e => e.SapOrderId).HasColumnName("SapOrderID");

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");
            });

            modelBuilder.Entity<ProductionOrder>(entity =>
            {
                entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderID");

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add");

                entity.Property(e => e.DateTimeClosed)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Closed");

                entity.Property(e => e.DateTimeRelease)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Release");

                entity.Property(e => e.DateTimeUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Update");

                entity.Property(e => e.DeviceSerialNo).HasMaxLength(50);

                entity.Property(e => e.DeviceSerialNoRelease)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_Release");

                entity.Property(e => e.FinishDate).HasColumnType("date");

                entity.Property(e => e.IsClosed).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsCommingFromSap).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsCreatedOnSap).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsMobile).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsReleased).HasDefaultValueSql("((0))");

                entity.Property(e => e.Message).HasColumnName("MESSAGE");

                entity.Property(e => e.MessageCode)
                    .HasMaxLength(50)
                    .HasColumnName("MESSAGE_CODE");

                entity.Property(e => e.MessageText)
                    .HasMaxLength(50)
                    .HasColumnName("MESSAGE_TEXT");

                entity.Property(e => e.OrderDate).HasColumnType("datetime");

                entity.Property(e => e.OrderTypeCode).HasMaxLength(50);

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.PlantCodePlanning)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_Planning");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionVersion).HasMaxLength(50);

                entity.Property(e => e.Qty).HasDefaultValueSql("((0))");

                entity.Property(e => e.SapOrderId).HasColumnName("SapOrderID");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.Uom)
                    .HasMaxLength(50)
                    .HasColumnName("UOM");

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdClosed).HasColumnName("UserID_Closed");

                entity.Property(e => e.UserIdRelease).HasColumnName("UserID_Release");

                entity.Property(e => e.UserIdUpdate).HasColumnName("UserID_Update");
            });

            modelBuilder.Entity<ProductionOrderDetail>(entity =>
            {
                entity.HasKey(e => e.OrderDetailsId);

                entity.ToTable("ProductionOrder_Details");

                entity.Property(e => e.OrderDetailsId).HasColumnName("OrderDetailsID");

                entity.Property(e => e.BatchNo).HasMaxLength(50);

                entity.Property(e => e.BatchStatus).HasMaxLength(50);

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add");

                entity.Property(e => e.DateTimeRelease)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Release");

                entity.Property(e => e.DateTimeUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Update");

                entity.Property(e => e.DeviceSerialNo).HasMaxLength(50);

                entity.Property(e => e.DeviceSerialNoRelease)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_Release");

                entity.Property(e => e.DeviceSerialNoUpdate)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_Update");

                entity.Property(e => e.IsClosedBatch)
                    .HasColumnName("IsClosed_Batch")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.IsReceived).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsReleased).HasDefaultValueSql("((0))");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("datetime");

                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderID");

                entity.Property(e => e.Qty).HasDefaultValueSql("((0))");

                entity.Property(e => e.ReceivedQty).HasDefaultValueSql("((0))");

                entity.Property(e => e.SapOrderId).HasColumnName("SapOrderID");

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdRelease).HasColumnName("UserID_Release");

                entity.Property(e => e.UserIdUpdate).HasColumnName("UserID_Update");
            });

            modelBuilder.Entity<ProductionOrderReceiving>(entity =>
            {
                entity.HasKey(e => e.ReceivingId);

                entity.ToTable("ProductionOrder_Receiving");

                entity.Property(e => e.ReceivingId).HasColumnName("ReceivingID");

                entity.Property(e => e.BatchNo).HasMaxLength(50);

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add");

                entity.Property(e => e.DateTimeCheckIn)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_CheckIn");

                entity.Property(e => e.DateTimeCheckOut)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_CheckOut");

                entity.Property(e => e.DateTimePutAway)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_PutAway");

                entity.Property(e => e.DateTimeWarehouse)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Warehouse");

                entity.Property(e => e.DenominatorforConversionPac)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("DenominatorforConversionPAC");

                entity.Property(e => e.DenominatorforConversionPal)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("DenominatorforConversionPAL");

                entity.Property(e => e.DeviceSerialNoCheckIn)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_CheckIn");

                entity.Property(e => e.DeviceSerialNoCheckOut)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_CheckOut");

                entity.Property(e => e.DeviceSerialNoPutAway)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_PutAway");

                entity.Property(e => e.DeviceSerialNoReceived)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_Received");

                entity.Property(e => e.DeviceSerialNoWarehouse)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_Warehouse");

                entity.Property(e => e.IsExcessProductionReceiving).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsWarehouseLocation).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsWarehouseReceived).HasDefaultValueSql("((0))");

                entity.Property(e => e.LaneCode).HasMaxLength(50);

                entity.Property(e => e.NumeratorforConversionPac)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("NumeratorforConversionPAC");

                entity.Property(e => e.NumeratorforConversionPal)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("NumeratorforConversionPAL");

                entity.Property(e => e.PalletCode).HasMaxLength(50);

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("date");

                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderID");

                entity.Property(e => e.ProductionQtyCheckIn)
                    .HasColumnName("ProductionQty_CheckIn")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.ProductionQtyCheckOut)
                    .HasColumnName("ProductionQty_CheckOut")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.Qty).HasDefaultValueSql("((0))");

                entity.Property(e => e.SaporderId).HasColumnName("SAPOrderID");

                entity.Property(e => e.StorageLocationCode).HasMaxLength(50);

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdCheckIn).HasColumnName("UserID_CheckIn");

                entity.Property(e => e.UserIdCheckOut).HasColumnName("UserID_CheckOut");

                entity.Property(e => e.UserIdPutAway).HasColumnName("UserID_PutAway");

                entity.Property(e => e.UserIdWarehouse).HasColumnName("UserID_Warehouse");

                entity.Property(e => e.WarehouseReceivingQty).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<ProductionStation>(entity =>
            {
                entity.HasKey(e => e.StationId);

                entity.Property(e => e.StationId).HasColumnName("StationID");

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add");

                entity.Property(e => e.DateTimeUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Update");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.StationName).HasMaxLength(50);

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdUpdate).HasColumnName("UserID_Update");
            });

            modelBuilder.Entity<ProductionTransaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);

                entity.ToTable("Production_Transactions");

                entity.Property(e => e.TransactionId).HasColumnName("TransactionID");

                entity.Property(e => e.BatchNo).HasMaxLength(50);

                entity.Property(e => e.DatePutAway)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_PutAway");

                entity.Property(e => e.DateTimeProduction)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Production");

                entity.Property(e => e.DateTimeWarehouse)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Warehouse");

                entity.Property(e => e.IsDispatch).HasDefaultValueSql("((0))");

                entity.Property(e => e.LaneCode).HasMaxLength(50);

                entity.Property(e => e.PalletCode).HasMaxLength(50);

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("date");

                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderID");

                entity.Property(e => e.QtyProduction).HasColumnName("Qty_Production");

                entity.Property(e => e.QtyWarehouse).HasColumnName("Qty_Warehouse");

                entity.Property(e => e.SaporderId).HasColumnName("SAPOrderID");

                entity.Property(e => e.StorageLocationCode).HasMaxLength(50);

                entity.Property(e => e.UserIdProduction).HasColumnName("UserID_Production");

                entity.Property(e => e.UserIdPutAway).HasColumnName("UserID_PutAway");

                entity.Property(e => e.UserIdWarehouse).HasColumnName("UserID_Warehouse");
            });

            modelBuilder.Entity<PurchaseRequest>(entity =>
            {
                entity.HasKey(e => e.Prid);

                entity.ToTable("PurchaseRequest");

                entity.Property(e => e.Prid).HasColumnName("PRID");

                entity.Property(e => e.PlantCodeDestination)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_Destination");

                entity.Property(e => e.PlantCodeIssue)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_Issue");

                entity.Property(e => e.Prno)
                    .HasMaxLength(50)
                    .HasColumnName("PRNO");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ShipmentDate).HasMaxLength(50);
            });

            modelBuilder.Entity<PurchaseRequisition>(entity =>
            {
                entity.Property(e => e.PurchaseRequisitionId).HasColumnName("PurchaseRequisitionID");

                entity.Property(e => e.DateClosed)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Closed");

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsClosed).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsInProgress).HasDefaultValueSql("((1))");

                entity.Property(e => e.IsShipment).HasDefaultValueSql("((0))");

                entity.Property(e => e.PlantCodeDestination)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_Destination");

                entity.Property(e => e.PlantCodeSource)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_Source");

                entity.Property(e => e.PurchaseRequisitionNo).HasMaxLength(50);

                entity.Property(e => e.PurchaseRequisitionQty).HasDefaultValueSql("((0))");

                entity.Property(e => e.PurchaseRequisitionReleaseDate).HasColumnType("datetime");

                entity.Property(e => e.PurchaseRequisitionStatus).HasMaxLength(150);

                entity.Property(e => e.StorageLocationCodeDestination)
                    .HasMaxLength(50)
                    .HasColumnName("StorageLocationCode_Destination");

                entity.Property(e => e.StorageLocationCodeSource)
                    .HasMaxLength(50)
                    .HasColumnName("StorageLocationCode_Source");

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdClosed).HasColumnName("UserID_Closed");
            });

            modelBuilder.Entity<PurchaseRequisitionDetail>(entity =>
            {
                entity.Property(e => e.PurchaseRequisitionDetailId).HasColumnName("PurchaseRequisitionDetailID");

                entity.Property(e => e.DateClosed)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Closed");

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsClosed).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsInProgress).HasDefaultValueSql("((1))");

                entity.Property(e => e.IsShipment).HasDefaultValueSql("((0))");

                entity.Property(e => e.LineNumber).HasDefaultValueSql("((0))");

                entity.Property(e => e.LineStatus).HasMaxLength(150);

                entity.Property(e => e.PickupQty).HasDefaultValueSql("((0))");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.PurchaseRequisitionId).HasColumnName("PurchaseRequisitionID");

                entity.Property(e => e.PurchaseRequisitionNo).HasMaxLength(50);

                entity.Property(e => e.Qty).HasDefaultValueSql("((0))");

                entity.Property(e => e.Uom)
                    .HasMaxLength(50)
                    .HasColumnName("UOM");

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdClosed).HasColumnName("UserID_Closed");
            });

            modelBuilder.Entity<ReceivingPalletsNeedApproval>(entity =>
            {
                entity.ToTable("ReceivingPallets_NeedApproval");

                entity.Property(e => e.ReceivingPalletsNeedApprovalId).HasColumnName("ReceivingPallets_NeedApprovalID");

                entity.Property(e => e.BatchNo).HasMaxLength(50);

                entity.Property(e => e.DateTimeProductionApproved)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_ProductionApproved");

                entity.Property(e => e.DateTimeWarehouse)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Warehouse");

                entity.Property(e => e.DeviceSerialNoProductionApproved)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_ProductionApproved");

                entity.Property(e => e.DeviceSerialNoWarehouse)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_Warehouse");

                entity.Property(e => e.PalletCode).HasMaxLength(50);

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("datetime");

                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderID");

                entity.Property(e => e.SaporderId).HasColumnName("SAPOrderID");

                entity.Property(e => e.UserIdProductionApproved).HasColumnName("UserID_ProductionApproved");

                entity.Property(e => e.UserIdWarehouse).HasColumnName("UserID_Warehouse");

                entity.Property(e => e.WarehouseCartoonReceivingQty).HasDefaultValueSql("((0))");

                entity.Property(e => e.WarehouseReceivingQty).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.ArRoleName).HasMaxLength(255);

                entity.Property(e => e.DateAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Add");

                entity.Property(e => e.DateLock)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Lock");

                entity.Property(e => e.DateUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Update");

                entity.Property(e => e.EnRoleName).HasMaxLength(255);

                entity.Property(e => e.Locked).HasDefaultValueSql("((0))");

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdLock).HasColumnName("UserID_Lock");

                entity.Property(e => e.UserIdUpdate).HasColumnName("UserID_Update");
            });

            modelBuilder.Entity<Route>(entity =>
            {
                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.RouteCode).HasMaxLength(50);

                entity.Property(e => e.RouteDesc).HasMaxLength(150);
            });

            modelBuilder.Entity<RouteDetail>(entity =>
            {
                entity.Property(e => e.RouteDetailId).HasColumnName("RouteDetailID");

                entity.Property(e => e.DeparturePoint).HasMaxLength(50);

                entity.Property(e => e.DestinationPoint).HasMaxLength(50);

                entity.Property(e => e.RouteId).HasColumnName("RouteID");

                entity.Property(e => e.Staging).HasMaxLength(50);
            });

            modelBuilder.Entity<Section>(entity =>
            {
                entity.ToTable("Section");

                entity.Property(e => e.SectionId).HasColumnName("SectionID");

                entity.Property(e => e.DateAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Add");

                entity.Property(e => e.DateUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Update");

                entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

                entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.SectionCode).HasMaxLength(50);

                entity.Property(e => e.SectionName).HasMaxLength(50);

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdUpdate).HasColumnName("UserID_Update");
            });

            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.Property(e => e.ShipmentId).HasColumnName("ShipmentID");

                entity.Property(e => e.DriverCode).HasMaxLength(50);

                entity.Property(e => e.IsRent).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsShipped).HasDefaultValueSql("((0))");

                entity.Property(e => e.PlantCodeDestination)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_Destination");

                entity.Property(e => e.RouteCode).HasMaxLength(50);

                entity.Property(e => e.SapshipmentId).HasColumnName("SAPShipmentID");

                entity.Property(e => e.ShipmentNo).HasMaxLength(50);

                entity.Property(e => e.ShipmentTypeCode).HasMaxLength(50);

                entity.Property(e => e.TruckCapacity).HasDefaultValueSql("((0))");

                entity.Property(e => e.VehiclePlateNo).HasMaxLength(50);

                entity.Property(e => e.VendorName).HasMaxLength(150);

                entity.Property(e => e.VendorNo).HasMaxLength(50);
            });

            modelBuilder.Entity<ShipmentType>(entity =>
            {
                entity.Property(e => e.ShipmentTypeId).HasColumnName("ShipmentTypeID");

                entity.Property(e => e.ShipmentTypeCode).HasMaxLength(50);

                entity.Property(e => e.ShipmentTypeDesc).HasMaxLength(150);
            });

            modelBuilder.Entity<ShipmentTypePlant>(entity =>
            {
                entity.ToTable("ShipmentType_Plant");

                entity.Property(e => e.ShipmentTypePlantId).HasColumnName("ShipmentType_PlantID");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ShipmentTypeCode).HasMaxLength(50);

                entity.Property(e => e.ShipmentTypeId).HasColumnName("ShipmentTypeID");
            });

            modelBuilder.Entity<Stock>(entity =>
            {
                entity.ToTable("Stock");

                entity.Property(e => e.StockId).HasColumnName("StockID");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("date");

                entity.Property(e => e.Qty).HasDefaultValueSql("((0))");

                entity.Property(e => e.StorageLocationCode).HasMaxLength(50);
            });

            modelBuilder.Entity<StockLocation>(entity =>
            {
                entity.ToTable("Stock_Locations");

                entity.Property(e => e.StockLocationId).HasColumnName("StockLocationID");

                entity.Property(e => e.LaneCode).HasMaxLength(50);

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductionDate).HasColumnType("date");

                entity.Property(e => e.Qty).HasDefaultValueSql("((0))");

                entity.Property(e => e.StockId).HasColumnName("StockID");

                entity.Property(e => e.StorageLocationCode).HasMaxLength(50);
            });

            modelBuilder.Entity<StorageLocation>(entity =>
            {
                entity.Property(e => e.StorageLocationId).HasColumnName("StorageLocationID");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.StorageLocationCode).HasMaxLength(50);

                entity.Property(e => e.StorageLocationDesc).HasMaxLength(50);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.CountryId).HasColumnName("CountryID");

                entity.Property(e => e.DateAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Add");

                entity.Property(e => e.DateUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Update");

                entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.IsBackOffice).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsBoincubation).HasColumnName("IsBOIncubation");

                entity.Property(e => e.IsBoprintPalletCode).HasColumnName("IsBOPrintPalletCode");

                entity.Property(e => e.IsBoproductionReceiving).HasColumnName("IsBOProductionReceiving");

                entity.Property(e => e.IsExcessProductionReceiving).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsMobShipping).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsMobileOffice).HasDefaultValueSql("((0))");

                entity.Property(e => e.Password).HasMaxLength(50);

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.SapUserCode).HasMaxLength(50);

                entity.Property(e => e.UserIdAdd).HasColumnName("UserID_Add");

                entity.Property(e => e.UserIdUpdate).HasColumnName("UserID_Update");

                entity.Property(e => e.UserName).HasMaxLength(50);
            });

            modelBuilder.Entity<Version>(entity =>
            {
                entity.Property(e => e.VersionId).HasColumnName("VersionID");

                entity.Property(e => e.Apiversion).HasColumnName("APIVersion");

                entity.Property(e => e.BackendVersion).HasColumnName("backendVersion");

                entity.Property(e => e.FrondendVersion).HasColumnName("frondendVersion");
            });

            modelBuilder.Entity<VwPalletDetail>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_PalletDetails");

                entity.Property(e => e.BatchNo).HasMaxLength(50);

                entity.Property(e => e.DateTimePutAway)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_PutAway");

                entity.Property(e => e.DateTimeWarehouse)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Warehouse");

                entity.Property(e => e.DeviceSerialNoCheckIn)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_CheckIn");

                entity.Property(e => e.DeviceSerialNoCheckOut)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_CheckOut");

                entity.Property(e => e.DeviceSerialNoPutAway)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_PutAway");

                entity.Property(e => e.DeviceSerialNoWarehouse)
                    .HasMaxLength(50)
                    .HasColumnName("DeviceSerialNo_Warehouse");

                entity.Property(e => e.LaneCode).HasMaxLength(50);

                entity.Property(e => e.LaneDesc).HasMaxLength(50);

                entity.Property(e => e.LaneId).HasColumnName("LaneID");

                entity.Property(e => e.OrderDate).HasColumnType("datetime");

                entity.Property(e => e.OrderTypeCode).HasMaxLength(50);

                entity.Property(e => e.OrderTypeDesc).HasMaxLength(50);

                entity.Property(e => e.OrderTypeId).HasColumnName("OrderTypeID");

                entity.Property(e => e.PalletCode).HasMaxLength(50);

                entity.Property(e => e.PalletWipid).HasColumnName("PalletWIPID");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.PlantDesc).HasMaxLength(50);

                entity.Property(e => e.PlantId).HasColumnName("PlantID");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductDesc).HasMaxLength(50);

                entity.Property(e => e.ProductDescAr).HasMaxLength(50);

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.ProductionDate).HasColumnType("datetime");

                entity.Property(e => e.ProductionLineCode).HasMaxLength(50);

                entity.Property(e => e.ProductionLineDesc).HasMaxLength(50);

                entity.Property(e => e.ProductionLineId).HasColumnName("ProductionLineID");

                entity.Property(e => e.ProductionLineNumber).HasMaxLength(50);

                entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderID");

                entity.Property(e => e.ProductionQtyCheckIn).HasColumnName("ProductionQty_CheckIn");

                entity.Property(e => e.ProductionQtyCheckOut).HasColumnName("ProductionQty_CheckOut");

                entity.Property(e => e.SapOrderId).HasColumnName("SapOrderID");

                entity.Property(e => e.StorageLocationCode).HasMaxLength(50);

                entity.Property(e => e.StorageLocationDesc).HasMaxLength(50);

                entity.Property(e => e.StorageLocationId).HasColumnName("StorageLocationID");

                entity.Property(e => e.UserNamePutAway)
                    .HasMaxLength(50)
                    .HasColumnName("UserName_PutAway");

                entity.Property(e => e.UserNameWarehouse)
                    .HasMaxLength(50)
                    .HasColumnName("UserName_Warehouse");
            });

            modelBuilder.Entity<VwProductionOrder>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_ProductionOrder");

                entity.Property(e => e.OrderDate).HasColumnType("datetime");

                entity.Property(e => e.OrderTypeCode).HasMaxLength(50);

                entity.Property(e => e.OrderTypeDesc).HasMaxLength(50);

                entity.Property(e => e.OrderTypeId).HasColumnName("OrderTypeID");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.PlantDesc).HasMaxLength(50);

                entity.Property(e => e.PlantId).HasColumnName("PlantID");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductDesc).HasMaxLength(50);

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderID");

                entity.Property(e => e.SapOrderId).HasColumnName("SapOrderID");
            });

            modelBuilder.Entity<VwPurchaseRequest>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_PurchaseRequest");

                entity.Property(e => e.PlantCodeDest)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_Dest");

                entity.Property(e => e.PlantCodeIssue)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_Issue");

                entity.Property(e => e.PlantDescDest)
                    .HasMaxLength(50)
                    .HasColumnName("PlantDesc_Dest");

                entity.Property(e => e.PlantDescIssue)
                    .HasMaxLength(50)
                    .HasColumnName("PlantDesc_Issue");

                entity.Property(e => e.PlantIdDest).HasColumnName("PlantID_Dest");

                entity.Property(e => e.PlantIdIssue).HasColumnName("PlantID_Issue");

                entity.Property(e => e.Prid).HasColumnName("PRID");

                entity.Property(e => e.Prno)
                    .HasMaxLength(50)
                    .HasColumnName("PRNO");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductDesc).HasMaxLength(50);

                entity.Property(e => e.ProductDescAr).HasMaxLength(50);

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.ShipmentDate).HasMaxLength(50);
            });

            modelBuilder.Entity<VwPurchaseRequisition>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_PurchaseRequisition");

                entity.Property(e => e.DateClosed)
                    .HasColumnType("datetime")
                    .HasColumnName("Date_Closed");

                entity.Property(e => e.DateTimeAdd)
                    .HasColumnType("datetime")
                    .HasColumnName("DateTime_Add");

                entity.Property(e => e.PlantCodeDest)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_Dest");

                entity.Property(e => e.PlantCodeSource)
                    .HasMaxLength(50)
                    .HasColumnName("PlantCode_Source");

                entity.Property(e => e.PlantDescDest)
                    .HasMaxLength(50)
                    .HasColumnName("PlantDesc_Dest");

                entity.Property(e => e.PlantDescSource)
                    .HasMaxLength(50)
                    .HasColumnName("PlantDesc_Source");

                entity.Property(e => e.PlantIdDest).HasColumnName("PlantID_Dest");

                entity.Property(e => e.PlantIdSource).HasColumnName("PlantID_Source");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductDesc).HasMaxLength(50);

                entity.Property(e => e.ProductDescAr).HasMaxLength(50);

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.PurchaseRequisitionDetailId).HasColumnName("PurchaseRequisitionDetailID");

                entity.Property(e => e.PurchaseRequisitionId).HasColumnName("PurchaseRequisitionID");

                entity.Property(e => e.PurchaseRequisitionNo).HasMaxLength(50);

                entity.Property(e => e.PurchaseRequisitionReleaseDate).HasColumnType("datetime");

                entity.Property(e => e.PurchaseRequisitionStatus).HasMaxLength(150);

                entity.Property(e => e.StorageLocationCodeDest)
                    .HasMaxLength(50)
                    .HasColumnName("StorageLocationCode_Dest");

                entity.Property(e => e.StorageLocationCodeSource)
                    .HasMaxLength(50)
                    .HasColumnName("StorageLocationCode_Source");

                entity.Property(e => e.StorageLocationDescDest)
                    .HasMaxLength(50)
                    .HasColumnName("StorageLocationDesc_Dest");

                entity.Property(e => e.StorageLocationDescSource)
                    .HasMaxLength(50)
                    .HasColumnName("StorageLocationDesc_Source");

                entity.Property(e => e.StorageLocationIdDest).HasColumnName("StorageLocationID_Dest");

                entity.Property(e => e.StorageLocationIdSource).HasColumnName("StorageLocationID_Source");

                entity.Property(e => e.Uom)
                    .HasMaxLength(50)
                    .HasColumnName("UOM");

                entity.Property(e => e.UserNameAdd).HasMaxLength(50);

                entity.Property(e => e.UserNameClosed).HasMaxLength(50);
            });

            modelBuilder.Entity<VwStockDetail>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("VW_StockDetails");

                entity.Property(e => e.LaneCode).HasMaxLength(50);

                entity.Property(e => e.LaneDesc).HasMaxLength(50);

                entity.Property(e => e.LaneId).HasColumnName("LaneID");

                entity.Property(e => e.PlantCode).HasMaxLength(50);

                entity.Property(e => e.PlantDesc).HasMaxLength(50);

                entity.Property(e => e.PlantId).HasColumnName("PlantID");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductDesc).HasMaxLength(50);

                entity.Property(e => e.ProductDescAr).HasMaxLength(50);

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.ProductionDate).HasColumnType("date");

                entity.Property(e => e.StockId).HasColumnName("StockID");

                entity.Property(e => e.StockLocationId).HasColumnName("StockLocationID");

                entity.Property(e => e.StorageLocationCode).HasMaxLength(50);

                entity.Property(e => e.StorageLocationDesc).HasMaxLength(50);

                entity.Property(e => e.StorageLocationId).HasColumnName("StorageLocationID");

                entity.Property(e => e.Uom)
                    .HasMaxLength(50)
                    .HasColumnName("UOM");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
