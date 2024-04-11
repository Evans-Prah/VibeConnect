using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeConnect.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Added_IsMutual_Friendship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMutual",
                table: "Friendships",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMutual",
                table: "Friendships");
        }
    }
}
