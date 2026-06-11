using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class UserPrompt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPrompt_AspNetUsers_UserId",
                table: "UserPrompt");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrompt_Posts_PostId",
                table: "UserPrompt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPrompt",
                table: "UserPrompt");

            migrationBuilder.RenameTable(
                name: "UserPrompt",
                newName: "UserPrompts");

            migrationBuilder.RenameIndex(
                name: "IX_UserPrompt_UserId",
                table: "UserPrompts",
                newName: "IX_UserPrompts_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPrompt_PostId",
                table: "UserPrompts",
                newName: "IX_UserPrompts_PostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPrompts",
                table: "UserPrompts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrompts_AspNetUsers_UserId",
                table: "UserPrompts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrompts_Posts_PostId",
                table: "UserPrompts",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPrompts_AspNetUsers_UserId",
                table: "UserPrompts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrompts_Posts_PostId",
                table: "UserPrompts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPrompts",
                table: "UserPrompts");

            migrationBuilder.RenameTable(
                name: "UserPrompts",
                newName: "UserPrompt");

            migrationBuilder.RenameIndex(
                name: "IX_UserPrompts_UserId",
                table: "UserPrompt",
                newName: "IX_UserPrompt_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPrompts_PostId",
                table: "UserPrompt",
                newName: "IX_UserPrompt_PostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPrompt",
                table: "UserPrompt",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrompt_AspNetUsers_UserId",
                table: "UserPrompt",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrompt_Posts_PostId",
                table: "UserPrompt",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
