using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsignadorIPs.Migrations
{
    public partial class AddNotificacionesToSoftware : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorreosNotificacion",
                table: "Softwares",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DiasAntesNotificar",
                table: "Softwares",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Notificar",
                table: "Softwares",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorreosNotificacion",
                table: "Softwares");

            migrationBuilder.DropColumn(
                name: "DiasAntesNotificar",
                table: "Softwares");

            migrationBuilder.DropColumn(
                name: "Notificar",
                table: "Softwares");
        }
    }
}
