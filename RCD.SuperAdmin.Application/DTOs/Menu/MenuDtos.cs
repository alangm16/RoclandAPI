using System.Text.Json.Serialization;

namespace RCD.SuperAdmin.Application.DTOs.Menu;

public record VistaMenuDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("codigo")] string Codigo,
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("ruta")] string Ruta,
    [property: JsonPropertyName("icono")] string? Icono,
    [property: JsonPropertyName("orden")] int Orden,
    [property: JsonPropertyName("esContenedor")] bool EsContenedor,
    [property: JsonPropertyName("hijos")] IReadOnlyList<VistaMenuDto> Hijos
);