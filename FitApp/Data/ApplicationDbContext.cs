using Microsoft.EntityFrameworkCore;
using FitApp.Models;
using FitApp.Services;

namespace FitApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<UserGoal> UserGoals { get; set; }
        public DbSet<UserDailyStats> UserDailyStats { get; set; }
        public DbSet<UserMeal> UserMeals { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleSection> ArticleSections { get; set; }
        public DbSet<ArticleImage> ArticleImages { get; set; }
        public DbSet<ArticleTip> ArticleTips { get; set; }
        public DbSet<ArticleWarning> ArticleWarnings { get; set; }
        public DbSet<ArticleKeyTakeaway> ArticleKeyTakeaways { get; set; }
        public DbSet<ArticleReference> ArticleReferences { get; set; }
        public DbSet<WorkoutProgram> WorkoutPrograms { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<WorkoutProgramExercise> WorkoutProgramExercises { get; set; }
        public DbSet<Todo> Todos { get; set; }
        public DbSet<TaskCompletion> TaskCompletions { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserDetails)
                .WithOne(ud => ud.User)
                .HasForeignKey<UserDetails>(ud => ud.UserId)
                .IsRequired();

            modelBuilder.Entity<UserDetails>()
                .HasIndex(ud => ud.UserId)
                .IsUnique();

            modelBuilder.Entity<UserDailyStats>()
                .HasIndex(uds => new { uds.UserId, uds.Date })
                .IsUnique();

            modelBuilder.Entity<UserDailyStats>()
                .HasOne(uds => uds.User)
                .WithMany()
                .HasForeignKey(uds => uds.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserMeal>()
                .HasIndex(um => new { um.UserId, um.Date, um.MealTime, um.Name })
                .IsUnique(false);

            modelBuilder.Entity<UserMeal>()
                .HasOne(um => um.User)
                .WithMany()
                .HasForeignKey(um => um.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FoodItem>()
                .HasIndex(fi => new { fi.Name, fi.Locale })
                .IsUnique(false);

            modelBuilder.Entity<Article>()
                .HasIndex(a => new { a.Title, a.Category })
                .IsUnique(false);

            // Article enhanced relationships
            modelBuilder.Entity<ArticleSection>()
                .HasOne(articleSection => articleSection.Article)
                .WithMany(article => article.Sections)
                .HasForeignKey(articleSection => articleSection.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticleImage>()
                .HasOne(articleImage => articleImage.Article)
                .WithMany(article => article.Images)
                .HasForeignKey(articleImage => articleImage.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticleImage>()
                .HasOne(articleImage => articleImage.Section)
                .WithMany(section => section.Images)
                .HasForeignKey(articleImage => articleImage.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticleTip>()
                .HasOne(articleTip => articleTip.Article)
                .WithMany(article => article.Tips)
                .HasForeignKey(articleTip => articleTip.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticleTip>()
                .HasOne(articleTip => articleTip.Section)
                .WithMany(section => section.Tips)
                .HasForeignKey(articleTip => articleTip.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticleWarning>()
                .HasOne(articleWarning => articleWarning.Article)
                .WithMany(article => article.Warnings)
                .HasForeignKey(articleWarning => articleWarning.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticleWarning>()
                .HasOne(articleWarning => articleWarning.Section)
                .WithMany(section => section.Warnings)
                .HasForeignKey(articleWarning => articleWarning.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticleKeyTakeaway>()
                .HasOne(articleKeyTakeaway => articleKeyTakeaway.Article)
                .WithMany(article => article.KeyTakeaways)
                .HasForeignKey(articleKeyTakeaway => articleKeyTakeaway.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ArticleReference>()
                .HasOne(articleReference => articleReference.Article)
                .WithMany(article => article.References)
                .HasForeignKey(articleReference => articleReference.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkoutProgram>()
                .HasIndex(w => new { w.Title, w.FocusArea, w.Difficulty })
                .IsUnique(false);

            modelBuilder.Entity<Exercise>()
                .HasIndex(e => new { e.Name, e.MuscleGroup })
                .IsUnique(false);

            modelBuilder.Entity<WorkoutProgramExercise>()
                .HasIndex(wpe => new { wpe.WorkoutProgramId, wpe.OrderIndex })
                .IsUnique(false);

            modelBuilder.Entity<WorkoutProgramExercise>()
                .HasOne(wpe => wpe.WorkoutProgram)
                .WithMany(wp => wp.WorkoutProgramExercises)
                .HasForeignKey(wpe => wpe.WorkoutProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkoutProgramExercise>()
                .HasOne(wpe => wpe.Exercise)
                .WithMany(e => e.WorkoutProgramExercises)
                .HasForeignKey(wpe => wpe.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Todo Global Query Filter - Kullanıcı izolasyonu
            modelBuilder.Entity<Todo>()
                .HasQueryFilter(t => t.UserId == _currentUserService.UserId && !t.IsArchived);

            modelBuilder.Entity<Todo>()
                .HasIndex(t => new { t.UserId, t.IsArchived })
                .IsUnique(false);

            modelBuilder.Entity<Todo>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // TaskCompletion konfigürasyonu
            modelBuilder.Entity<TaskCompletion>()
                .HasIndex(tc => new { tc.TodoId, tc.CompletionDate })
                .IsUnique();

            modelBuilder.Entity<TaskCompletion>()
                .HasOne(tc => tc.Todo)
                .WithMany()
                .HasForeignKey(tc => tc.TodoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmailVerification>()
                .HasIndex(ev => ev.Email)
                .IsUnique(false);

            modelBuilder.Entity<EmailVerification>()
                .HasIndex(ev => new { ev.Email, ev.Purpose })
                .IsUnique(false);
        }
    }
} 