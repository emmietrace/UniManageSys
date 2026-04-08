using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniManageSys.Migrations
{
    /// <inheritdoc />
    public partial class FixCourseRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseAssignments_Courses_CourseId1",
                table: "CourseAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseRegistrations_Courses_CourseId1",
                table: "CourseRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_CourseRegistrations_CourseId1",
                table: "CourseRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_CourseAssignments_CourseId1",
                table: "CourseAssignments");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "CourseRegistrations");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "CourseAssignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "CourseRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "CourseAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistrations_CourseId1",
                table: "CourseRegistrations",
                column: "CourseId1");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAssignments_CourseId1",
                table: "CourseAssignments",
                column: "CourseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAssignments_Courses_CourseId1",
                table: "CourseAssignments",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseRegistrations_Courses_CourseId1",
                table: "CourseRegistrations",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id");
        }
    }
}
