namespace RCD.Shared.Infrastructure.Notifications;

public interface IFcmTokenRepository
{
    Task InvalidarTokenAsync(string fcmToken);
}