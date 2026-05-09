namespace RCD.Shared.Kernel.Settings;

/// Se enlaza con la sección "Jwt" del appsettings.json.
/// Registrar con: builder.Services.Configure&lt;JwtSettings&gt;(builder.Configuration.GetSection("Jwt"));

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "RCD";
    public string Audience { get; set; } = "RCD";
    public int ExpirationMinutes { get; set; } = 60;
    public int MaestroExpirationMinutes { get; set; } = 480;
    public int RefreshTokenDays { get; set; } = 30;
}