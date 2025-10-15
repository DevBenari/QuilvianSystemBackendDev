using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTabelTransferPasien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstSpecimen",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SpecimenPemeriksaans");

            migrationBuilder.RenameColumn(
                name: "SpecimenId",
                table: "SpecimenJeniss",
                newName: "AsalSpecimenId");

            migrationBuilder.RenameColumn(
                name: "SpecimenTestId",
                table: "LabBookingDetails",
                newName: "AsalSpecimenId");

            migrationBuilder.AddColumn<Guid>(
                name: "AsalSpecimenId",
                table: "SpecimenMethods",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRawatInap",
                schema: "public",
                table: "MstTindakan",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KunjunganId",
                table: "DarahPermintaans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PasienId",
                table: "DarahPermintaans",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstSpecimenAsal",
                schema: "public",
                columns: table => new
                {
                    SpecimenAsalId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsalSpecimen = table.Column<string>(type: "text", nullable: true),
                    KodeAsalSpecimen = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstSpecimenAsal", x => x.SpecimenAsalId);
                });

            migrationBuilder.CreateTable(
                name: "TransferPasienDetails",
                columns: table => new
                {
                    DetailTransferPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    PemeriksaanLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransferPasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabId = table.Column<Guid>(type: "uuid", nullable: true),
                    PenggunaanAlat = table.Column<string>(type: "text", nullable: true),
                    TglPasang = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TglPemeriksaanLab = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JumlahPemeriksaanLab = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_TransferPasienDetails", x => x.DetailTransferPasienId);
                });

            migrationBuilder.CreateTable(
                name: "TransferPasiens",
                columns: table => new
                {
                    TransferPasienId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    KamarId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosaUtama = table.Column<string>(type: "text", nullable: true),
                    DiagnosaSekunder = table.Column<string>(type: "text", nullable: true),
                    DokterId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId2 = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId3 = table.Column<Guid>(type: "uuid", nullable: true),
                    IndikasiRanap = table.Column<string>(type: "text", nullable: true),
                    IsAlergic = table.Column<bool>(type: "boolean", nullable: true),
                    AlergicOf = table.Column<string>(type: "text", nullable: true),
                    AlasanPindahPasien = table.Column<string>(type: "text", nullable: true),
                    TglPindah = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PengawasanHarianId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObservasiCairanId = table.Column<Guid>(type: "uuid", nullable: true),
                    IndikatorPengkajianId = table.Column<Guid>(type: "uuid", nullable: true),
                    PemberianObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalScoreAldrete = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalScoreSteward = table.Column<decimal>(type: "numeric", nullable: true),
                    IsICU = table.Column<bool>(type: "boolean", nullable: true),
                    BarangDiserahkan = table.Column<string>(type: "text", nullable: true),
                    IntervensiPerawat = table.Column<string>(type: "text", nullable: true),
                    PlanningTindakan = table.Column<string>(type: "text", nullable: true),
                    TTDMenyerahkanPath = table.Column<string>(type: "text", nullable: true),
                    TTDMenyerahkanId = table.Column<Guid>(type: "uuid", nullable: true),
                    TTDMengetahuiPath = table.Column<string>(type: "text", nullable: true),
                    TTDMengetahuiId = table.Column<Guid>(type: "uuid", nullable: true),
                    TTDPenerimaPath = table.Column<string>(type: "text", nullable: true),
                    TTDPenerimaId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_TransferPasiens", x => x.TransferPasienId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstSpecimenAsal",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TransferPasienDetails");

            migrationBuilder.DropTable(
                name: "TransferPasiens");

            migrationBuilder.DropColumn(
                name: "AsalSpecimenId",
                table: "SpecimenMethods");

            migrationBuilder.DropColumn(
                name: "IsRawatInap",
                schema: "public",
                table: "MstTindakan");

            migrationBuilder.DropColumn(
                name: "KunjunganId",
                table: "DarahPermintaans");

            migrationBuilder.DropColumn(
                name: "PasienId",
                table: "DarahPermintaans");

            migrationBuilder.RenameColumn(
                name: "AsalSpecimenId",
                table: "SpecimenJeniss",
                newName: "SpecimenId");

            migrationBuilder.RenameColumn(
                name: "AsalSpecimenId",
                table: "LabBookingDetails",
                newName: "SpecimenTestId");

            migrationBuilder.CreateTable(
                name: "MstSpecimen",
                schema: "public",
                columns: table => new
                {
                    SpecimenId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    KodeSpecimen = table.Column<string>(type: "text", nullable: true),
                    NamaSpecimen = table.Column<string>(type: "text", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstSpecimen", x => x.SpecimenId);
                });

            migrationBuilder.CreateTable(
                name: "SpecimenPemeriksaans",
                columns: table => new
                {
                    SpecimenPemeriksaanId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    JenisSpecimenId = table.Column<Guid>(type: "uuid", nullable: true),
                    Keterangan = table.Column<string>(type: "text", nullable: true),
                    KodeSpecimenTest = table.Column<string>(type: "text", nullable: true),
                    PemeriksaanSpecimen = table.Column<string>(type: "text", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecimenPemeriksaans", x => x.SpecimenPemeriksaanId);
                });
        }
    }
}
