using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trainingsmanager.Migrations
{
    /// <inheritdoc />
    public partial class addMailToSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdateMail",
                table: "Subscriptions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateMail",
                table: "Subscriptions");
        }
    }
}
