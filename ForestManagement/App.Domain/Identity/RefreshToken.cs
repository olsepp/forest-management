using Base.Domain;

namespace App.Domain.Identity;

/// <summary>
/// Stored refresh token that can be exchanged for a new JWT access token.
/// Uses token-rotation: each use invalidates the old token and issues a new one.
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>Opaque, cryptographically-random token string (base64).</summary>
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Set when the token is revoked (logout or rotation).</summary>
    public DateTime? RevokedAt { get; set; }

    // -----------------------------------------------------------------------
    // Navigation
    // -----------------------------------------------------------------------
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    // -----------------------------------------------------------------------
    // Computed helpers
    // -----------------------------------------------------------------------
    public bool IsExpired  => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked  => RevokedAt.HasValue;
    public bool IsActive   => !IsRevoked && !IsExpired;
}
