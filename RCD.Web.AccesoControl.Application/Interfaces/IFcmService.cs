namespace RCD.Web.AccesoControl.Application.Interfaces;

public interface IFcmService
{
    Task EnviarAsync(string deviceToken, string titulo, string cuerpo,
        Dictionary<string, string>? data = null);
}