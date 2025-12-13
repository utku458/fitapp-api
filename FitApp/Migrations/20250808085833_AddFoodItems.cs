using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoodItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CaloriesPer100g = table.Column<int>(type: "int", nullable: false),
                    ProteinPer100g = table.Column<int>(type: "int", nullable: false),
                    CarbsPer100g = table.Column<int>(type: "int", nullable: false),
                    FatPer100g = table.Column<int>(type: "int", nullable: false),
                    Locale = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Brand = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PortionName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PortionGrams = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_Name_Locale",
                table: "FoodItems",
                columns: new[] { "Name", "Locale" });

            // Seed data - TR ürünler
            migrationBuilder.InsertData(
                table: "FoodItems",
                columns: new[] { "Name", "CaloriesPer100g", "ProteinPer100g", "CarbsPer100g", "FatPer100g", "Locale", "Category" },
                values: new object[,]
                {
                    { "Tavuk Göğsü", 165, 31, 0, 3, "tr-TR", "Et" },
                    { "Yumurta", 155, 13, 1, 11, "tr-TR", "Protein" },
                    { "Yoğurt", 59, 10, 3, 0, "tr-TR", "Süt Ürünleri" },
                    { "Pirinç Pilavı", 130, 2, 28, 0, "tr-TR", "Tahıl" },
                    { "Mercimek Çorbası", 116, 9, 20, 0, "tr-TR", "Çorba" },
                    { "Balık", 206, 22, 0, 12, "tr-TR", "Deniz Ürünleri" },
                    { "Makarna", 131, 5, 25, 1, "tr-TR", "Tahıl" },
                    { "Ekmek", 265, 9, 49, 3, "tr-TR", "Tahıl" },
                    { "Süt", 42, 3, 5, 1, "tr-TR", "Süt Ürünleri" },
                    { "Peynir", 113, 25, 1, 0, "tr-TR", "Süt Ürünleri" },
                    { "Elma", 52, 0, 14, 0, "tr-TR", "Meyve" },
                    { "Muz", 89, 1, 23, 0, "tr-TR", "Meyve" },
                    { "Portakal", 47, 1, 12, 0, "tr-TR", "Meyve" },
                    { "Domates", 18, 1, 4, 0, "tr-TR", "Sebze" },
                    { "Salatalık", 16, 1, 4, 0, "tr-TR", "Sebze" },
                    { "Havuç", 41, 1, 10, 0, "tr-TR", "Sebze" },
                    { "Brokoli", 34, 3, 7, 0, "tr-TR", "Sebze" },
                    { "Ispanak", 23, 3, 4, 0, "tr-TR", "Sebze" },
                    { "Patates", 77, 2, 17, 0, "tr-TR", "Sebze" },
                    { "Soğan", 40, 1, 9, 0, "tr-TR", "Sebze" },
                    { "Sarımsak", 149, 6, 33, 0, "tr-TR", "Sebze" },
                    { "Zeytinyağı", 884, 0, 0, 100, "tr-TR", "Yağ" },
                    { "Tereyağı", 717, 1, 0, 81, "tr-TR", "Yağ" },
                    { "Ayçiçek Yağı", 884, 0, 0, 100, "tr-TR", "Yağ" },
                    { "Bal", 304, 0, 82, 0, "tr-TR", "Tatlı" },
                    { "Şeker", 387, 0, 100, 0, "tr-TR", "Tatlı" },
                    { "Çikolata", 545, 5, 61, 31, "tr-TR", "Tatlı" },
                    { "Bisküvi", 502, 5, 65, 25, "tr-TR", "Tatlı" },
                    { "Dondurma", 207, 4, 24, 11, "tr-TR", "Tatlı" },
                    { "Kek", 257, 5, 45, 6, "tr-TR", "Tatlı" },
                    { "Kurabiye", 502, 5, 65, 25, "tr-TR", "Tatlı" },
                    { "Reçel", 250, 0, 65, 0, "tr-TR", "Tatlı" },
                    { "Fındık", 628, 15, 17, 61, "tr-TR", "Kuruyemiş" },
                    { "Ceviz", 654, 15, 14, 65, "tr-TR", "Kuruyemiş" },
                    { "Badem", 579, 21, 22, 50, "tr-TR", "Kuruyemiş" },
                    { "Antep Fıstığı", 560, 20, 28, 45, "tr-TR", "Kuruyemiş" },
                    { "Kuru Üzüm", 299, 3, 79, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Kayısı", 241, 3, 63, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru İncir", 249, 3, 64, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Erik", 240, 2, 64, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Elma", 243, 2, 66, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Armut", 262, 2, 69, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Şeftali", 239, 3, 61, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Vişne", 290, 2, 73, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Kiraz", 263, 2, 67, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Çilek", 286, 3, 74, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Ananas", 269, 3, 70, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Mango", 319, 3, 78, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Papaya", 233, 3, 62, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Limon", 29, 1, 9, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Greyfurt", 42, 1, 11, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Mandalina", 53, 1, 13, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Limon", 29, 1, 9, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Portakal", 47, 1, 12, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Elma", 243, 2, 66, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Armut", 262, 2, 69, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Şeftali", 239, 3, 61, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Vişne", 290, 2, 73, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Kiraz", 263, 2, 67, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Çilek", 286, 3, 74, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Ananas", 269, 3, 70, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Mango", 319, 3, 78, 0, "tr-TR", "Kuruyemiş" },
                    { "Kuru Papaya", 233, 3, 62, 0, "tr-TR", "Kuruyemiş" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodItems");
        }
    }
}
