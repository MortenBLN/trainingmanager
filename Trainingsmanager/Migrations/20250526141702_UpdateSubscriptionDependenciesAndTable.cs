using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trainingsmanager.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscriptionDependenciesAndTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_AppUsers_UserId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_UserId_SessionId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Subscriptions");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Subscriptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserName_SessionId",
                table: "Subscriptions",
                columns: new[] { "UserName", "SessionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_UserName_SessionId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Subscriptions");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId_SessionId",
                table: "Subscriptions",
                columns: new[] { "UserId", "SessionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_AppUsers_UserId",
                table: "Subscriptions",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
