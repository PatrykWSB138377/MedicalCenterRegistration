using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalCenterRegistration.Migrations
{
    /// <inheritdoc />
    public partial class VisitSummaryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visit_VisitScheduleId",
                table: "Visit");

            migrationBuilder.CreateTable(
                name: "VisitSummary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitSummary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitSummary_Visit_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisitSummaryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFile_VisitSummary_VisitSummaryId",
                        column: x => x.VisitSummaryId,
                        principalTable: "VisitSummary",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Visit_VisitScheduleId",
                table: "Visit",
                column: "VisitScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFile_VisitSummaryId",
                table: "UserFile",
                column: "VisitSummaryId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitSummary_VisitId",
                table: "VisitSummary",
                column: "VisitId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFile");

            migrationBuilder.DropTable(
                name: "VisitSummary");

            migrationBuilder.DropIndex(
                name: "IX_Visit_VisitScheduleId",
                table: "Visit");

            migrationBuilder.CreateIndex(
                name: "IX_Visit_VisitScheduleId",
                table: "Visit",
                column: "VisitScheduleId",
                unique: true);
        }
    }
}
