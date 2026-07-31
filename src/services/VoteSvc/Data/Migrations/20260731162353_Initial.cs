using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoteSvc.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "votes",
                columns: table => new
                {
                    voter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_votes", x => new { x.voter_id, x.target_id, x.target_type });
                    table.CheckConstraint("ck_vote_target_type", "target_type IN ('Question', 'Answer')");
                    table.CheckConstraint("ck_vote_value", "value IN (1, -1)");
                });

            migrationBuilder.CreateIndex(
                name: "ix_votes_target_id_target_type",
                table: "votes",
                columns: new[] { "target_id", "target_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "votes");
        }
    }
}
