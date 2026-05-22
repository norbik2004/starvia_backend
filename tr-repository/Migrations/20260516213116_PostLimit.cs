using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tr_repository.Migrations
{
    /// <inheritdoc />
    public partial class PostLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PostsCounterResetAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PostsGeneratedThisMonth",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostsCounterResetAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PostsGeneratedThisMonth",
                table: "AspNetUsers");
        }
    }
}
