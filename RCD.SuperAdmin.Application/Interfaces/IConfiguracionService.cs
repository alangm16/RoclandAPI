using RCD.SuperAdmin.Application.DTOs.Configuracion;

namespace RCD.SuperAdmin.Application.Interfaces;

public interface IConfiguracionService
{
    Task<ConfiguracionSistemaDto> ObtenerAsync();
    Task ActualizarAsync(ConfiguracionSistemaDto dto);
}