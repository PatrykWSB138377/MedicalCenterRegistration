using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCenterRegistration.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorDataAndImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DateOfBirth",
                table: "Doctor",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "Doctor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sex",
                table: "Doctor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Image",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Base64Data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Image", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_ImageId",
                table: "Doctor",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctor_Image_ImageId",
                table: "Doctor",
                column: "ImageId",
                principalTable: "Image",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctor_Image_ImageId",
                table: "Doctor");

            migrationBuilder.DropTable(
                name: "Image");

            migrationBuilder.DropIndex(
                name: "IX_Doctor_ImageId",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "Doctor");
        }
    }
}
