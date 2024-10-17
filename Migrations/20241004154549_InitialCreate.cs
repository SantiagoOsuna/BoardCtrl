using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace boardCtrl.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    categoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    titleCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    statusCategory = table.Column<bool>(type: "bit", nullable: false),
                    createdCategoryBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    editedCategoryBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdCategoryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editedCategoryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.categoryId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    roleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    statusRole = table.Column<bool>(type: "bit", nullable: false),
                    cretedRoleBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    editedRoleBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdRoleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editedRoleDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.roleId);
                });

            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    boardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    titleBoard = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    descriptionBoard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    statusBoard = table.Column<bool>(type: "bit", nullable: false),
                    categoryId = table.Column<int>(type: "int", nullable: false),
                    createdBoardBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    editedBoardBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createBoardDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editedBoardDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.boardId);
                    table.ForeignKey(
                        name: "FK_Boards_Categories_categoryId",
                        column: x => x.categoryId,
                        principalTable: "Categories",
                        principalColumn: "categoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    passwordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    roleId = table.Column<int>(type: "int", nullable: false),
                    statusUser = table.Column<bool>(type: "bit", nullable: false),
                    createdUserBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    editedUserBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdUserDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editedUserDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.userId);
                    table.ForeignKey(
                        name: "FK_Users_Roles_roleId",
                        column: x => x.roleId,
                        principalTable: "Roles",
                        principalColumn: "roleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Slides",
                columns: table => new
                {
                    slideId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    titleSlide = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    uRL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    time = table.Column<int>(type: "int", nullable: false),
                    boardId = table.Column<int>(type: "int", nullable: false),
                    statusSlide = table.Column<bool>(type: "bit", nullable: false),
                    createdSlideBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    editedSlideBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdSlideDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editedSlideDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slides", x => x.slideId);
                    table.ForeignKey(
                        name: "FK_Slides_Boards_boardId",
                        column: x => x.boardId,
                        principalTable: "Boards",
                        principalColumn: "boardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Boards_categoryId",
                table: "Boards",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Slides_boardId",
                table: "Slides",
                column: "boardId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_roleId",
                table: "Users",
                column: "roleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Slides");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
