using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniManageSys.Migrations
{
    /// <inheritdoc />
    public partial class AddHODToDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HODId",
                table: "Departments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HODId",
                table: "Departments",
                column: "HODId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Lecturers_HODId",
                table: "Departments",
                column: "HODId",
                principalTable: "Lecturers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Lecturers_HODId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_HODId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HODId",
                table: "Departments");
        }
    }
}
