using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarviaBackend.Modules.Emails.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEmailTyping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "emails",
                table: "Emails",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                schema: "emails",
                table: "Emails");
        }
    }
}
