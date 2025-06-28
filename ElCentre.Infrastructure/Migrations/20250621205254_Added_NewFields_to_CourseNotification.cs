using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElCentre.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_NewFields_to_CourseNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "CourseNotifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CourseNotifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGlobal",
                table: "CourseNotifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "CourseNotifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetUserId",
                table: "CourseNotifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetUserRole",
                table: "CourseNotifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "CourseNotifications");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CourseNotifications");

            migrationBuilder.DropColumn(
                name: "IsGlobal",
                table: "CourseNotifications");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "CourseNotifications");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "CourseNotifications");

            migrationBuilder.DropColumn(
                name: "TargetUserRole",
                table: "CourseNotifications");
        }
    }
}
