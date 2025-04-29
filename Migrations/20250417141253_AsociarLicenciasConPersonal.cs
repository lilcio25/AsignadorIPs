using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsignadorIPs.Migrations
{
    public partial class AsociarLicenciasConPersonal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioSoftwares_Usuarios_UsuarioId",
                table: "UsuarioSoftwares");

            migrationBuilder.DropColumn(
                name: "FechaAsignacion",
                table: "UsuarioSoftwares");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "UsuarioSoftwares",
                newName: "PersonalId");

            migrationBuilder.RenameIndex(
                name: "IX_UsuarioSoftwares_UsuarioId",
                table: "UsuarioSoftwares",
                newName: "IX_UsuarioSoftwares_PersonalId");

            migrationBuilder.CreateTable(
                name: "Personal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreCompleto = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personal", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioSoftwares_Personal_PersonalId",
                table: "UsuarioSoftwares",
                column: "PersonalId",
                principalTable: "Personal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioSoftwares_Personal_PersonalId",
                table: "UsuarioSoftwares");

            migrationBuilder.DropTable(
                name: "Personal");

            migrationBuilder.RenameColumn(
                name: "PersonalId",
                table: "UsuarioSoftwares",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_UsuarioSoftwares_PersonalId",
                table: "UsuarioSoftwares",
                newName: "IX_UsuarioSoftwares_UsuarioId");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAsignacion",
                table: "UsuarioSoftwares",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioSoftwares_Usuarios_UsuarioId",
                table: "UsuarioSoftwares",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
