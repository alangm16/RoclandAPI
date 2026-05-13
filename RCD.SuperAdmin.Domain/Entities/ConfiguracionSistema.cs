namespace RCD.SuperAdmin.Domain.Entities;

public class ConfiguracionSistema
{
    public int Id { get; set; }                     // Siempre 1 (single row)
    public int MaxIntentosFallidos { get; set; } = 5;
    public int MinutosBloqueo { get; set; } = 15;
    public int ExpiracionRefreshTokenHoras { get; set; } = 72;
    public int ExpiracionAccessTokenMinutos { get; set; } = 60;
    public bool RequiereQRParaMobile { get; set; } = false;
}