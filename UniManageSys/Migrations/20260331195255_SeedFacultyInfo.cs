using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UniManageSys.Migrations
{
    /// <inheritdoc />
    public partial class SeedFacultyInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Faculties",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 2, "ENG", "Faculty of Engineering" },
                    { 3, "SMS", "Faculty of Social and Management Sciences" },
                    { 4, "ART", "Faculty of Arts and Humanities" },
                    { 5, "MED", "Faculty of Basic Medical Sciences" },
                    { 6, "LAW", "Faculty of Law" },
                    { 7, "EDU", "Faculty of Education" },
                    { 8, "AGR", "Faculty of Agriculture" },
                    { 9, "ENV", "Faculty of Environmental Sciences" },
                    { 10, "CIT", "Faculty of Computing and Information Technology" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
