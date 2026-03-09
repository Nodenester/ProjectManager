using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhitelistedUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GitHubUsername = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhitelistedUsers", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "WhitelistedUsers",
                columns: new[] { "Id", "AddedAt", "AddedBy", "GitHubUsername", "IsActive" },
                values: new object[] { 1, new DateTime(2024, 12, 12, 19, 54, 7, 852, DateTimeKind.Utc).AddTicks(6936), null, "NodeNestor", true });

            migrationBuilder.CreateIndex(
                name: "IX_WhitelistedUsers_GitHubUsername",
                table: "WhitelistedUsers",
                column: "GitHubUsername",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhitelistedUsers");
        }
    }
}
