using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace 占卜.Migrations
{
    /// <inheritdoc />
    public partial class InitAzure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DrawSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DrawMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DrawCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AiSummary = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrawSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TarotCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KeywordsUp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KeywordsRev = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarotCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrawCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DrawSessionId = table.Column<int>(type: "int", nullable: false),
                    TarotCardId = table.Column<int>(type: "int", nullable: false),
                    PositionIndex = table.Column<int>(type: "int", nullable: false),
                    PositionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Orientation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AiDetail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrawCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrawCards_DrawSessions_DrawSessionId",
                        column: x => x.DrawSessionId,
                        principalTable: "DrawSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrawCards_TarotCards_TarotCardId",
                        column: x => x.TarotCardId,
                        principalTable: "TarotCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrawCards_DrawSessionId",
                table: "DrawCards",
                column: "DrawSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DrawCards_TarotCardId",
                table: "DrawCards",
                column: "TarotCardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrawCards");

            migrationBuilder.DropTable(
                name: "DrawSessions");

            migrationBuilder.DropTable(
                name: "TarotCards");
        }
    }
}
