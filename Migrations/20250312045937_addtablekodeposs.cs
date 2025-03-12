using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class addtablekodeposs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NamaDokter",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropColumn(
                name: "SubPoliId",
                schema: "public",
                table: "MstJadwalPraktek");

            migrationBuilder.DropColumn(
                name: "NamaDokter",
                table: "DokterPolis");

            migrationBuilder.DropColumn(
                name: "NamaPoliKlinik",
                table: "DokterPolis");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamMulai",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.CreateTable(
                name: "MstKodePos",
                schema: "public",
                columns: table => new
                {
                    KodePosId = table.Column<Guid>(type: "uuid", nullable: false),
                    UniqueKodePos = table.Column<string>(type: "text", nullable: false),
                    NamaKodePos = table.Column<string>(type: "text", nullable: false),
                    KelurahanId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstKodePos", x => x.KodePosId);
                    table.ForeignKey(
                        name: "FK_MstKodePos_MstKelurahan_KelurahanId",
                        column: x => x.KelurahanId,
                        principalSchema: "public",
                        principalTable: "MstKelurahan",
                        principalColumn: "KelurahanId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstKodePos_KelurahanId",
                schema: "public",
                table: "MstKodePos",
                column: "KelurahanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstKodePos",
                schema: "public");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamMulai",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "JamBerakhir",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaDokter",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SubPoliId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaDokter",
                table: "DokterPolis",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NamaPoliKlinik",
                table: "DokterPolis",
                type: "text",
                nullable: true);
        }
    }
}
