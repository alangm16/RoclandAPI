namespace RCD.SuperAdmin.Application.Interfaces;

public interface ICurrentUserService
{
    int? GetUserId();

    int? GetProyectoId();

    string GetPlataforma();

    bool EsTokenMaestro();
}