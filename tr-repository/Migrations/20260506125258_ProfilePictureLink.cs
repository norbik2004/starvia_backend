using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tr_repository.Migrations
{
    /// <inheritdoc />
    public partial class ProfilePictureLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureLink",
                table: "UserPlatforms",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureLink",
                table: "UserPlatforms");
        }
    }
}
