
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
        public DbSet<Relevo> Relevos { get; set; }
        public DbSet<Rondin> Rondines => Set <Rondin>();
        public DbSet<ConfigTurno> ConfigTurnos { get; set; }
        public DbSet<ChecklistPunto> ChecklistPuntos => Set <ChecklistPunto>();
        public DbSet<ChecklistRespuesta> ChecklistRespuestas => Set <ChecklistRespuesta>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
            modelBuilder.ApplyConfiguration(new RelevoConfiguration());
            modelBuilder.ApplyConfiguration(new RondinConfiguration());
            modelBuilder.ApplyConfiguration(new ConfigTurnoConfiguration());
            modelBuilder.ApplyConfiguration(new ChecklistPuntoConfiguration());
            modelBuilder.ApplyConfiguration(new ChecklistRespuestaConfiguration());
        }
    }
}
