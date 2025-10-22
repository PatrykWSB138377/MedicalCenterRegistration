using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCenterRegistration.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Visit",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Visit");
        }
    }
}
