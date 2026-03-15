using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MijnKeuken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HasDelivery = table.Column<bool>(type: "bit", nullable: false),
                    DeliveryNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SysId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuEntries", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_MenuEntries_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuEntries_Date",
                table: "MenuEntries",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuEntries_RecipeId",
                table: "MenuEntries",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuEntries_SysId",
                table: "MenuEntries",
                column: "SysId",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuEntries");
        }
    }
}
