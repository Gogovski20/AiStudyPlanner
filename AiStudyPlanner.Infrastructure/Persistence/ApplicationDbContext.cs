using AiStudyPlanner.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

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

            var taskItemComparer = new ValueComparer<List<TaskItem>>(
                (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null)
                            == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),

                c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),

                c => JsonSerializer.Deserialize<List<TaskItem>>(
                        JsonSerializer.Serialize(c, (JsonSerializerOptions?)null),
                        (JsonSerializerOptions?)null
                    ) ?? new List<TaskItem>()
            );

            modelBuilder.Entity<ChatHistory>()
                .Property(ch => ch.Tasks)
                .HasConversion(
                    tasks => JsonSerializer.Serialize(tasks, (JsonSerializerOptions?)null),
                    json => JsonSerializer.Deserialize<List<TaskItem>>(json, (JsonSerializerOptions?)null)
                            ?? new List<TaskItem>()
                )
                .Metadata.SetValueComparer(taskItemComparer);

            modelBuilder.Entity<ChatHistory>()
                .Property(ch => ch.Tasks)
                .HasColumnType("jsonb");
        }
    }
}