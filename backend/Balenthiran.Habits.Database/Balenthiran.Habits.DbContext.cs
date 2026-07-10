using Balenthiran.Habits.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace Balenthiran.Habits.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<HabitEntity> Habits => Set<HabitEntity>();
    public DbSet<HabitEntryEntity> HabitEntries => Set<HabitEntryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HabitEntity>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Name).IsRequired().HasMaxLength(100);
            e.Property(h => h.Unit).HasMaxLength(20);
            // Today / Manage read habits in this order.
            e.HasIndex(h => h.SortOrder);
        });

        modelBuilder.Entity<HabitEntryEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Habit)
                .WithMany(h => h.Entries)
                .HasForeignKey(x => x.HabitId)
                .OnDelete(DeleteBehavior.Cascade);
            // At most one entry per habit per day — the key the daily upsert writes on.
            e.HasIndex(x => new { x.HabitId, x.Date }).IsUnique();
        });
    }
}
