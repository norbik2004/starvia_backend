using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarviaBackend.Modules.Posts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePostPlatformTypeWithPlatformsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Platforms",
                schema: "posts",
                table: "Posts",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[0]'::jsonb");

            migrationBuilder.Sql(
                """
                UPDATE posts."Posts"
                SET "Platforms" = jsonb_build_array("PlatformType");
                """);

            migrationBuilder.DropColumn(
                name: "PlatformType",
                schema: "posts",
                table: "Posts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlatformType",
                schema: "posts",
                table: "Posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE posts."Posts"
                SET "PlatformType" = COALESCE(("Platforms" ->> 0)::integer, 0);
                """);

            migrationBuilder.DropColumn(
                name: "Platforms",
                schema: "posts",
                table: "Posts");
        }
    }
}
