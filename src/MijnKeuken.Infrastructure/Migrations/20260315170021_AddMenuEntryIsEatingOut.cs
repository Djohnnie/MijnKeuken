using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MijnKeuken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuEntryIsEatingOut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEatingOut",
                table: "MenuEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEatingOut",
                table: "MenuEntries");
        }
    }
}
