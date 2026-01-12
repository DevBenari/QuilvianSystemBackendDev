using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTabelHandoverPasienandDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HandoverPasienDetails",
                columns: table => new
                {
                    DetailHandoverPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    HandoverPasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsSudah = table.Column<bool>(type: "boolean", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_HandoverPasienDetails", x => x.DetailHandoverPasienId);
                });

            migrationBuilder.CreateTable(
                name: "HandoverPasiens",
                columns: table => new
                {
                    HandoverPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TanggalSerahTerima = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AdministrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CROId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerawatId = table.Column<Guid>(type: "uuid", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_HandoverPasiens", x => x.HandoverPasienId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HandoverPasienDetails");

            migrationBuilder.DropTable(
                name: "HandoverPasiens");
        }
    }
}
