using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class InitialBrands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "brands");

            migrationBuilder.CreateTable(
                name: "BrandProfiles",
                schema: "brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Brief = table.Column<string>(type: "jsonb", nullable: false),
                    GenerationDefaults = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BrandProfileUserPlatforms",
                schema: "brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserPlatformId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandProfileUserPlatforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrandProfileUserPlatforms_BrandProfiles_BrandProfileId",
                        column: x => x.BrandProfileId,
                        principalSchema: "brands",
                        principalTable: "BrandProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrandProfiles_UserId",
                schema: "brands",
                table: "BrandProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandProfileUserPlatforms_BrandProfileId_UserPlatformId",
                schema: "brands",
                table: "BrandProfileUserPlatforms",
                columns: new[] { "BrandProfileId", "UserPlatformId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BrandProfileUserPlatforms_UserPlatformId",
                schema: "brands",
                table: "BrandProfileUserPlatforms",
                column: "UserPlatformId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrandProfileUserPlatforms",
                schema: "brands");

            migrationBuilder.DropTable(
                name: "BrandProfiles",
                schema: "brands");
        }
    }
}
