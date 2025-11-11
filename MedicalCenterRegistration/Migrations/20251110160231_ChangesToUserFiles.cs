using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCenterRegistration.Migrations
{
    /// <inheritdoc />
    public partial class ChangesToUserFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRating_Doctor_DoctorId",
                table: "DoctorRating");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRating_Patient_PatientId",
                table: "DoctorRating");

            migrationBuilder.DropForeignKey(
                name: "FK_Visit_Patient_PatientId",
                table: "Visit");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "UserFile");

            migrationBuilder.CreateTable(
                name: "UserFileOwner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFileOwner", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFileOwner_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserFileOwner_UserFile_FileId",
                        column: x => x.FileId,
                        principalTable: "UserFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFileOwner_FileId",
                table: "UserFileOwner",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFileOwner_UserId",
                table: "UserFileOwner",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRating_Doctor_DoctorId",
                table: "DoctorRating",
                column: "DoctorId",
                principalTable: "Doctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRating_Patient_PatientId",
                table: "DoctorRating",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Visit_Patient_PatientId",
                table: "Visit",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRating_Doctor_DoctorId",
                table: "DoctorRating");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRating_Patient_PatientId",
                table: "DoctorRating");

            migrationBuilder.DropForeignKey(
                name: "FK_Visit_Patient_PatientId",
                table: "Visit");

            migrationBuilder.DropTable(
                name: "UserFileOwner");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "UserFile",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRating_Doctor_DoctorId",
                table: "DoctorRating",
                column: "DoctorId",
                principalTable: "Doctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRating_Patient_PatientId",
                table: "DoctorRating",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Visit_Patient_PatientId",
                table: "Visit",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
