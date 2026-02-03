using EduConnect.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TeacherProfile> TeacherProfiles { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<ContractSession> ContractSessions { get; set; }
    public DbSet<AttendanceLog> AttendanceLogs { get; set; }
    public DbSet<TeacherAvailability> TeacherAvailabilities { get; set; }
    public DbSet<StudentWallet> StudentWallets { get; set; }
    public DbSet<TransactionHistory> TransactionHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.Role)
                .HasConversion<int>();
        });

        // Configure TeacherProfile
        builder.Entity<TeacherProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User)
                .WithOne(u => u.TeacherProfile)
                .HasForeignKey<TeacherProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.VerificationStatus)
                .HasConversion<int>();
        });

        // Configure Student
        builder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Parent)
                .WithMany(u => u.Students)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.GradeLevel)
                .HasConversion<int>();
        });

        // Configure ContractSession
        builder.Entity<ContractSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContractId).IsUnique();
            entity.HasOne(e => e.Teacher)
                .WithMany(t => t.ContractSessions)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Student)
                .WithMany(s => s.ContractSessions)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.Property(e => e.Status)
                .HasConversion<int>();
        });

        // Configure AttendanceLog
        builder.Entity<AttendanceLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.HasOne(e => e.ContractSession)
                .WithMany(c => c.AttendanceLogs)
                .HasForeignKey(e => e.ContractId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.Property(e => e.Status)
                .HasConversion<int>();
        });

        // Configure TeacherAvailability
        builder.Entity<TeacherAvailability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Teacher)
                .WithMany(t => t.Availabilities)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure StudentWallet
        builder.Entity<StudentWallet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StudentId).IsUnique();
            entity.HasOne(e => e.Student)
                .WithOne()
                .HasForeignKey<StudentWallet>(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TransactionHistory
        builder.Entity<TransactionHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(e => e.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ContractSession)
                .WithMany()
                .HasForeignKey(e => e.ContractId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.Property(e => e.Type)
                .HasConversion<int>();
        });
    }
}
