using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomBuatMasterDiskon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDireksiApproved",
                schema: "public",
                table: "Diskon",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDiskonCombined",
                schema: "public",
                table: "Diskon",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KategoriDiskon",
                schema: "public",
                table: "Diskon",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Qty",
                schema: "public",
                table: "Diskon",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipeDiskonDokter",
                schema: "public",
                table: "Diskon",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValueDiskonDokter",
                schema: "public",
                table: "Diskon",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDireksiApproved",
                schema: "public",
                table: "Diskon");

            migrationBuilder.DropColumn(
                name: "IsDiskonCombined",
                schema: "public",
                table: "Diskon");

            migrationBuilder.DropColumn(
                name: "KategoriDiskon",
                schema: "public",
                table: "Diskon");

            migrationBuilder.DropColumn(
                name: "Qty",
                schema: "public",
                table: "Diskon");

            migrationBuilder.DropColumn(
                name: "TipeDiskonDokter",
                schema: "public",
                table: "Diskon");

            migrationBuilder.DropColumn(
                name: "ValueDiskonDokter",
                schema: "public",
                table: "Diskon");
        }
    }
}
