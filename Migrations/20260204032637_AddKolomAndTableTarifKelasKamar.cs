using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomAndTableTarifKelasKamar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KelasId",
                schema: "public",
                table: "MstKunjungan",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TarifKelasKamars",
                columns: table => new
                {
                    TarifKelasKamarId = table.Column<Guid>(type: "uuid", nullable: false),
                    TarifId = table.Column<Guid>(type: "uuid", nullable: true),
                    KamarId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_TarifKelasKamars", x => x.TarifKelasKamarId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TarifKelasKamars");

            migrationBuilder.DropColumn(
                name: "KelasId",
                schema: "public",
                table: "MstKunjungan");
        }
    }
}
