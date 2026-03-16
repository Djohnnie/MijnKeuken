using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MijnKeuken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeSourceUrlAndServings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Servings",
                table: "Recipes",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "Recipes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Servings",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "Recipes");
        }
    }
}
