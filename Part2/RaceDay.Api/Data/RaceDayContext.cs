using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<EventStatus> EventStatuses => Set<EventStatus>();
    public DbSet<EnrolmentStatus> EnrolmentStatuses => Set<EnrolmentStatus>();

    // The SQL script uses varchar, so strings are not unicode
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().AreUnicode(false);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.ToTable("Users", t => t.HasCheckConstraint("CK_Users_ROLE", "[ROLE] in ('Organiser', 'Participant')"));
        });

        modelBuilder.Entity<EventType>().HasIndex(t => t.TypeName).IsUnique();
        modelBuilder.Entity<EventStatus>().HasIndex(s => s.StatusName).IsUnique();
        modelBuilder.Entity<EnrolmentStatus>().HasIndex(s => s.StatusName).IsUnique();

        modelBuilder.Entity<Event>(e =>
        {
            e.Property(x => x.EventStatusId).HasDefaultValue(2);
            e.HasOne(x => x.Organiser).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.EventType).WithMany().HasForeignKey(x => x.TypeId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.EventStatus).WithMany().HasForeignKey(x => x.EventStatusId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(c => new { c.EventId, c.CategoryName }).IsUnique();
            e.HasOne(c => c.Event).WithMany(ev => ev.Categories).HasForeignKey(c => c.EventId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrolment>(e =>
        {
            e.Property(x => x.StatusId).HasDefaultValue(1);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Event).WithMany(ev => ev.Enrolments).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Status).WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Result>(e =>
        {
            e.HasOne(r => r.Enrolment).WithMany(en => en.Results).HasForeignKey(r => r.EnrolmentId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
