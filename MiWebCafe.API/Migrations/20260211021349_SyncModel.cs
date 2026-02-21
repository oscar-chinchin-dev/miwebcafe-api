using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiWebCafe.API.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CierreCajaId",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCierre",
                table: "CierresCaja",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "CierresCaja",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MontoFinalDeclarado",
                table: "CierresCaja",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoInicial",
                table: "CierresCaja",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CierreCajaId",
                table: "Ventas",
                column: "CierreCajaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_CierresCaja_CierreCajaId",
                table: "Ventas",
                column: "CierreCajaId",
                principalTable: "CierresCaja",
                principalColumn: "CierreCajaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_CierresCaja_CierreCajaId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_CierreCajaId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "CierreCajaId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "CierresCaja");

            migrationBuilder.DropColumn(
                name: "MontoFinalDeclarado",
                table: "CierresCaja");

            migrationBuilder.DropColumn(
                name: "MontoInicial",
                table: "CierresCaja");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCierre",
                table: "CierresCaja",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
