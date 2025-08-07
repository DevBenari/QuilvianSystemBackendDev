using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddParamSoapAndPerawt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RanapId",
                schema: "public",
                table: "MstSOAP");

            migrationBuilder.AddColumn<Guid>(
                name: "DiagnosaSDKIId",
                schema: "public",
                table: "PerawatSubjective",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DiagnosaSDKIId",
                schema: "public",
                table: "PerawatObejctive",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Evaluasi",
                schema: "public",
                table: "MstSOAP",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Intervensi",
                schema: "public",
                table: "MstSOAP",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reevaluasi",
                schema: "public",
                table: "MstSOAP",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PerawatIntervensis",
                columns: table => new
                {
                    IntervensiId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiagnosaSDKIId = table.Column<Guid>(type: "uuid", nullable: true),
                    NamaIntervensi = table.Column<string>(type: "text", nullable: true),
                    TipeIntervensi = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_PerawatIntervensis", x => x.IntervensiId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerawatIntervensis");

            migrationBuilder.DropColumn(
                name: "DiagnosaSDKIId",
                schema: "public",
                table: "PerawatSubjective");

            migrationBuilder.DropColumn(
                name: "DiagnosaSDKIId",
                schema: "public",
                table: "PerawatObejctive");

            migrationBuilder.DropColumn(
                name: "Evaluasi",
                schema: "public",
                table: "MstSOAP");

            migrationBuilder.DropColumn(
                name: "Intervensi",
                schema: "public",
                table: "MstSOAP");

            migrationBuilder.DropColumn(
                name: "Reevaluasi",
                schema: "public",
                table: "MstSOAP");

            migrationBuilder.AddColumn<Guid>(
                name: "RanapId",
                schema: "public",
                table: "MstSOAP",
                type: "uuid",
                nullable: true);
        }
    }
}
