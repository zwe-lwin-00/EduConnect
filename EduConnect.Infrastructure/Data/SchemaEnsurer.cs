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
}
