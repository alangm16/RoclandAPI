namespace RCD.SuperAdmin.Application.DTOs;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalRegistros,
    int Pagina,
    int TamanoPagina
);