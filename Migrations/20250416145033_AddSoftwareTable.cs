using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsignadorIPs.Migrations
{
    public partial class AddSoftwareTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Softwares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    OrdenCompra = table.Column<string>(type: "TEXT", nullable: false),
                    CantidadLicencias = table.Column<int>(type: "INTEGER", nullable: false),
                    CostoTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    FechaAdquisicion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Softwares", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Softwares");
        }
    }
}
