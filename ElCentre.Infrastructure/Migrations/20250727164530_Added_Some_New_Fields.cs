using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElCentre.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_Some_New_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPreview",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CourseLanguage",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPreview",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "CourseLanguage",
                table: "Courses");
        }
    }
}
