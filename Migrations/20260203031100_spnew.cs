using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class spnew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Telepon",
                schema: "public",
                table: "MstSupplier");

            migrationBuilder.RenameColumn(
                name: "Ppn",
                schema: "public",
                table: "MstSupplier",
                newName: "PPN");

            migrationBuilder.RenameColumn(
                name: "TermOfPaymentName",
                schema: "public",
                table: "MstSupplier",
                newName: "TermOfPayment");

            migrationBuilder.RenameColumn(
                name: "TermOfPaymentId",
                schema: "public",
                table: "MstSupplier",
                newName: "BankId");

            migrationBuilder.RenameColumn(
                name: "KhususUnit",
                schema: "public",
                table: "MstSupplier",
                newName: "PhoneNumber");

            migrationBuilder.AlterColumn<decimal>(
                name: "PPN",
                schema: "public",
                table: "MstSupplier",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "public",
                table: "MstSupplier",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContactPerson",
                schema: "public",
                table: "MstSupplier",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                schema: "public",
                table: "MstSupplier",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "AccountHolderName",
                schema: "public",
                table: "MstSupplier",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBloodBankSupplier",
                schema: "public",
                table: "MstSupplier",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFullPaid",
                schema: "public",
                table: "MstSupplier",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeadTime",
                schema: "public",
                table: "MstSupplier",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoRekening",
                schema: "public",
                table: "MstSupplier",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                schema: "public",
                table: "MstSupplier",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountHolderName",
                schema: "public",
                table: "MstSupplier");

            migrationBuilder.DropColumn(
                name: "IsBloodBankSupplier",
                schema: "public",
                table: "MstSupplier");

            migrationBuilder.DropColumn(
                name: "IsFullPaid",
                schema: "public",
                table: "MstSupplier");

            migrationBuilder.DropColumn(
                name: "LeadTime",
                schema: "public",
                table: "MstSupplier");

            migrationBuilder.DropColumn(
                name: "NoRekening",
                schema: "public",
                table: "MstSupplier");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                schema: "public",
                table: "MstSupplier");

            migrationBuilder.RenameColumn(
                name: "PPN",
                schema: "public",
                table: "MstSupplier",
                newName: "Ppn");

            migrationBuilder.RenameColumn(
                name: "TermOfPayment",
                schema: "public",
                table: "MstSupplier",
                newName: "TermOfPaymentName");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                schema: "public",
                table: "MstSupplier",
                newName: "KhususUnit");

            migrationBuilder.RenameColumn(
                name: "BankId",
                schema: "public",
                table: "MstSupplier",
                newName: "TermOfPaymentId");

            migrationBuilder.AlterColumn<int>(
                name: "Ppn",
                schema: "public",
                table: "MstSupplier",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "public",
                table: "MstSupplier",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactPerson",
                schema: "public",
                table: "MstSupplier",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                schema: "public",
                table: "MstSupplier",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telepon",
                schema: "public",
                table: "MstSupplier",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
