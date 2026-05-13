namespace RCD.SuperAdmin.Application.DTOs.Configuracion;

public record ConfiguracionSistemaDto(
    int MaxIntentosFallidos,
    int MinutosBloqueo,
    int ExpiracionRefreshTokenHoras,
    int ExpiracionAccessTokenMinutos,
    bool RequiereQRParaMobile
);