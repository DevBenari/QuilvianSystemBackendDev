using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editasuransi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "DokterId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "TanggalMulaiKerjasama",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TanggalAkhirKerjasama",
                schema: "public",
                table: "MstAsuransi",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "DokterId",
                schema: "public",
                table: "MstJadwalPraktek",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TanggalMulaiKerjasama",
                schema: "public",
                table: "MstAsuransi",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TanggalAkhirKerjasama",
                schema: "public",
                table: "MstAsuransi",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
