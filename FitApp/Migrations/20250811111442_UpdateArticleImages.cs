using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateArticleImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update article images with Unsplash URLs
            migrationBuilder.Sql("UPDATE Articles SET ImageUrl = 'https://images.unsplash.com/photo-1490645935967-10de6ba17061' WHERE Category = 'Nutrition'");
            migrationBuilder.Sql("UPDATE Articles SET ImageUrl = 'https://images.unsplash.com/photo-1541781774459-bb2af2f05b55' WHERE Category = 'Sleep'");
            migrationBuilder.Sql("UPDATE Articles SET ImageUrl = 'https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b' WHERE Category = 'Workout'");
            migrationBuilder.Sql("UPDATE Articles SET ImageUrl = 'https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b' WHERE Category = 'Cardio'");
            migrationBuilder.Sql("UPDATE Articles SET ImageUrl = 'https://images.unsplash.com/photo-1544367567-0f2fcb009e0b' WHERE Category = 'Recovery'");
            migrationBuilder.Sql("UPDATE Articles SET ImageUrl = 'https://images.unsplash.com/photo-1584308666744-24d5c474f2ae' WHERE Category = 'Supplements'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
