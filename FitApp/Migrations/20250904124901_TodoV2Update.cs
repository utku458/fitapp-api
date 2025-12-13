using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitApp.Migrations
{
    /// <inheritdoc />
    public partial class TodoV2Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Todos");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Todos",
                newName: "WeekdaysMask");

            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                table: "Todos",
                newName: "IsArchived");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Todos",
                type: "varchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Todos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceType",
                table: "Todos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Todos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TodoCompletions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TodoId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoCompletions_Todos_TodoId",
                        column: x => x.TodoId,
                        principalTable: "Todos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Önce yeni index'i oluştur
            migrationBuilder.CreateIndex(
                name: "IX_Todos_UserId_IsArchived",
                table: "Todos",
                columns: new[] { "UserId", "IsArchived" });

            // Sonra eski index'i sil
            migrationBuilder.DropIndex(
                name: "IX_Todos_UserId",
                table: "Todos");

            migrationBuilder.CreateIndex(
                name: "IX_TodoCompletions_TodoId_Date",
                table: "TodoCompletions",
                columns: new[] { "TodoId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TodoCompletions");

            migrationBuilder.DropIndex(
                name: "IX_Todos_UserId_IsArchived",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "RecurrenceType",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Todos");

            migrationBuilder.RenameColumn(
                name: "WeekdaysMask",
                table: "Todos",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "IsArchived",
                table: "Todos",
                newName: "IsCompleted");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Todos",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(120)",
                oldMaxLength: 120)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Todos",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Todos_UserId",
                table: "Todos",
                column: "UserId");
        }
    }
}
