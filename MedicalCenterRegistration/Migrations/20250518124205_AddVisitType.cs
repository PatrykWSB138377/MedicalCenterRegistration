using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCenterRegistration.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VisitType",
                table: "Visit",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisitType",
                table: "Visit");
        }
    }
}
