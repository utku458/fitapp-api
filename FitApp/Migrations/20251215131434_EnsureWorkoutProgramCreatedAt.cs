using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitApp.Migrations
{
    /// <inheritdoc />
    public partial class EnsureWorkoutProgramCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add CreatedAt column if it doesn't exist
            // MySQL doesn't support IF NOT EXISTS for ALTER TABLE ADD COLUMN
            // So we check if column exists first using a stored procedure approach
            migrationBuilder.Sql(@"
                SET @dbname = DATABASE();
                SET @tablename = 'WorkoutPrograms';
                SET @columnname = 'CreatedAt';
                SET @preparedStatement = (SELECT IF(
                    (
                        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = @dbname
                        AND TABLE_NAME = @tablename
                        AND COLUMN_NAME = @columnname
                    ) > 0,
                    'SELECT 1',
                    CONCAT('ALTER TABLE ', @tablename, ' ADD COLUMN ', @columnname, ' DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP())')
                ));
                PREPARE alterIfNotExists FROM @preparedStatement;
                EXECUTE alterIfNotExists;
                DEALLOCATE PREPARE alterIfNotExists;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: We don't remove CreatedAt column in Down migration
            // to avoid data loss. If needed, remove manually.
        }
    }
}
