namespace EduConnect.Domain.Entities;

/// <summary>
/// Stored refresh token (hash only). Used to issue new access tokens without re-login.
/// Revoked when user logs out or when token is rotated after use.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
