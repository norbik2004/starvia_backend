using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tr_repository.Migrations
{
    /// <inheritdoc />
    public partial class UserPromptAdditionalField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPrompts_Posts_PostId",
                table: "UserPrompts");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "UserPrompts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Response",
                table: "UserPrompts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrompts_Posts_PostId",
                table: "UserPrompts",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPrompts_Posts_PostId",
                table: "UserPrompts");

            migrationBuilder.DropColumn(
                name: "Response",
                table: "UserPrompts");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "UserPrompts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrompts_Posts_PostId",
                table: "UserPrompts",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
