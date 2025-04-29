using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsignadorIPs.Migrations
{
    public partial class AddActivoToSoftware : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Softwares",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Softwares");
        }
    }
}
