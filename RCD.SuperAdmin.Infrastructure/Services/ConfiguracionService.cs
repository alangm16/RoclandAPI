using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Application.DTOs.Configuracion;
using RCD.SuperAdmin.Application.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Infrastructure.Data;

namespace RCD.SuperAdmin.Infrastructure.Services;

public class ConfiguracionService : IConfiguracionService
{
    private readonly SuperAdminDbContext _db;

    public ConfiguracionService(SuperAdminDbContext db)
    {
        _db = db;
    }

    public async Task<ConfiguracionSistemaDto> ObtenerAsync()
    {
        var config = await _db.Configuraciones
            .FirstOrDefaultAsync(c => c.Id == 1);

        if (config is null)
            throw new InvalidOperationException(
                "No se ha inicializado la configuración del sistema. Ejecute el seed correspondiente.");

        return new ConfiguracionSistemaDto(
            config.MaxIntentosFallidos,
            config.MinutosBloqueo,
            config.ExpiracionRefreshTokenHoras,
            config.ExpiracionAccessTokenMinutos,
            config.RequiereQRParaMobile
        );
    }

    public async Task ActualizarAsync(ConfiguracionSistemaDto dto)
    {
        var config = await _db.Configuraciones
            .FirstOrDefaultAsync(c => c.Id == 1)
            ?? throw new InvalidOperationException(
                "No se ha inicializado la configuración del sistema.");

        config.MaxIntentosFallidos = dto.MaxIntentosFallidos;
        config.MinutosBloqueo = dto.MinutosBloqueo;
        config.ExpiracionRefreshTokenHoras = dto.ExpiracionRefreshTokenHoras;
        config.ExpiracionAccessTokenMinutos = dto.ExpiracionAccessTokenMinutos;
        config.RequiereQRParaMobile = dto.RequiereQRParaMobile;

        _db.Configuraciones.Update(config);
        await _db.SaveChangesAsync();
    }
}