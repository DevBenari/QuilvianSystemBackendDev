using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableTarifKelasAsuransi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Diskon",
                schema: "public",
                table: "MstTindakanAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Diskon",
                schema: "public",
                table: "MstObatAsuransi",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstPemeriksaanAsuransi",
                schema: "public",
                columns: table => new
                {
                    PemeriksaanLabAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    PemeriksaanLabId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    Diskon = table.Column<decimal>(type: "numeric", nullable: true),
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
                    table.PrimaryKey("PK_MstPemeriksaanAsuransi", x => x.PemeriksaanLabAsuransiId);
                });

            migrationBuilder.CreateTable(
                name: "MstTarifKelasAsuransi",
                schema: "public",
                columns: table => new
                {
                    TarifKelasAsuransiId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    TarifKelasId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstTarifKelasAsuransi", x => x.TarifKelasAsuransiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPemeriksaanAsuransi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTarifKelasAsuransi",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "Diskon",
                schema: "public",
                table: "MstTindakanAsuransi");

            migrationBuilder.DropColumn(
                name: "Diskon",
                schema: "public",
                table: "MstObatAsuransi");
        }
    }
}
