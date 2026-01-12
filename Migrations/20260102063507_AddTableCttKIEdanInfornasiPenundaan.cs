using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableCttKIEdanInfornasiPenundaan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DepositRanap",
                schema: "public",
                table: "MstKunjungan",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CatatanKIEs",
                columns: table => new
                {
                    CatKIEId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TanggalCatat = table.Column<Guid>(type: "uuid", nullable: true),
                    PenjelasanKIE = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_CatatanKIEs", x => x.CatKIEId);
                });

            migrationBuilder.CreateTable(
                name: "InformasiPenundaans",
                columns: table => new
                {
                    InfoPenundaanId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    TanggalInfoTunda = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Keterangan = table.Column<Guid>(type: "uuid", nullable: true),
                    PerawatId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_InformasiPenundaans", x => x.InfoPenundaanId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatatanKIEs");

            migrationBuilder.DropTable(
                name: "InformasiPenundaans");

            migrationBuilder.DropColumn(
                name: "DepositRanap",
                schema: "public",
                table: "MstKunjungan");
        }
    }
}
