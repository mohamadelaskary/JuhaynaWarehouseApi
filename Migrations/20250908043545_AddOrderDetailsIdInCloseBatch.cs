using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBSWarehouse.Migrations
{
    public partial class AddOrderDetailsIdInCloseBatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrderDetailsId",
                table: "CloseBatchs",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderDetailsId",
                table: "CloseBatchs");
        }
    }
}
