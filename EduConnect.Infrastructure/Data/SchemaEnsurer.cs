using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastructure.Data;

/// <summary>
/// Ensures tables that were added after the initial EnsureCreated() exist.
/// EnsureCreated() does not add new tables to an existing database.
/// </summary>
public static class SchemaEnsurer
{
    public static async Task EnsureMissingTablesAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        await EnsureNotificationsTableAsync(context, cancellationToken);
        await EnsureRefreshTokensTableAsync(context, cancellationToken);
        await EnsureGroupClassesTableAsync(context, cancellationToken);
        await EnsureGroupClassEnrollmentsTableAsync(context, cancellationToken);
        await EnsureGroupSessionsTableAsync(context, cancellationToken);
        await EnsureGroupSessionAttendancesTableAsync(context, cancellationToken);
        await EnsureZoomColumnsAsync(context, cancellationToken);
    }

    /// <summary>Add Zoom join URL columns to existing tables (for teaching via Zoom).</summary>
    private static async Task EnsureZoomColumnsAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        var alterSql = @"
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TeacherProfiles') AND name = 'ZoomJoinUrl')
                ALTER TABLE [TeacherProfiles] ADD [ZoomJoinUrl] nvarchar(max) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GroupClasses') AND name = 'ZoomJoinUrl')
                ALTER TABLE [GroupClasses] ADD [ZoomJoinUrl] nvarchar(max) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AttendanceLogs') AND name = 'ZoomJoinUrl')
                ALTER TABLE [AttendanceLogs] ADD [ZoomJoinUrl] nvarchar(max) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GroupSessions') AND name = 'ZoomJoinUrl')
                ALTER TABLE [GroupSessions] ADD [ZoomJoinUrl] nvarchar(max) NULL;
        ";
        await context.Database.ExecuteSqlRawAsync(alterSql, cancellationToken);
    }

    private static async Task EnsureNotificationsTableAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        const string checkSql = """
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
            BEGIN
                CREATE TABLE [Notifications] (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [UserId] nvarchar(450) NOT NULL,
                    [Title] nvarchar(max) NOT NULL,
                    [Message] nvarchar(max) NOT NULL,
                    [Type] int NOT NULL,
                    [RelatedEntityType] nvarchar(max) NULL,
                    [RelatedEntityId] int NULL,
                    [IsRead] bit NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
            END
            """;
        await context.Database.ExecuteSqlRawAsync(checkSql, cancellationToken);
    }

    private static async Task EnsureRefreshTokensTableAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        const string checkSql = """
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
            BEGIN
                CREATE TABLE [RefreshTokens] (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [UserId] nvarchar(450) NOT NULL,
                    [TokenHash] nvarchar(450) NOT NULL,
                    [ExpiresAt] datetime2 NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [RevokedAt] datetime2 NULL,
                    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens] ([TokenHash]);
            END
            """;
        await context.Database.ExecuteSqlRawAsync(checkSql, cancellationToken);
    }

    private static async Task EnsureGroupClassesTableAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GroupClasses')
            BEGIN
                CREATE TABLE [GroupClasses] (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [TeacherId] int NOT NULL,
                    [Name] nvarchar(200) NOT NULL,
                    [IsActive] bit NOT NULL,
                    [ZoomJoinUrl] nvarchar(max) NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_GroupClasses] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_GroupClasses_TeacherProfiles_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [TeacherProfiles] ([Id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_GroupClasses_TeacherId] ON [GroupClasses] ([TeacherId]);
            END
            """;
        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private static async Task EnsureGroupClassEnrollmentsTableAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GroupClassEnrollments')
            BEGIN
                CREATE TABLE [GroupClassEnrollments] (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [GroupClassId] int NOT NULL,
                    [StudentId] int NOT NULL,
                    [ContractId] int NOT NULL,
                    [EnrolledAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_GroupClassEnrollments] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_GroupClassEnrollments_GroupClasses_GroupClassId] FOREIGN KEY ([GroupClassId]) REFERENCES [GroupClasses] ([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_GroupClassEnrollments_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
                    CONSTRAINT [FK_GroupClassEnrollments_ContractSessions_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [ContractSessions] ([Id]) ON DELETE NO ACTION
                );
                CREATE INDEX [IX_GroupClassEnrollments_GroupClassId] ON [GroupClassEnrollments] ([GroupClassId]);
            END
            """;
        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private static async Task EnsureGroupSessionsTableAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GroupSessions')
            BEGIN
                CREATE TABLE [GroupSessions] (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [GroupClassId] int NOT NULL,
                    [CheckInTime] datetime2 NOT NULL,
                    [CheckOutTime] datetime2 NULL,
                    [TotalDurationHours] decimal(18,2) NOT NULL,
                    [LessonNotes] nvarchar(max) NULL,
                    [ZoomJoinUrl] nvarchar(max) NULL,
                    [Status] int NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_GroupSessions] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_GroupSessions_GroupClasses_GroupClassId] FOREIGN KEY ([GroupClassId]) REFERENCES [GroupClasses] ([Id]) ON DELETE CASCADE
                );
                CREATE INDEX [IX_GroupSessions_GroupClassId] ON [GroupSessions] ([GroupClassId]);
            END
            """;
        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private static async Task EnsureGroupSessionAttendancesTableAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GroupSessionAttendances')
            BEGIN
                CREATE TABLE [GroupSessionAttendances] (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [GroupSessionId] int NOT NULL,
                    [StudentId] int NOT NULL,
                    [ContractId] int NOT NULL,
                    [HoursUsed] decimal(18,2) NOT NULL,
                    CONSTRAINT [PK_GroupSessionAttendances] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_GroupSessionAttendances_GroupSessions_GroupSessionId] FOREIGN KEY ([GroupSessionId]) REFERENCES [GroupSessions] ([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_GroupSessionAttendances_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION,
                    CONSTRAINT [FK_GroupSessionAttendances_ContractSessions_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [ContractSessions] ([Id]) ON DELETE NO ACTION
                );
                CREATE INDEX [IX_GroupSessionAttendances_GroupSessionId] ON [GroupSessionAttendances] ([GroupSessionId]);
            END
            """;
        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
