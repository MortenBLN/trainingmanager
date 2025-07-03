using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trainingsmanager.Migrations
{
    /// <inheritdoc />
    public partial class addSessionTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Teamname = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    TrainingStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TrainingEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplicationsLimit = table.Column<int>(type: "integer", nullable: false),
                    ApplicationsRequired = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionVenue = table.Column<string>(type: "text", nullable: true),
                    TemplateName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionTemplates_AppUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionTemplates_CreatedById",
                table: "SessionTemplates",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionTemplates");
        }
    }
}
