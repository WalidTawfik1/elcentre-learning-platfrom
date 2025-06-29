using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElCentre.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_Edited_Bool_to_Q_A_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAt",
                table: "LessonQuestions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEdited",
                table: "LessonQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAt",
                table: "LessonAnswers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEdited",
                table: "LessonAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "LessonQuestions");

            migrationBuilder.DropColumn(
                name: "IsEdited",
                table: "LessonQuestions");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "LessonAnswers");

            migrationBuilder.DropColumn(
                name: "IsEdited",
                table: "LessonAnswers");
        }
    }
}
