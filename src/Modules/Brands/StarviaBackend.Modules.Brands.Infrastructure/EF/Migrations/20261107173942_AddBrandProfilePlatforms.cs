using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandProfilePlatforms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BrandProfilePlatforms",
                schema: "brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandProfilePlatforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrandProfilePlatforms_BrandProfiles_BrandProfileId",
                        column: x => x.BrandProfileId,
                        principalSchema: "brands",
                        principalTable: "BrandProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrandProfilePlatforms_BrandProfileId_PlatformType",
                schema: "brands",
                table: "BrandProfilePlatforms",
                columns: new[] { "BrandProfileId", "PlatformType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BrandProfilePlatforms_PlatformType",
                schema: "brands",
                table: "BrandProfilePlatforms",
                column: "PlatformType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrandProfilePlatforms",
                schema: "brands");
        }
    }
}
