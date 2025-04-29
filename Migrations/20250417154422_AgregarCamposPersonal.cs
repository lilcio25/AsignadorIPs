using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsignadorIPs.Migrations
{
    public partial class AgregarCamposPersonal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoFuncionario",
                table: "Personal",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CorreoFuncionario",
                table: "Personal",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoFuncionario",
                table: "Personal");

            migrationBuilder.DropColumn(
                name: "CorreoFuncionario",
                table: "Personal");
        }
    }
}
