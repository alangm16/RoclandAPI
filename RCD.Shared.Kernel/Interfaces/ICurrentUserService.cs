namespace RCD.Shared.Kernel.Interfaces;

public interface ICurrentUserService
{
    int? GetUserId();

    int? GetProyectoId();

    string GetPlataforma();

    bool EsTokenMaestro();
}