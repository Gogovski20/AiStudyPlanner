using AiStudyPlanner.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AiStudyPlanner.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<ChatHistory> ChatHistories => Set<ChatHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.ChatHistories)
                .WithOne(ch => ch.User)
                .HasForeignKey(ch => ch.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<ChatHistory>()
                .Property(ch => ch.Tasks)
                .HasColumnType("jsonb");
        }
    }
}
