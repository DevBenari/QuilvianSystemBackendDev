using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class tidnakanallnew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TindakanKunjungans");

            migrationBuilder.DropTable(
                name: "TindakanPolikliniks");

            migrationBuilder.DropColumn(
                name: "DeskripsiTindakan",
                schema: "public",
                table: "MstTindakan");

            migrationBuilder.AlterColumn<string>(
                name: "NamaTindakan",
                schema: "public",
                table: "MstTindakan",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KodeTindakan",
                schema: "public",
                table: "MstTindakan",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "MstTindakanAsuransi",
                schema: "public",
                columns: table => new
                {
                    TindakanAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MstTindakanAsuransi", x => x.TindakanAsuransiId);
                });

            migrationBuilder.CreateTable(
                name: "MstTindakanPoli",
                schema: "public",
                columns: table => new
                {
                    TindakanPoliId = table.Column<Guid>(type: "uuid", nullable: false),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoliId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MstTindakanPoli", x => x.TindakanPoliId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstTindakanAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTindakanPoli",
                schema: "public");

            migrationBuilder.AlterColumn<string>(
                name: "NamaTindakan",
                schema: "public",
                table: "MstTindakan",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "KodeTindakan",
                schema: "public",
                table: "MstTindakan",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "DeskripsiTindakan",
                schema: "public",
                table: "MstTindakan",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TindakanKunjungans",
                columns: table => new
                {
                    TindakanKunjunganId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Disposition = table.Column<string>(type: "text", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    KelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    KunjunganId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaKelas = table.Column<string>(type: "text", nullable: true),
                    NamaPegawai = table.Column<string>(type: "text", nullable: true),
                    PasienId = table.Column<Guid>(type: "uuid", nullable: true),
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    TarifKelasId = table.Column<Guid>(type: "uuid", nullable: true),
                    TindakanPoliId = table.Column<string>(type: "text", nullable: true),
                    Total = table.Column<decimal>(type: "numeric", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    NamaPoliklinik = table.Column<string>(type: "text", nullable: true),
                    NamaTindakan = table.Column<string>(type: "text", nullable: true),
                    PoliklinikId = table.Column<Guid>(type: "uuid", nullable: true),
                    TindakanId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TindakanPolikliniks", x => x.TindakanPoliklinikId);
                });
        }
    }
}
