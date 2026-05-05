using Microsoft.EntityFrameworkCore;
using RCD.Web.AccesoControl.Domain.Models.Entities;

namespace RCD.Web.AccesoControl.Infrastructure.Persistence
{
    public class AccesoControlWebDbContext : DbContext
    {
        public AccesoControlWebDbContext(DbContextOptions<AccesoControlWebDbContext> options) : base(options)
        {
        }

        // DbSets (Tablas)
        public DbSet<TipoIdentificacion> TiposIdentificacion { get; set; } = null!;
        public DbSet<Area> Areas { get; set; } = null!;
        public DbSet<MotivoVisita> MotivosVisita { get; set; } = null!;
        public DbSet<Gafete> Gafetes { get; set; } = null!;
        public DbSet<Persona> Personas { get; set; } = null!;
        public DbSet<Perfil> Perfiles { get; set; } = null!;
        public DbSet<RegistroVisitante> RegistrosVisitantes { get; set; } = null!;
        public DbSet<RegistroProveedor> RegistrosProveedores { get; set; } = null!;
        public DbSet<SolicitudPendiente> SolicitudesPendientes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================
            // MAPEO DE NOMBRES DE TABLAS
            // ============================================================
            modelBuilder.Entity<TipoIdentificacion>().ToTable("TBL_ROCLAND_ACCESOCONTROL_TIPODEIDENTIFICACION");
            modelBuilder.Entity<Area>().ToTable("TBL_ROCLAND_ACCESOCONTROL_AREAS");
            modelBuilder.Entity<MotivoVisita>().ToTable("TBL_ROCLAND_ACCESOCONTROL_MOTIVOVISITA");
            modelBuilder.Entity<Gafete>().ToTable("TBL_ROCLAND_ACCESOCONTROL_GAFETES");
            modelBuilder.Entity<Persona>().ToTable("TBL_ROCLAND_ACCESOCONTROL_PERSONAS");
            modelBuilder.Entity<Perfil>().ToTable("TBL_ROCLAND_ACCESOCONTROL_PERFILES");
            modelBuilder.Entity<RegistroVisitante>().ToTable("TBL_ROCLAND_ACCESOCONTROL_REGISTROVISITANTES");
            modelBuilder.Entity<RegistroProveedor>().ToTable("TBL_ROCLAND_ACCESOCONTROL_REGISTROPROVEEDORES");
            modelBuilder.Entity<SolicitudPendiente>().ToTable("TBL_ROCLAND_ACCESOCONTROL_SOLICITUDESPENDIENTES");

            // ============================================================
            // CONFIGURACIONES ESPECÍFICAS
            // ============================================================

            // --- PERFIL ---
            modelBuilder.Entity<Perfil>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SuperAdminUsuarioId).IsUnique(); // Constraint UNIQUE
            });

            // --- REGISTRO VISITANTES ---
            modelBuilder.Entity<RegistroVisitante>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Campos calculados (PERSISTED en la BD)
                entity.Property(e => e.HoraEntrada).ValueGeneratedOnAddOrUpdate();
                entity.Property(e => e.HoraSalida).ValueGeneratedOnAddOrUpdate();
                entity.Property(e => e.MinutosEstancia).ValueGeneratedOnAddOrUpdate();

                // Relación con Perfil (Entrada)
                entity.HasOne(d => d.PerfilEntrada)
                    .WithMany(p => p.RegistrosVisitantesEntrada)
                    .HasForeignKey(d => d.PerfilEntradaId)
                    .OnDelete(DeleteBehavior.Restrict); // Evitar ciclos de cascada

                // Relación con Perfil (Salida)
                entity.HasOne(d => d.PerfilSalida)
                    .WithMany(p => p.RegistrosVisitantesSalida)
                    .HasForeignKey(d => d.PerfilSalidaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- REGISTRO PROVEEDORES ---
            modelBuilder.Entity<RegistroProveedor>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Campos calculados (PERSISTED en la BD)
                entity.Property(e => e.HoraEntrada).ValueGeneratedOnAddOrUpdate();
                entity.Property(e => e.HoraSalida).ValueGeneratedOnAddOrUpdate();
                entity.Property(e => e.MinutosEstancia).ValueGeneratedOnAddOrUpdate();

                // Relación con Perfil (Entrada)
                entity.HasOne(d => d.PerfilEntrada)
                    .WithMany(p => p.RegistrosProveedoresEntrada)
                    .HasForeignKey(d => d.PerfilEntradaId)
                    .OnDelete(DeleteBehavior.Restrict); // Evitar ciclos de cascada

                // Relación con Perfil (Salida)
                entity.HasOne(d => d.PerfilSalida)
                    .WithMany(p => p.RegistrosProveedoresSalida)
                    .HasForeignKey(d => d.PerfilSalidaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- SOLICITUDES PENDIENTES ---
            modelBuilder.Entity<SolicitudPendiente>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Relación con Perfil
                entity.HasOne(d => d.Perfil)
                    .WithMany() // No agregamos colección en Perfil para mantenerlo limpio
                    .HasForeignKey(d => d.PerfilId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // --- PERSONA ---
            modelBuilder.Entity<Persona>(entity =>
            {
                entity.HasIndex(e => new { e.TipoIdentificacionId, e.NumeroIdentificacion }).IsUnique();
            });
        }
    }
}