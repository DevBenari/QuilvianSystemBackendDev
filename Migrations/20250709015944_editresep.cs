using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editresep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Signa",
                schema: "public",
                table: "MstRacikan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignaTambahan",
                schema: "public",
                table: "MstRacikan",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StatusPengambilan",
                schema: "public",
                table: "Billing",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RacikanDetail",
                schema: "public",
                columns: table => new
                {
                    DetailRacikanId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetailResepId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: true),
                    KomposisiDosis = table.Column<decimal>(type: "numeric", nullable: true),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RacikanDetail", x => x.DetailRacikanId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RacikanDetail",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "Signa",
                schema: "public",
                table: "MstRacikan");

            migrationBuilder.DropColumn(
                name: "SignaTambahan",
                schema: "public",
                table: "MstRacikan");

            migrationBuilder.DropColumn(
                name: "StatusPengambilan",
                schema: "public",
                table: "Billing");
        }
    }
}
