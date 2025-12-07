using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace whiteboard_app_data.Migrations
{
    /// <inheritdoc />
    public partial class AddStrokeStyleToShape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StrokeStyle",
                table: "Shapes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StrokeStyle",
                table: "Shapes");
        }
    }
}
