using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTablePelunasanDeposit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PelunasanDeposits",
                columns: table => new
                {
                    PelunasanDepositId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    Urutan = table.Column<decimal>(type: "numeric", nullable: true),
                    NoRevisi = table.Column<decimal>(type: "numeric", nullable: true),
                    TanggalTTD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NamaPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    AlamatPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    TelpPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    TanggalJatuhTempo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TTDPenandaTanganPath = table.Column<string>(type: "text", nullable: true),
                    PetugasId = table.Column<Guid>(type: "uuid", nullable: true),
                    PathTTDPetugas = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PelunasanDeposits", x => x.PelunasanDepositId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PelunasanDeposits");
        }
    }
}
