// RefreshTokenResponse.cs
namespace RCD.SuperAdmin.Application.DTOs.Auth;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpira
);