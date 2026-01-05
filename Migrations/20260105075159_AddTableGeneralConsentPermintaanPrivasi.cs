using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableGeneralConsentPermintaanPrivasi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneralConsents",
                columns: table => new
                {
                    GeneralConsentId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    HubunganPasien = table.Column<string>(type: "text", nullable: true),
                    NamaPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    AlamatPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    TipeKamarRawat = table.Column<string>(type: "text", nullable: true),
                    KamarRawat = table.Column<string>(type: "text", nullable: true),
                    TanggalTTD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsMenerimaPanduanRawatInap = table.Column<bool>(type: "boolean", nullable: true),
                    KepalaRuanganId = table.Column<Guid>(type: "uuid", nullable: false),
                    PathTTDKepalaRuangan = table.Column<string>(type: "text", nullable: true),
                    PathTTDPenandaTangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_GeneralConsents", x => x.GeneralConsentId);
                });

            migrationBuilder.CreateTable(
                name: "NilaiKepercayaans",
                columns: table => new
                {
                    NilaiKepercayaanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    Urutan = table.Column<decimal>(type: "numeric", nullable: true),
                    NoRevisi = table.Column<decimal>(type: "numeric", nullable: true),
                    TanggalTTD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NamaPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    TanggalLahirPenandaTangan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UmurPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    GenderPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    AlamatPenandaTangan = table.Column<string>(type: "text", nullable: true),
                    HubDenganPasien = table.Column<string>(type: "text", nullable: true),
                    AgamaPasien = table.Column<string>(type: "text", nullable: true),
                    GenderPasien = table.Column<string>(type: "text", nullable: true),
                    PathLabelPasien = table.Column<string>(type: "text", nullable: true),
                    NilaiBertentangan = table.Column<string>(type: "text", nullable: true),
                    TTDPenandaTanganPath = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_NilaiKepercayaans", x => x.NilaiKepercayaanId);
                });

            migrationBuilder.CreateTable(
                name: "PermintaanPrivasis",
                columns: table => new
                {
                    PermintaanPrivasiId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    Urutan = table.Column<string>(type: "text", nullable: true),
                    NoRevisi = table.Column<string>(type: "text", nullable: true),
                    AksesDiperbolehkan = table.Column<string>(type: "text", nullable: true),
                    PermintaanKhusus = table.Column<string>(type: "text", nullable: true),
                    IsTransportasiPrivasi = table.Column<bool>(type: "boolean", nullable: true),
                    TanggalPermintaan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KepalaRuanganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PathKepalaRuangan = table.Column<string>(type: "text", nullable: true),
                    PathTTDPenandaTangan = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PermintaanPrivasis", x => x.PermintaanPrivasiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralConsents");

            migrationBuilder.DropTable(
                name: "NilaiKepercayaans");

            migrationBuilder.DropTable(
                name: "PermintaanPrivasis");
        }
    }
}
