using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trainingsmanager.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SessionGroupId",
                table: "Sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SessionGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionGroupId",
                table: "Sessions",
                column: "SessionGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_SessionGroups_SessionGroupId",
                table: "Sessions",
                column: "SessionGroupId",
                principalTable: "SessionGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_SessionGroups_SessionGroupId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "SessionGroups");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_SessionGroupId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "SessionGroupId",
                table: "Sessions");
        }
    }
}
