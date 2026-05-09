namespace RCD.Shared.Infrastructure.Notifications;

public interface IFcmService
{
    Task EnviarAsync(string deviceToken, string titulo, string cuerpo,
        Dictionary<string, string>? data = null);
}