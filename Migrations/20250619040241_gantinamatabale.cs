using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class gantinamatabale : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDetailResep",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "MstResepDetail",
                schema: "public",
                columns: table => new
                {
                    DetailResepId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResepId = table.Column<Guid>(type: "uuid", nullable: true),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaAsuransi = table.Column<string>(type: "text", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: true),
                    Signa = table.Column<string>(type: "text", nullable: true),
                    SignaTambahan = table.Column<string>(type: "text", nullable: true),
                    InteraturObat = table.Column<string>(type: "text", nullable: true),
                    JenisObat = table.Column<string>(type: "text", nullable: true),
                    HargaObat = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalHargaObat = table.Column<decimal>(type: "numeric", nullable: true),
                    StatusCoverObat = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_MstResepDetail", x => x.DetailResepId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstResepDetail",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "MstDetailResep",
                schema: "public",
                columns: table => new
                {
                    DetailResepId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsuransiId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    HargaObat = table.Column<decimal>(type: "numeric", nullable: true),
                    InteraturObat = table.Column<string>(type: "text", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    JenisObat = table.Column<string>(type: "text", nullable: true),
                    NamaAsuransi = table.Column<string>(type: "text", nullable: true),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: true),
                    ResepId = table.Column<Guid>(type: "uuid", nullable: true),
                    Signa = table.Column<string>(type: "text", nullable: true),
                    SignaTambahan = table.Column<string>(type: "text", nullable: true),
                    StatusCoverObat = table.Column<bool>(type: "boolean", nullable: true),
                    TotalHargaObat = table.Column<decimal>(type: "numeric", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstDetailResep", x => x.DetailResepId);
                });
        }
    }
}
