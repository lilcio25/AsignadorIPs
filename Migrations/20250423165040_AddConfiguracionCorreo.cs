using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsignadorIPs.Migrations
{
    public partial class AddConfiguracionCorreo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Softwares");

            migrationBuilder.CreateTable(
                name: "ConfiguracionCorreo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Remitente = table.Column<string>(type: "TEXT", nullable: false),
                    HostSMTP = table.Column<string>(type: "TEXT", nullable: false),
                    PuertoSMTP = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioSMTP = table.Column<string>(type: "TEXT", nullable: false),
                    ClaveSMTP = table.Column<string>(type: "TEXT", nullable: false),
                    UsarSSL = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionCorreo", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionCorreo");

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Softwares",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
