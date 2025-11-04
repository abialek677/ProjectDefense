using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Shared.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Room> Rooms { get; set; }
    public DbSet<InstructorAvailability> InstructorAvailabilities { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<StudentBlock> StudentBlocks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationship: Reservation - Student
        builder.Entity<Reservation>()
            .HasOne(r => r.Student)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure relationship: Reservation - InstructorAvailability
        builder.Entity<Reservation>()
            .HasOne(r => r.InstructorAvailability)
            .WithMany(d => d.Reservations)
            .HasForeignKey(r => r.InstructorAvailabilityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship: InstructorAvailability - Instructor
        builder.Entity<InstructorAvailability>()
            .HasOne(d => d.Instructor)
            .WithMany(u => u.Availabilities)
            .HasForeignKey(d => d.InstructorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship: InstructorAvailability - Room
        builder.Entity<InstructorAvailability>()
            .HasOne(d => d.Room)
            .WithMany(s => s.Availabilities)
            .HasForeignKey(d => d.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship: StudentBlock
        builder.Entity<StudentBlock>()
            .HasOne(b => b.Student)
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentBlock>()
            .HasOne(b => b.BlockingInstructor)
            .WithMany()
            .HasForeignKey(b => b.BlockingInstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for better performance
        builder.Entity<Reservation>()
            .HasIndex(r => r.StartTime);

        builder.Entity<Reservation>()
            .HasIndex(r => r.StudentId);

        builder.Entity<InstructorAvailability>()
            .HasIndex(d => new { d.RoomId, StartTime = d.StartDate, EndTime = d.EndDate });
    }
}
