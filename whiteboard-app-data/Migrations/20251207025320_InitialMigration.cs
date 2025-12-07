using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace whiteboard_app_data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Theme = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DefaultCanvasWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultCanvasHeight = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultStrokeColor = table.Column<string>(type: "TEXT", maxLength: 9, nullable: false),
                    DefaultStrokeThickness = table.Column<double>(type: "REAL", nullable: false),
                    DefaultFillColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Canvases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    BackgroundColor = table.Column<string>(type: "TEXT", maxLength: 9, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Canvases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Canvases_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shapes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShapeType = table.Column<int>(type: "INTEGER", nullable: false),
                    StrokeColor = table.Column<string>(type: "TEXT", maxLength: 9, nullable: false),
                    StrokeThickness = table.Column<double>(type: "REAL", nullable: false),
                    FillColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsTemplate = table.Column<bool>(type: "INTEGER", nullable: false),
                    TemplateName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CanvasId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SerializedData = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shapes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shapes_Canvases_CanvasId",
                        column: x => x.CanvasId,
                        principalTable: "Canvases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Canvases_CreatedDate",
                table: "Canvases",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Canvases_Name",
                table: "Canvases",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Canvases_ProfileId",
                table: "Canvases",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_IsActive",
                table: "Profiles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Name",
                table: "Profiles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shapes_CanvasId",
                table: "Shapes",
                column: "CanvasId");

            migrationBuilder.CreateIndex(
                name: "IX_Shapes_CreatedDate",
                table: "Shapes",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Shapes_IsTemplate",
                table: "Shapes",
                column: "IsTemplate");

            migrationBuilder.CreateIndex(
                name: "IX_Shapes_IsTemplate_TemplateName",
                table: "Shapes",
                columns: new[] { "IsTemplate", "TemplateName" },
                filter: "[IsTemplate] = 1 AND [TemplateName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Shapes_ShapeType",
                table: "Shapes",
                column: "ShapeType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shapes");

            migrationBuilder.DropTable(
                name: "Canvases");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
