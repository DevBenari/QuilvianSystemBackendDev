using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddTableMstItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NamaBentukObat",
                schema: "public",
                table: "MstRacikanAddon",
                newName: "NamaBentukSatuan");

            migrationBuilder.RenameColumn(
                name: "BentukObatId",
                schema: "public",
                table: "MstRacikanAddon",
                newName: "BentukSatuanId");

            migrationBuilder.RenameColumn(
                name: "NamaBentukObat",
                schema: "public",
                table: "MstBentukObat",
                newName: "NamaBentukSatuan");

            migrationBuilder.RenameColumn(
                name: "KodeBentukObat",
                schema: "public",
                table: "MstBentukObat",
                newName: "KodeBentukSatuan");

            migrationBuilder.RenameColumn(
                name: "BentukObatId",
                schema: "public",
                table: "MstBentukObat",
                newName: "BentukSatuanId");

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    KodeItem = table.Column<string>(type: "text", nullable: true),
                    NamaItem = table.Column<string>(type: "text", nullable: true),
                    GenericName = table.Column<string>(type: "text", nullable: true),
                    KategoriItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    BentukSatuanId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                });

            migrationBuilder.CreateTable(
                name: "MstItemKategori",
                schema: "public",
                columns: table => new
                {
                    KategoriItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaKategoriItem = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstItemKategori", x => x.KategoriItemId);
                });

            migrationBuilder.CreateTable(
                name: "MstObatRute",
                schema: "public",
                columns: table => new
                {
                    RuteObatId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuteObat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstObatRute", x => x.RuteObatId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "MstItemKategori",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstObatRute",
                schema: "public");

            migrationBuilder.RenameColumn(
                name: "NamaBentukSatuan",
                schema: "public",
                table: "MstRacikanAddon",
                newName: "NamaBentukObat");

            migrationBuilder.RenameColumn(
                name: "BentukSatuanId",
                schema: "public",
                table: "MstRacikanAddon",
                newName: "BentukObatId");

            migrationBuilder.RenameColumn(
                name: "NamaBentukSatuan",
                schema: "public",
                table: "MstBentukObat",
                newName: "NamaBentukObat");

            migrationBuilder.RenameColumn(
                name: "KodeBentukSatuan",
                schema: "public",
                table: "MstBentukObat",
                newName: "KodeBentukObat");

            migrationBuilder.RenameColumn(
                name: "BentukSatuanId",
                schema: "public",
                table: "MstBentukObat",
                newName: "BentukObatId");
        }
    }
}
