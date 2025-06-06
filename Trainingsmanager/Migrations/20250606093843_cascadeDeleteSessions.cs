﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trainingsmanager.Migrations
{
    /// <inheritdoc />
    public partial class cascadeDeleteSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_SessionGroups_SessionGroupId",
                table: "Sessions");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_SessionGroups_SessionGroupId",
                table: "Sessions",
                column: "SessionGroupId",
                principalTable: "SessionGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_SessionGroups_SessionGroupId",
                table: "Sessions");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_SessionGroups_SessionGroupId",
                table: "Sessions",
                column: "SessionGroupId",
                principalTable: "SessionGroups",
                principalColumn: "Id");
        }
    }
}
