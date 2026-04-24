using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddCatatanDiet2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatatanDietDetail_CatatanDiets_CatatanDietId",
                table: "CatatanDietDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatatanDietDetail",
                table: "CatatanDietDetail");

            migrationBuilder.RenameTable(
                name: "CatatanDietDetail",
                newName: "CatatanDietDetails");

            migrationBuilder.RenameIndex(
                name: "IX_CatatanDietDetail_CatatanDietId",
                table: "CatatanDietDetails",
                newName: "IX_CatatanDietDetails_CatatanDietId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TglCatatanDiet",
                table: "CatatanDiets",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "Icd10Id",
                table: "CatatanDietDetails",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "CatatanDietId",
                table: "CatatanDietDetails",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                table: "CatatanDietDetails",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDateTime",
                table: "CatatanDietDetails",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                table: "CatatanDietDetails",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                table: "CatatanDietDetails",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "CatatanDietDetails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                table: "CatatanDietDetails",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                table: "CatatanDietDetails",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatatanDietDetails",
                table: "CatatanDietDetails",
                column: "CatatanDietDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatatanDietDetails_CatatanDiets_CatatanDietId",
                table: "CatatanDietDetails",
                column: "CatatanDietId",
                principalTable: "CatatanDiets",
                principalColumn: "CatatanDietId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatatanDietDetails_CatatanDiets_CatatanDietId",
                table: "CatatanDietDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CatatanDietDetails",
                table: "CatatanDietDetails");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "CatatanDietDetails");

            migrationBuilder.DropColumn(
                name: "CreateDateTime",
                table: "CatatanDietDetails");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                table: "CatatanDietDetails");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                table: "CatatanDietDetails");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "CatatanDietDetails");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                table: "CatatanDietDetails");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                table: "CatatanDietDetails");

            migrationBuilder.RenameTable(
                name: "CatatanDietDetails",
                newName: "CatatanDietDetail");

            migrationBuilder.RenameIndex(
                name: "IX_CatatanDietDetails_CatatanDietId",
                table: "CatatanDietDetail",
                newName: "IX_CatatanDietDetail_CatatanDietId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TglCatatanDiet",
                table: "CatatanDiets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Icd10Id",
                table: "CatatanDietDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CatatanDietId",
                table: "CatatanDietDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CatatanDietDetail",
                table: "CatatanDietDetail",
                column: "CatatanDietDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatatanDietDetail_CatatanDiets_CatatanDietId",
                table: "CatatanDietDetail",
                column: "CatatanDietId",
                principalTable: "CatatanDiets",
                principalColumn: "CatatanDietId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
