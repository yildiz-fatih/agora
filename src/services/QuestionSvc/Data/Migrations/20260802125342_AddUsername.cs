using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionSvc.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "author_username",
                table: "questions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "author_username",
                table: "answers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "author_username",
                table: "questions");

            migrationBuilder.DropColumn(
                name: "author_username",
                table: "answers");
        }
    }
}
