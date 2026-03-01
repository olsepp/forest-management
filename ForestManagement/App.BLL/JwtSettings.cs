namespace App.BLL;

/// <summary>
/// JWT configuration bound from the "JWT" section of appsettings.
/// Register via: services.Configure&lt;JwtSettings&gt;(config.GetSection("JWT"))
/// </summary>
public class JwtSettings
{
    /// <summary>HMAC-SHA256 signing key (≥ 32 chars).</summary>
    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "ForestManagement";
    public string Audience { get; set; } = "ForestManagementUsers";

    /// <summary>Access-token lifetime in minutes. Default 60.</summary>
    public int ExpiresInMinutes { get; set; } = 60;

    /// <summary>Refresh-token lifetime in days. Default 7.</summary>
    public int RefreshTokenExpiresInDays { get; set; } = 7;
}
