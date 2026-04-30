using Microsoft.EntityFrameworkCore;
using RCD.SuperAdmin.Domain.Entities;

namespace RCD.SuperAdmin.Infrastructure.Data
{
    public class SuperAdminDbContext (DbContextOptions<SuperAdminDbContext> options) : DbContext(options)
    {
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
        public DbSet<Proyecto> Proyectos => Set<Proyecto>();
        public DbSet<Vista> Vistas => Set<Vista>();
        public DbSet<PermisoRol> PermisosRol => Set<PermisoRol>();
        public DbSet<PermisoUsuario> PermisosUsuario => Set<PermisoUsuario>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<LogAcceso> LogsAcceso => Set<LogAcceso>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SuperAdminDbContext).Assembly);
    }
}
