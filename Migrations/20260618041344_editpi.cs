using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class editpi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "Fin_PurchasingInvoice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "Fin_PurchasingInvoice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "public",
                table: "Fin_PurchasingInvoice",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "public",
                table: "Fin_PurchasingInvoice",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "Fin_PurchasingInvoice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreateBy",
                schema: "public",
                table: "Fin_ItemPurchasingInvoice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "Fin_ItemPurchasingInvoice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeleteDateTime",
                schema: "public",
                table: "Fin_ItemPurchasingInvoice",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "Fin_ItemPurchasingInvoice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "public",
                table: "Fin_PurchasingInvoice");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "public",
                table: "Fin_PurchasingInvoice");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "public",
                table: "Fin_PurchasingInvoice");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "Fin_PurchasingInvoice");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "public",
                table: "Fin_PurchasingInvoice");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                schema: "public",
                table: "Fin_ItemPurchasingInvoice");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "public",
                table: "Fin_ItemPurchasingInvoice");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "public",
                table: "Fin_ItemPurchasingInvoice");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "public",
                table: "Fin_ItemPurchasingInvoice");
        }
    }
}
