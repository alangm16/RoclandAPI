using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RCD.Web.AccesoControl.Application.Interfaces;

namespace RCD.Web.AccesoControl.Web.Controllers;

[ApiController]
[Route("api/web/accesocontrol/[controller]")]
[ApiExplorerSettings(GroupName = "web-accesocontrol")]
public class PersonasController : ControllerBase
{
    private readonly IAccesoService _acceso;
    public PersonasController(IAccesoService acceso) => _acceso = acceso;

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string numId)
    {
        if (string.IsNullOrWhiteSpace(numId) || numId.Length < 3)
            return BadRequest("Ingresa al menos 3 caracteres.");

        var result = await _acceso.BuscarPersonaAsync(numId);
        return result is null ? NotFound() : Ok(result);
    }
}