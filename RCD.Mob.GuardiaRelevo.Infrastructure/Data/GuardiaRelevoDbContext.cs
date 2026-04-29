
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RCD.Mob.GuardiaRelevo.Domain.Entities;
using RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data
{
    public class GuardiaRelevoDbContext : DbContext
    {
        public GuardiaRelevoDbContext(DbContextOptions<GuardiaRelevoDbContext> options) : base(options) {}

        public DbSet<Usuario> Usuarios => Set <Usuario>();
        public DbSet<Rondin> Rondines => Set <Rondin>();
        public DbSet<RondinEvento> RondinEventos => Set <RondinEvento>();
        public DbSet<ChecklistPunto> ChecklistPuntos => Set <ChecklistPunto>();
        public DbSet<ChecklistRespuesta> ChecklistRespuestas => Set <ChecklistRespuesta>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
            modelBuilder.ApplyConfiguration(new RondinConfiguration());
            modelBuilder.ApplyConfiguration(new RondinEventoConfiguration());
            modelBuilder.ApplyConfiguration(new ChecklistPuntoConfiguration());
            modelBuilder.ApplyConfiguration(new ChecklistRespuestaConfiguration());
        }
    }
}
