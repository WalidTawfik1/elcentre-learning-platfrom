using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElCentre.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_Q_A_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LessonQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatorImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInstructor = table.Column<bool>(type: "bit", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonQuestions_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatorImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInstructor = table.Column<bool>(type: "bit", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonAnswers_LessonQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "LessonQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonAnswers_QuestionId",
                table: "LessonAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonQuestions_LessonId",
                table: "LessonQuestions",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonAnswers");

            migrationBuilder.DropTable(
                name: "LessonQuestions");
        }
    }
}
