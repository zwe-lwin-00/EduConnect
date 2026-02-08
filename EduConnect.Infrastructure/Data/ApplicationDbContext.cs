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
    public DbSet<Homework> Homeworks { get; set; }
    public DbSet<StudentGrade> StudentGrades { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<GroupClass> GroupClasses { get; set; }
    public DbSet<GroupClassEnrollment> GroupClassEnrollments { get; set; }
    public DbSet<GroupSession> GroupSessions { get; set; }
    public DbSet<GroupSessionAttendance> GroupSessionAttendances { get; set; }

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

        // Configure Homework
        builder.Entity<Homework>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Homeworks)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Teacher)
                .WithMany(t => t.Homeworks)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ContractSession)
                .WithMany(c => c.Homeworks)
                .HasForeignKey(e => e.ContractSessionId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.Status)
                .HasConversion<int>();
        });

        // Configure StudentGrade
        builder.Entity<StudentGrade>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Student)
                .WithMany(s => s.StudentGrades)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Teacher)
                .WithMany(t => t.StudentGrades)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ContractSession)
                .WithMany(c => c.StudentGrades)
                .HasForeignKey(e => e.ContractSessionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure RefreshToken
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenHash);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Notification
        builder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Type)
                .HasConversion<int>();
        });

        // Configure GroupClass
        builder.Entity<GroupClass>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Teacher)
                .WithMany(t => t.GroupClasses)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure GroupClassEnrollment
        builder.Entity<GroupClassEnrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.GroupClass)
                .WithMany(g => g.Enrollments)
                .HasForeignKey(e => e.GroupClassId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ContractSession)
                .WithMany()
                .HasForeignKey(e => e.ContractId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure GroupSession
        builder.Entity<GroupSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.GroupClass)
                .WithMany(g => g.Sessions)
                .HasForeignKey(e => e.GroupClassId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Status)
                .HasConversion<int>();
        });

        // Configure GroupSessionAttendance
        builder.Entity<GroupSessionAttendance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.GroupSession)
                .WithMany(s => s.Attendances)
                .HasForeignKey(e => e.GroupSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ContractSession)
                .WithMany()
                .HasForeignKey(e => e.ContractId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
