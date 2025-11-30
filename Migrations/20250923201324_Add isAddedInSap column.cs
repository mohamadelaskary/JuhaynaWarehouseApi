using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBSWarehouse.Migrations
{
    public partial class AddisAddedInSapcolumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderDetailsId",
                table: "CloseBatchs");

            migrationBuilder.AddColumn<bool>(
                name: "IsAddedInSap",
                table: "ProductionOrder_Receiving",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAddedInSap",
                table: "ProductionOrder_Receiving");

            migrationBuilder.AddColumn<long>(
                name: "OrderDetailsId",
                table: "CloseBatchs",
                type: "bigint",
                nullable: true);
        }
    }
}
