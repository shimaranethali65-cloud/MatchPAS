using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlindMatchPAS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectProposalAndMatchRecordModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IdentitiesRevealed",
                table: "Proposals",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentitiesRevealed",
                table: "Proposals");
        }
    }
}
