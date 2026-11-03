using BookSpace.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace BookSpace.Infrastructure.Data;

public class BookSpaceDbContext : DbContext
{
    public BookSpaceDbContext(DbContextOptions<BookSpaceDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).HasMaxLength(254).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.Role).HasConversion<int>();
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Room>(e =>
        {
            e.ToTable("Rooms", t => t.HasCheckConstraint("CK_Rooms_Capacity", "[Capacity] >= 1 AND [Capacity] <= 100"));
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).HasMaxLength(100).IsRequired();
            e.Property(r => r.Location).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.ToTable("Bookings", t => t.HasCheckConstraint("CK_Bookings_StartHour", "[StartHour] >= 0 AND [StartHour] <= 23"));
            e.HasKey(b => b.Id);
            e.Property(b => b.Status).HasConversion<int>();

            e.Ignore(b => b.StartsAt);
            e.Ignore(b => b.EndsAt);
            e.Ignore(b => b.IsActive);

            e.HasOne(b => b.Room).WithMany().HasForeignKey(b => b.RoomId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Restrict);

            // ერთ ოთახში ერთ სლოტზე მხოლოდ ერთი აქტიური ჯავშანი — გადაფარვის დაცვა ბაზის დონეზე,
            // მათ შორის ერთდროული მოთხოვნების დროს. გაუქმებული ჯავშანი სლოტს ათავისუფლებს.
            e.HasIndex(b => new { b.RoomId, b.Date, b.StartHour })
                .IsUnique()
                .HasFilter("[Status] = 0")
                .HasDatabaseName("UX_Bookings_ActiveSlot");

            e.HasIndex(b => new { b.UserId, b.Date });
        });
    }
}
