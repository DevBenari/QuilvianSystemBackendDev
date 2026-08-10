using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class glupdtae : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExchangeRateId",
                schema: "public",
                table: "Fin_GLHeader",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MataUangId",
                schema: "public",
                table: "Fin_GLHeader",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaMataUang",
                schema: "public",
                table: "Fin_GLHeader",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RateToIdr",
                schema: "public",
                table: "Fin_GLHeader",
                type: "numeric(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TempRJId",
                schema: "public",
                table: "Fin_GLHeader",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnbalanceAmount",
                schema: "public",
                table: "Fin_GLHeader",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DetailTempRJId",
                schema: "public",
                table: "Fin_GLDetail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "RoleSetupCOA",
                schema: "public",
                table: "Fin_GLDetail",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExchangeRateId",
                schema: "public",
                table: "Fin_GLHeader");

            migrationBuilder.DropColumn(
                name: "MataUangId",
                schema: "public",
                table: "Fin_GLHeader");

            migrationBuilder.DropColumn(
                name: "NamaMataUang",
                schema: "public",
                table: "Fin_GLHeader");

            migrationBuilder.DropColumn(
                name: "RateToIdr",
                schema: "public",
                table: "Fin_GLHeader");

            migrationBuilder.DropColumn(
                name: "TempRJId",
                schema: "public",
                table: "Fin_GLHeader");

            migrationBuilder.DropColumn(
                name: "UnbalanceAmount",
                schema: "public",
                table: "Fin_GLHeader");

            migrationBuilder.DropColumn(
                name: "DetailTempRJId",
                schema: "public",
                table: "Fin_GLDetail");

            migrationBuilder.DropColumn(
                name: "RoleSetupCOA",
                schema: "public",
                table: "Fin_GLDetail");
        }
    }
}
