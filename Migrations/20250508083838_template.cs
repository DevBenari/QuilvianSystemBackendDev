using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class template : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Umur",
                schema: "public",
                table: "MstAsuransiPasien",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MstResepTemplate",
                schema: "public",
                columns: table => new
                {
                    ResepTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObatId = table.Column<Guid>(type: "uuid", nullable: true),
                    KodeResepTemplate = table.Column<string>(type: "text", nullable: true),
                    Judul = table.Column<string>(type: "text", nullable: true),
                    DokterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: true),
                    Signa = table.Column<string>(type: "text", nullable: true),
                    SignaTambahan = table.Column<string>(type: "text", nullable: true),
                    InteraturObat = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_MstResepTemplate", x => x.ResepTemplateId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstResepTemplate",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "Umur",
                schema: "public",
                table: "MstAsuransiPasien");
        }
    }
}
