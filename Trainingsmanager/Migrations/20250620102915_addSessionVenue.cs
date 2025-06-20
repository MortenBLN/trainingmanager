﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trainingsmanager.Migrations
{
    /// <inheritdoc />
    public partial class addSessionVenue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionVenue",
                table: "Sessions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionVenue",
                table: "Sessions");
        }
    }
}
