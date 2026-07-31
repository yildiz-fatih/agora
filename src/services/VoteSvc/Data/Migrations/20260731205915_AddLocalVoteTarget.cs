using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoteSvc.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalVoteTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "local_vote_targets",
                columns: table => new
                {
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "text", nullable: false),
                    parent_question_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_local_vote_targets", x => new { x.target_id, x.target_type });
                    table.CheckConstraint("ck_local_target_type", "target_type IN ('Question', 'Answer')");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "local_vote_targets");
        }
    }
}
