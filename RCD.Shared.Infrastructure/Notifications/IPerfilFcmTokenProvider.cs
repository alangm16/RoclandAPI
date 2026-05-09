public interface IPerfilFcmTokenProvider
{
    Task<IEnumerable<string>> ObtenerTokensActivosAsync(IEnumerable<int> superAdminUsuarioIds);
}