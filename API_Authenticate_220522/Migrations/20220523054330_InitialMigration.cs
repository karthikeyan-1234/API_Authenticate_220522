using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace API_Authenticate_220522.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_expired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.token);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tokens");
        }
    }
}
