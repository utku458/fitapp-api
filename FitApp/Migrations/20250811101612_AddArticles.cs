using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitApp.Migrations
{
    /// <inheritdoc />
    public partial class AddArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Author = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstimatedReadMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Title_Category",
                table: "Articles",
                columns: new[] { "Title", "Category" });

            migrationBuilder.InsertData(
                table: "Articles",
                columns: new[] { "Title", "Description", "Author", "Category", "ImageUrl", "EstimatedReadMinutes", "CreatedAt" },
                values: new object[,]
                {
                    { "Protein Zamanlaması 101", "Antrenman öncesi ve sonrası protein alımının performansa etkileri.", "FitApp", "Nutrition", "https://images.unsplash.com/photo-1490645935967-10de6ba17061", 5, DateTime.UtcNow },
                    { "Uyku ve Performans", "Kaliteli uykunun kas gelişimi ve yağ yakımına etkileri.", "FitApp", "Sleep", "https://images.unsplash.com/photo-1541781774459-bb2af2f05b55", 6, DateTime.UtcNow },
                    { "Evde 20 dk HIIT", "Ekipmansız yapılabilecek yüksek yoğunluklu interval antrenman.", "FitApp", "Workout", "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b", 7, DateTime.UtcNow },
                    { "Koşuya Başlangıç Rehberi", "Doğru ayakkabı, tempo ve nefes teknikleri.", "FitApp", "Cardio", "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b", 8, DateTime.UtcNow },
                    { "Soğuma ve Esneme", "Antrenman sonrası toparlanma için öneriler.", "FitApp", "Recovery", "https://images.unsplash.com/photo-1544367567-0f2fcb009e0b", 4, DateTime.UtcNow },
                    { "Kreatin Hakkında Her Şey", "Kreatin nedir, nasıl kullanılır, kimler için uygundur?", "FitApp", "Supplements", "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae", 9, DateTime.UtcNow },
                    { "Ofis Çalışanları İçin Egzersiz", "Gün içinde basit hareketlerle aktif kalma yöntemleri.", "FitApp", "Workout", "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b", 5, DateTime.UtcNow },
                    { "Yağ Yakımında Kardiyo Stratejileri", "Hangi kardiyo türleri daha etkili?", "FitApp", "Cardio", "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b", 6, DateTime.UtcNow },
                    { "Ara Öğün Önerileri", "Hızlı ve pratik sağlıklı atıştırmalıklar.", "FitApp", "Nutrition", "https://images.unsplash.com/photo-1490645935967-10de6ba17061", 4, DateTime.UtcNow },
                    { "Su Tüketimi ve Performans", "Günlük su ihtiyacı ve spor performansı ilişkisi.", "FitApp", "Nutrition", "https://images.unsplash.com/photo-1490645935967-10de6ba17061", 4, DateTime.UtcNow },
                    { "Sakatlık Önleme", "Isınma, teknik ve dinlenme ile sakatlıkların önüne geçin.", "FitApp", "Recovery", "https://images.unsplash.com/photo-1544367567-0f2fcb009e0b", 7, DateTime.UtcNow },
                    { "Uyku Hijyeni", "Daha derin ve kaliteli uyku için ipuçları.", "FitApp", "Sleep", "https://images.unsplash.com/photo-1541781774459-bb2af2f05b55", 6, DateTime.UtcNow },
                    { "Protein Kaynakları", "Hayvansal ve bitkisel protein kaynaklarının karşılaştırması.", "FitApp", "Nutrition", "https://images.unsplash.com/photo-1490645935967-10de6ba17061", 8, DateTime.UtcNow },
                    { "İlk Defa Ağırlık Çalışmak", "Başlangıç programı ve form ipuçları.", "FitApp", "Workout", "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b", 10, DateTime.UtcNow },
                    { "Interval Koşu Programı", "Hız ve dayanıklılık için haftalık plan.", "FitApp", "Cardio", "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b", 9, DateTime.UtcNow },
                    { "Foam Roller Kullanımı", "Kas gevşetme ve toparlanma için rehber.", "FitApp", "Recovery", "https://images.unsplash.com/photo-1544367567-0f2fcb009e0b", 5, DateTime.UtcNow },
                    { "Omega-3 Takviyesi", "Ne zaman, nasıl ve ne kadar?", "FitApp", "Supplements", "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae", 4, DateTime.UtcNow },
                    { "Kahvaltı Seçenekleri", "Dengeli bir başlangıç için pratik menüler.", "FitApp", "Nutrition", "https://images.unsplash.com/photo-1490645935967-10de6ba17061", 3, DateTime.UtcNow },
                    { "Split vs Full Body", "Hangi antrenman bölümü sizin için uygun?", "FitApp", "Workout", "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b", 8, DateTime.UtcNow },
                    { "Koşu Ayakkabısı Seçimi", "Sakatlık riskini azaltan doğru seçimler.", "FitApp", "Cardio", "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b", 5, DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articles");
        }
    }
}
