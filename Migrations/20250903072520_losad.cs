using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class losad : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hrd_PengajuanRekrutmen",
                schema: "public",
                columns: table => new
                {
                    PengajuanRekrutmenId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserActiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: false),
                    TglPengajuan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LokasiPenempatan = table.Column<string>(type: "text", nullable: true),
                    JumlahDibutuhkan = table.Column<int>(type: "integer", nullable: true),
                    JenisKontrak = table.Column<string>(type: "text", nullable: true),
                    TglPerkiraan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusPrioritas = table.Column<string>(type: "text", nullable: true),
                    AlasanPengajuanRekrutmen = table.Column<string>(type: "text", nullable: true),
                    DeskripsiDetail = table.Column<string>(type: "text", nullable: true),
                    DampakPengajuan = table.Column<string>(type: "text", nullable: true),
                    EstimasiKerugian = table.Column<decimal>(type: "numeric", nullable: true),
                    DeskripsiPekerjaan = table.Column<string>(type: "text", nullable: true),
                    KualifikasiUtama = table.Column<string>(type: "text", nullable: true),
                    KualifikasiTambahan = table.Column<string>(type: "text", nullable: true),
                    MinimalPengalaman = table.Column<string>(type: "text", nullable: true),
                    MinimalPendidikan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Hrd_PengajuanRekrutmen", x => x.PengajuanRekrutmenId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hrd_PengajuanRekrutmen",
                schema: "public");
        }
    }
}
