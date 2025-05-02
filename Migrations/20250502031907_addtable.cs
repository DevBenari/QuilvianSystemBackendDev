using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstTarifTindakan",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "KategoriTindakan",
                schema: "public",
                table: "MstTindakan");

            migrationBuilder.AddColumn<string>(
                name: "Fungsi",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZatAktif",
                schema: "public",
                table: "MstObat",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstTarifKelas",
                schema: "public",
                columns: table => new
                {
                    TarifTindakanId = table.Column<Guid>(type: "uuid", nullable: false),
                    TindakanPoliId = table.Column<Guid>(type: "uuid", nullable: true),
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaPoliklinik = table.Column<string>(type: "text", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaKelas = table.Column<string>(type: "text", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_MstTarifKelas", x => x.TarifTindakanId);
                });

            migrationBuilder.CreateTable(
                name: "TindakanKunjungans",
                columns: table => new
                {
                    TindakanKunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaKelas = table.Column<string>(type: "text", nullable: true),
                    TarifKelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TindakanPoliId = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Total = table.Column<decimal>(type: "numeric", nullable: true),
                    Disposition = table.Column<string>(type: "text", nullable: true),
                    NamaPegawai = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_TindakanKunjungans", x => x.TindakanKunjunganId);
                });

            migrationBuilder.CreateTable(
                name: "TindakanPolikliniks",
                columns: table => new
                {
                    TindakanPoliklinikId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: true),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaTindakan = table.Column<string>(type: "text", nullable: true),
                    NamaPoliklinik = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_TindakanPolikliniks", x => x.TindakanPoliklinikId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstTarifKelas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TindakanKunjungans");

            migrationBuilder.DropTable(
                name: "TindakanPolikliniks");

            migrationBuilder.DropColumn(
                name: "Fungsi",
                schema: "public",
                table: "MstObat");

            migrationBuilder.DropColumn(
                name: "ZatAktif",
                schema: "public",
                table: "MstObat");

            migrationBuilder.AddColumn<string>(
                name: "KategoriTindakan",
                schema: "public",
                table: "MstTindakan",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstTarifTindakan",
                schema: "public",
                columns: table => new
                {
                    TarifTindakanId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    KSO = table.Column<decimal>(type: "numeric", nullable: true),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaKelas = table.Column<string>(type: "text", nullable: true),
                    NamaPoliklinik = table.Column<string>(type: "text", nullable: true),
                    NamaTindakan = table.Column<string>(type: "text", nullable: true),
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifBahp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifDokter = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifJp = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifLain = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifRs = table.Column<decimal>(type: "numeric", nullable: true),
                    TarifTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstTarifTindakan", x => x.TarifTindakanId);
                });
        }
    }
}
