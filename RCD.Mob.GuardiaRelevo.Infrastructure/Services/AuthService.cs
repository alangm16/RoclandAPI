using Microsoft.EntityFrameworkCore;
using RCD.Mob.GuardiaRelevo.Application.Interfaces;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Infrastructure.Data;
using System.Data;


namespace RCD.Mob.GuardiaRelevo.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly GuardiaRelevoDbContext _db;

    public AuthService(GuardiaRelevoDbContext db)
    {
        _db = db;
    }

    public async Task<Usuario?> ObtenerPerfilPorSuperAdminIdAsync(int superAdminId)
    {
        // Busca en TBL_ROCLAND_RELEVO_USUARIOS el perfil vinculado al SuperAdmin
        return await _db.Usuarios
        .FromSqlRaw("SELECT SuperAdminUsuarioId, NumeroEmpleado, RolLocal, Activo, FechaCreacion FROM TBL_ROCLAND_RELEVO_USUARIOS WHERE SuperAdminUsuarioId = {0}", superAdminId)
        .AsNoTracking()
        .FirstOrDefaultAsync();
    }

    public async Task<int?> ObtenerSuperAdminIdPorQRAsync(string qrCode)
    {
        // Obtenemos la conexión física a la BD que ya está usando tu DbContext actual
        var connection = _db.Database.GetDbConnection();
        bool connectionOpenedByUs = false;

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
                connectionOpenedByUs = true;
            }

            using var command = connection.CreateCommand();
            // Consultamos directamente la tabla de SuperAdmin por SQL
            command.CommandText = "SELECT Id FROM TBL_ROCLAND_SUPERADMIN_USUARIOS WHERE QRCode = @qr AND Activo = 1";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@qr";
            parameter.Value = qrCode;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync();

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }

            return null;
        }
        finally
        {
            // Si nosotros abrimos la conexión, la cerramos para dejar todo limpio
            if (connectionOpenedByUs)
            {
                await connection.CloseAsync();
            }
        }
    }
}