using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class AddKolomIntakeOutakeDiObservasiCairan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CairanMasuk: text -> numeric
            migrationBuilder.Sql(@"
        ALTER TABLE ""ObservasiCairans""
        ALTER COLUMN ""CairanMasuk"" TYPE numeric
        USING CASE
            WHEN ""CairanMasuk"" IS NULL THEN NULL
            WHEN btrim(""CairanMasuk"") = '' THEN NULL
            WHEN replace(btrim(""CairanMasuk""), ',', '.') ~ '^[+-]?\d+(\.\d+)?$'
                THEN replace(btrim(""CairanMasuk""), ',', '.')::numeric
            ELSE NULL
        END;
    ");

            // CairanKeluar: text -> numeric
            migrationBuilder.Sql(@"
        ALTER TABLE ""ObservasiCairans""
        ALTER COLUMN ""CairanKeluar"" TYPE numeric
        USING CASE
            WHEN ""CairanKeluar"" IS NULL THEN NULL
            WHEN btrim(""CairanKeluar"") = '' THEN NULL
            WHEN replace(btrim(""CairanKeluar""), ',', '.') ~ '^[+-]?\d+(\.\d+)?$'
                THEN replace(btrim(""CairanKeluar""), ',', '.')::numeric
            ELSE NULL
        END;
    ");

            migrationBuilder.AddColumn<string>(
                name: "Intake",
                table: "ObservasiCairans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Outake",
                table: "ObservasiCairans",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Intake",
                table: "ObservasiCairans");

            migrationBuilder.DropColumn(
                name: "Outake",
                table: "ObservasiCairans");

            migrationBuilder.AlterColumn<string>(
                name: "CairanMasuk",
                table: "ObservasiCairans",
                type: "text",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CairanKeluar",
                table: "ObservasiCairans",
                type: "text",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }
    }
}
