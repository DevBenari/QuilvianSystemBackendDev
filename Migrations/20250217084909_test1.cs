using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackendDev.Migrations
{
    public partial class test1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDokterPraktek_MstDokter_DokterId",
                schema: "dbo",
                table: "MstDokterPraktek");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKabupatenKota_MstProvinsi_ProvinsiId",
                schema: "dbo",
                table: "MstKabupatenKota");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKecamatan_MstKabupatenKota_KabupatenKotaId",
                schema: "dbo",
                table: "MstKecamatan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKelurahan_MstKecamatan_KecamatanId",
                schema: "dbo",
                table: "MstKelurahan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstProvinsi_MstNegara_NegaraId",
                schema: "dbo",
                table: "MstProvinsi");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDokterPraktek_MstDokter_DokterId",
                schema: "dbo",
                table: "MstDokterPraktek",
                column: "DokterId",
                principalSchema: "dbo",
                principalTable: "MstDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKabupatenKota_MstProvinsi_ProvinsiId",
                schema: "dbo",
                table: "MstKabupatenKota",
                column: "ProvinsiId",
                principalSchema: "dbo",
                principalTable: "MstProvinsi",
                principalColumn: "ProvinsiId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKecamatan_MstKabupatenKota_KabupatenKotaId",
                schema: "dbo",
                table: "MstKecamatan",
                column: "KabupatenKotaId",
                principalSchema: "dbo",
                principalTable: "MstKabupatenKota",
                principalColumn: "KabupatenKotaId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstKelurahan_MstKecamatan_KecamatanId",
                schema: "dbo",
                table: "MstKelurahan",
                column: "KecamatanId",
                principalSchema: "dbo",
                principalTable: "MstKecamatan",
                principalColumn: "KecamatanId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstProvinsi_MstNegara_NegaraId",
                schema: "dbo",
                table: "MstProvinsi",
                column: "NegaraId",
                principalSchema: "dbo",
                principalTable: "MstNegara",
                principalColumn: "NegaraId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDokterPraktek_MstDokter_DokterId",
                schema: "dbo",
                table: "MstDokterPraktek");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKabupatenKota_MstProvinsi_ProvinsiId",
                schema: "dbo",
                table: "MstKabupatenKota");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKecamatan_MstKabupatenKota_KabupatenKotaId",
                schema: "dbo",
                table: "MstKecamatan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstKelurahan_MstKecamatan_KecamatanId",
                schema: "dbo",
                table: "MstKelurahan");

            migrationBuilder.DropForeignKey(
                name: "FK_MstProvinsi_MstNegara_NegaraId",
                schema: "dbo",
                table: "MstProvinsi");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstDokterPraktek_MstDokter_DokterId",
                schema: "dbo",
                table: "MstDokterPraktek",
                column: "DokterId",
                principalSchema: "dbo",
                principalTable: "MstDokter",
                principalColumn: "DokterId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKabupatenKota_MstProvinsi_ProvinsiId",
                schema: "dbo",
                table: "MstKabupatenKota",
                column: "ProvinsiId",
                principalSchema: "dbo",
                principalTable: "MstProvinsi",
                principalColumn: "ProvinsiId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKecamatan_MstKabupatenKota_KabupatenKotaId",
                schema: "dbo",
                table: "MstKecamatan",
                column: "KabupatenKotaId",
                principalSchema: "dbo",
                principalTable: "MstKabupatenKota",
                principalColumn: "KabupatenKotaId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstKelurahan_MstKecamatan_KecamatanId",
                schema: "dbo",
                table: "MstKelurahan",
                column: "KecamatanId",
                principalSchema: "dbo",
                principalTable: "MstKecamatan",
                principalColumn: "KecamatanId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstProvinsi_MstNegara_NegaraId",
                schema: "dbo",
                table: "MstProvinsi",
                column: "NegaraId",
                principalSchema: "dbo",
                principalTable: "MstNegara",
                principalColumn: "NegaraId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
