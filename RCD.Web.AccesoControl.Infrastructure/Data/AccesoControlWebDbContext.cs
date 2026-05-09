using Microsoft.EntityFrameworkCore;
using RCD.Shared.Kernel.Interfaces;
using RCD.Web.AccesoControl.Domain.Models.Entities;
using RCD.Web.AccesoControl.Domain.Models.Entities.Base;

namespace RCD.Web.AccesoControl.Infrastructure.Data;

public class AccesoControlWebDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AccesoControlWebDbContext(DbContextOptions<AccesoControlWebDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }
    // =========================================================================
    // SETS DE DATOS
    // =========================================================================
    public DbSet<TipoIdentificacion> TiposIdentificacion => Set<TipoIdentificacion>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<MotivoVisita> MotivosVisita => Set<MotivoVisita>();
    public DbSet<Gafete> Gafetes => Set<Gafete>();
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Perfil> Perfiles => Set<Perfil>();
    public DbSet<RegistroVisitante> RegistrosVisitantes => Set<RegistroVisitante>();
    public DbSet<RegistroProveedor> RegistrosProveedores => Set<RegistroProveedor>();
    public DbSet<SolicitudPendiente> SolicitudesPendientes => Set<SolicitudPendiente>();

    // =========================================================================
    // MODELO
    // =========================================================================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── 1. TipoIdentificacion ─────────────────────────────────────────────
        modelBuilder.Entity<TipoIdentificacion>(b =>
        {
            b.ToTable("TBL_ROCLAND_ACCESOCONTROL_TIPODEIDENTIFICACION");
            b.HasIndex(t => t.Nombre).IsUnique();
        });

        // ── 2. Areas ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Area>(b =>
        {
            b.ToTable("TBL_ROCLAND_ACCESOCONTROL_AREAS");
            b.HasIndex(a => a.Nombre).IsUnique();
        });

        // ── 3. MotivoVisita ───────────────────────────────────────────────────
        modelBuilder.Entity<MotivoVisita>(b =>
        {
            b.ToTable("TBL_ROCLAND_ACCESOCONTROL_MOTIVOVISITA");
            b.HasIndex(m => m.Nombre).IsUnique();
        });

        // ── 4. Gafetes ────────────────────────────────────────────────────────
        modelBuilder.Entity<Gafete>(b =>
        {
            b.ToTable("TBL_ROCLAND_ACCESOCONTROL_GAFETES");

            b.HasIndex(g => g.Codigo).IsUnique();

            b.Property(g => g.Estado)
             .HasMaxLength(20)
             .HasDefaultValue("Libre");
        });

        // ── 5. Personas ───────────────────────────────────────────────────────
        modelBuilder.Entity<Persona>(b =>
        {
            b.ToTable("TBL_ROCLAND_ACCESOCONTROL_PERSONAS");

            // Constraint único compuesto: tipo + número de identificación
            b.HasIndex(p => new { p.TipoIdentificacionId, p.NumeroIdentificacion })
             .IsUnique();

            b.HasIndex(p => p.NumeroIdentificacion);

            b.HasOne(p => p.TipoIdentificacion)
             .WithMany(t => t.Personas)
             .HasForeignKey(p => p.TipoIdentificacionId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── 6. Perfiles ───────────────────────────────────────────────────────
        modelBuilder.Entity<Perfil>(b =>
        {
            b.ToTable("TBL_ROCLAND_ACCESOCONTROL_PERFILES");

            // FK lógica — único por usuario de SA en este proyecto
            b.HasIndex(p => p.SuperAdminUsuarioId).IsUnique();

            b.Property(p => p.Turno).HasMaxLength(20);

            // CreadoPor / ModificadoPor son FK lógicas a SA — sin constraint real
            b.Property(p => p.CreadoPor).HasColumnName("CreadoPor");
            b.Property(p => p.ModificadoPor).HasColumnName("ModificadoPor");
        });

        // ── 7. RegistroVisitantes ─────────────────────────────────────────────
        modelBuilder.Entity<RegistroVisitante>(b =>
        {
            b.ToTable("TBL_ROCLAND_ACCESOCONTROL_REGISTROVISITANTES");

            b.HasIndex(rv => rv.FechaEntrada);
            b.HasIndex(rv => rv.PersonaId);
            b.HasIndex(rv => rv.EstadoAcceso);

            b.Property(rv => rv.EstadoAcceso)
             .HasMaxLength(20)
             .HasDefaultValue("Pendiente");

            // Columnas calculadas PERSISTED — EF solo lee, nunca escribe
            b.Property(rv => rv.HoraEntrada).ValueGeneratedOnAddOrUpdate();
            b.Property(rv => rv.HoraSalida).ValueGeneratedOnAddOrUpdate();
            b.Property(rv => rv.MinutosEstancia).ValueGeneratedOnAddOrUpdate();

            b.HasOne(rv => rv.Persona)
             .WithMany(p => p.RegistrosVisitantes)
             .HasForeignKey(rv => rv.PersonaId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(rv => rv.Area)
             .WithMany(a => a.RegistrosVisitantes)
             .HasForeignKey(rv => rv.AreaId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(rv => rv.Motivo)
             .WithMany(m => m.RegistrosVisitantes)
             .HasForeignKey(rv => rv.MotivoId)
             .OnDelete(DeleteBehavior.Restrict);

            // Dos FK al mismo Perfil — EF necesita que sean explícitas
            b.HasOne(rv => rv.PerfilEntrada)
             .WithMany(p => p.EntradasVisitantes)
             .HasForeignKey(rv => rv.PerfilEntradaId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(rv => rv.PerfilSalida)
             .WithMany(p => p.SalidasVisitantes)
             .HasForeignKey(rv => rv.PerfilSalidaId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(rv => rv.Gafete)
             .WithMany(g => g.RegistrosVisitantes)
             .HasForeignKey(rv => rv.GafeteId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── 8. RegistroProveedores ────────────────────────────────────────────
        modelBuilder.Entity<RegistroProveedor>(b =>
        {
            b.ToTable("TBL_ROCLAND_ACCESOCONTROL_REGISTROPROVEEDORES");

            b.HasIndex(rp => rp.FechaEntrada);
            b.HasIndex(rp => rp.PersonaId);
            b.HasIndex(rp => rp.EstadoAcceso);

            b.Property(rp => rp.EstadoAcceso)
             .HasMaxLength(20)
             .HasDefaultValue("Pendiente");

            // Columnas calculadas PERSISTED
            b.Property(rp => rp.HoraEntrada).ValueGeneratedOnAddOrUpdate();
            b.Property(rp => rp.HoraSalida).ValueGeneratedOnAddOrUpdate();
            b.Property(rp => rp.MinutosEstancia).ValueGeneratedOnAddOrUpdate();

            b.HasOne(rp => rp.Persona)
             .WithMany(p => p.RegistrosProveedores)
             .HasForeignKey(rp => rp.PersonaId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(rp => rp.Motivo)
             .WithMany(m => m.RegistrosProveedores)
             .HasForeignKey(rp => rp.MotivoId)
             .OnDelete(DeleteBehavior.Restrict);

            // Dos FK al mismo Perfil
            b.HasOne(rp => rp.PerfilEntrada)
             .WithMany(p => p.EntradasProveedores)
             .HasForeignKey(rp => rp.PerfilEntradaId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(rp => rp.PerfilSalida)
             .WithMany(p => p.SalidasProveedores)
             .HasForeignKey(rp => rp.PerfilSalidaId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(rp => rp.Gafete)
             .WithMany(g => g.RegistrosProveedores)
             .HasForeignKey(rp => rp.GafeteId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── 9. SolicitudesPendientes ──────────────────────────────────────────
        modelBuilder.Entity<SolicitudPendiente>(b =>
        {
            b.ToTable("TBL_ROCLAND_ACCESOCONTROL_SOLICITUDESPENDIENTES");

            b.HasIndex(s => s.Estado);

            b.Property(s => s.TipoRegistro).HasMaxLength(20);
            b.Property(s => s.Estado)
             .HasMaxLength(20)
             .HasDefaultValue("Pendiente");

            b.HasOne(s => s.Persona)
             .WithMany(p => p.Solicitudes)
             .HasForeignKey(s => s.PersonaId)
             .OnDelete(DeleteBehavior.Restrict);

            // PerfilId = supervisor que resolvió (nullable)
            b.HasOne(s => s.PerfilResolutor)
             .WithMany(p => p.SolicitudesResueltas)
             .HasForeignKey(s => s.PerfilId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.NoAction);
        });
    }

    // =========================================================================
    // MOTOR DE AUDITORÍA AUTOMÁTICA
    // =========================================================================
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetUserId(); // SuperAdminUsuarioId del JWT

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.FechaCreacion = DateTime.UtcNow;
                    entry.Entity.FechaModificacion = null;
                    entry.Entity.CreadoPor = userId;
                    entry.Entity.Activo = true;
                    break;

                case EntityState.Modified:
                    entry.Entity.FechaModificacion = DateTime.UtcNow;
                    entry.Entity.ModificadoPor = userId;
                    break;

                case EntityState.Deleted:
                    // Borrado físico → lógico
                    entry.State = EntityState.Modified;
                    entry.Entity.Activo = false;
                    entry.Entity.FechaModificacion = DateTime.UtcNow;
                    entry.Entity.ModificadoPor = userId;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}