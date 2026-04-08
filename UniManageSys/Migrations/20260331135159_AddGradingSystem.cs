using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniManageSys.Migrations
{
    /// <inheritdoc />
    public partial class AddGradingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseRegistrationId = table.Column<int>(type: "int", nullable: false),
                    GradedByLecturerId = table.Column<int>(type: "int", nullable: false),
                    ContinuousAssessment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExamScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    GradePoint = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    DateUploaded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentResults_CourseRegistrations_CourseRegistrationId",
                        column: x => x.CourseRegistrationId,
                        principalTable: "CourseRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentResults_Lecturers_GradedByLecturerId",
                        column: x => x.GradedByLecturerId,
                        principalTable: "Lecturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentResults_CourseRegistrationId",
                table: "StudentResults",
                column: "CourseRegistrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentResults_GradedByLecturerId",
                table: "StudentResults",
                column: "GradedByLecturerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentResults");
        }
    }
}
