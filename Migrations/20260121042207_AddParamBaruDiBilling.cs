using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddParamBaruDiBilling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoiceBilling",
                schema: "public",
                table: "Billing",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsListWhiteOff",
                schema: "public",
                table: "Billing",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceBilling",
                schema: "public",
                table: "Billing");

            migrationBuilder.DropColumn(
                name: "IsListWhiteOff",
                schema: "public",
                table: "Billing");
        }
    }
}
