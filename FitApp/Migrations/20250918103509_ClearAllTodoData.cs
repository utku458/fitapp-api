using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitApp.Migrations
{
    /// <inheritdoc />
    public partial class ClearAllTodoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tüm TaskCompletions verilerini sil
            migrationBuilder.Sql("DELETE FROM TaskCompletions");
            
            // Tüm Todos verilerini sil
            migrationBuilder.Sql("DELETE FROM Todos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma işlemi - veriler geri gelmez
            // Bu migration sadece veri temizleme için
        }
    }
}
