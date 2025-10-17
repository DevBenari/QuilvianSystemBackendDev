using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtablepegawai : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pegawai",
                schema: "public",
                columns: table => new
                {
                    PegawaiId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoIdentitas = table.Column<string>(type: "text", nullable: true),
                    NamaLengkap = table.Column<string>(type: "text", nullable: true),
                    PinPegawai = table.Column<string>(type: "text", nullable: true),
                    JenisKelamin = table.Column<string>(type: "text", nullable: true),
                    TempatLahir = table.Column<string>(type: "text", nullable: true),
                    TanggalLahir = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Alamat = table.Column<string>(type: "text", nullable: true),
                    ProvinsiId = table.Column<Guid>(type: "uuid", nullable: true),
                    KabupatenKotaId = table.Column<Guid>(type: "uuid", nullable: true),
                    KecamatanId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelurahanId = table.Column<Guid>(type: "uuid", nullable: true),
                    NomorTelepon = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Kewarganegaraan = table.Column<string>(type: "text", nullable: true),
                    AgamaId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPerawat = table.Column<bool>(type: "boolean", nullable: true),
                    NoSTR = table.Column<string>(type: "text", nullable: true),
                    StatusPegawai = table.Column<string>(type: "text", nullable: true),
                    JenisPegawai = table.Column<string>(type: "text", nullable: true),
                    PosisiId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartementId = table.Column<Guid>(type: "uuid", nullable: true),
                    PendidikanId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsAktif = table.Column<bool>(type: "boolean", nullable: true),
                    TglMasuk = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglKeluar = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglAwalKontrak = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglAkhirKontrak = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Pegawai", x => x.PegawaiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pegawai",
                schema: "public");
        }
    }
}
