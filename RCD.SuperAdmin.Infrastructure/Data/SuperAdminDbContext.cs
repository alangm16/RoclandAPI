using Microsoft.EntityFrameworkCore;
using RCD.Shared.Kernel.Interfaces;
using RCD.SuperAdmin.Domain.Entities;
using RCD.SuperAdmin.Domain.Base;

namespace RCD.SuperAdmin.Infrastructure.Data;

public class SuperAdminDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public SuperAdminDbContext(
        DbContextOptions<SuperAdminDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<RolSA> RolesSA => Set<RolSA>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Vista> Vistas => Set<Vista>();
    public DbSet<ProyectoUsuarioRol> ProyectoUsuarioRoles => Set<ProyectoUsuarioRol>();
    public DbSet<UsuarioVistaAcceso> UsuarioVistasAcceso => Set<UsuarioVistaAcceso>();
    public DbSet<TokenDispositivo> TokensDispositivo => Set<TokenDispositivo>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LogAcceso> LogsAcceso => Set<LogAcceso>();
    public DbSet<Alerta> Alertas => Set<Alerta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── 0. RolSA ─────────────────────────────────────────────────────────
        modelBuilder.Entity<RolSA>(b =>
        {
            b.ToTable("TBL_ROCLAND_SUPERADMIN_ROLES_SA");
            b.HasIndex(r => r.Nombre).IsUnique();
        });

        // ── 1. Usuarios ───────────────────────────────────────────────────────
        modelBuilder.Entity<Usuario>(b =>
        {
            b.ToTable("TBL_ROCLAND_SUPERADMIN_USUARIOS", tb =>
            {
                tb.HasTrigger("TRG_SUPERADMIN_SYNC_NOMBRECOMPLETO_PERFIL");
            });

            b.HasIndex(u => u.Username).IsUnique();

            // Índice único filtrado: permite múltiples NULL en QRCode
            b.HasIndex(u => u.QRCode)
             .IsUnique()
             .HasFilter("[QRCode] IS NOT NULL");

            // Rol interno del panel SA (nullable → sin acceso al panel)
            b.HasOne(u => u.RolSA)
             .WithMany(r => r.Usuarios)
             .HasForeignKey(u => u.RolSAId)
             .OnDelete(DeleteBehavior.Restrict);

            // CreadoPor / ModificadoPor: FK auto-referencial diferida
            // (permite el seed del primer usuario sin circular dependency)
            b.HasOne<Usuario>()
             .WithMany()
             .HasForeignKey(u => u.CreadoPor)
             .OnDelete(DeleteBehavior.NoAction);

            b.HasOne<Usuario>()
             .WithMany()
             .HasForeignKey(u => u.ModificadoPor)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── 2. Proyectos ──────────────────────────────────────────────────────
        modelBuilder.Entity<Proyecto>(b =>
        {
            b.ToTable("TBL_ROCLAND_SUPERADMIN_PROYECTOS");

            b.HasIndex(p => p.Codigo).IsUnique();

            b.Property(p => p.Plataforma)
             .HasMaxLength(30);

            b.Property(p => p.Estado)
             .HasMaxLength(20)
             .HasDefaultValue("Produccion");

            b.HasOne<Usuario>()
             .WithMany()
             .HasForeignKey(p => p.CreadoPor)
             .OnDelete(DeleteBehavior.NoAction);

            b.HasOne<Usuario>()
             .WithMany()
             .HasForeignKey(p => p.ModificadoPor)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── 3. Roles por proyecto ─────────────────────────────────────────────
        modelBuilder.Entity<Rol>(b =>
        {
            b.ToTable("TBL_ROCLAND_SUPERADMIN_ROLES");

            // Un mismo nombre de rol no puede repetirse dentro del mismo proyecto
            b.HasIndex(r => new { r.ProyectoId, r.Nombre }).IsUnique();

            b.HasOne(r => r.Proyecto)
             .WithMany(p => p.Roles)
             .HasForeignKey(r => r.ProyectoId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── 4. Vistas (módulos/páginas) ───────────────────────────────────────
        modelBuilder.Entity<Vista>(b =>
        {
            b.ToTable("TBL_ROCLAND_SUPERADMIN_VISTAS");

            b.HasIndex(v => new { v.ProyectoId, v.Codigo }).IsUnique();

            b.HasOne(v => v.Proyecto)
             .WithMany(p => p.Vistas)
             .HasForeignKey(v => v.ProyectoId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── 5. ProyectoUsuarioRol (tabla puente central) ──────────────────────
        modelBuilder.Entity<ProyectoUsuarioRol>(b =>
        {
            b.ToTable("TBL_ROCLAND_SUPERADMIN_PROYECTO_USUARIO_ROL");

            // Un usuario solo puede tener un rol por proyecto
            b.HasIndex(pur => new { pur.UsuarioId, pur.ProyectoId }).IsUnique();

            b.HasOne(pur => pur.Proyecto)
             .WithMany(p => p.UsuariosAsignados)
             .HasForeignKey(pur => pur.ProyectoId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(pur => pur.Usuario)
             .WithMany(u => u.ProyectosAsignados)
             .HasForeignKey(pur => pur.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(pur => pur.Rol)
             .WithMany(r => r.UsuariosAsignados)
             .HasForeignKey(pur => pur.RolId)
             .OnDelete(DeleteBehavior.Restrict);

            // Trazabilidad de delegación
            b.HasOne(pur => pur.CreadoPorUsuario)
             .WithMany()
             .HasForeignKey(pur => pur.CreadoPor)
             .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(pur => pur.ModificadoPorUsuario)
             .WithMany()
             .HasForeignKey(pur => pur.ModificadoPor)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── 6. UsuarioVistaAcceso ─────────────────────────────────────────────
        modelBuilder.Entity<UsuarioVistaAcceso>(b =>
        {
            b.ToTable("TBL_ROCLAND_SUPERADMIN_USUARIO_VISTAS_ACCESO");

            // Un usuario solo puede tener un registro por vista
            b.HasIndex(uva => new { uva.UsuarioId, uva.VistaId }).IsUnique();

            b.HasOne(uva => uva.Usuario)
             .WithMany(u => u.VistasAcceso)
             .HasForeignKey(uva => uva.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(uva => uva.Proyecto)
             .WithMany()
             .HasForeignKey(uva => uva.ProyectoId)
             .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(uva => uva.Vista)
             .WithMany(v => v.AccesosUsuario)
             .HasForeignKey(uva => uva.VistaId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(uva => uva.CreadoPorUsuario)
             .WithMany()
             .HasForeignKey(uva => uva.CreadoPor)
             .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(uva => uva.ModificadoPorUsuario)
             .WithMany()
             .HasForeignKey(uva => uva.ModificadoPor)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── 7. TokensDispositivo ──────────────────────────────────────────────
        modelBuilder.Entity<TokenDispositivo>(b =>
        {
            b.ToTable("TBL_ROCLAND_SUPERADMIN_TOKENS_DISPOSITIVO");

            // Un solo token por usuario + proyecto + plataforma
            b.HasIndex(td => new { td.UsuarioId, td.ProyectoId, td.Plataforma }).IsUnique();

            b.Property(td => td.Plataforma).HasMaxLength(20);

            b.HasOne(td => td.Usuario)
             .WithMany(u => u.TokensDispositivo)
             .HasForeignKey(td => td.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(td => td.Proyecto)
             .WithMany(p => p.TokensDispositivo)
             .HasForeignKey(td => td.ProyectoId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── 8. RefreshTokens ──────────────────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.ToTable("TBL_ROCLAND_SUPERADMIN_REFRESH_TOKENS");

            b.HasIndex(rt => rt.Token).IsUnique();

            b.Property(rt => rt.Plataforma)
             .HasMaxLength(20)
             .HasDefaultValue("Web");

            b.HasOne(rt => rt.Usuario)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(rt => rt.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);

            // ProyectoId nullable: NULL = sesión del panel SA
            b.HasOne(rt => rt.Proyecto)
             .WithMany(p => p.RefreshTokens)
             .HasForeignKey(rt => rt.ProyectoId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── 9. LogsAcceso ─────────────────────────────────────────────────────
        modelBuilder.Entity<LogAcceso>(b =>
        {
            b.ToTable("TBL_ROCLAND_SUPERADMIN_LOGS_ACCESO");

            // UsuarioId nullable: puede no existir si el username no fue encontrado
            b.HasOne(la => la.Usuario)
             .WithMany(u => u.LogsAcceso)
             .HasForeignKey(la => la.UsuarioId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(la => la.Proyecto)
             .WithMany(p => p.LogsAcceso)
             .HasForeignKey(la => la.ProyectoId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── 10. Alertas ───────────────────────────────────────────────────────
        modelBuilder.Entity<Alerta>(b =>
        {
            b.ToTable("TBL_ROCLAND_SUPERADMIN_ALERTAS");

            b.Property(a => a.Tipo).HasMaxLength(20);

            // ProyectoId nullable: NULL = alerta global del SA
            b.HasOne(a => a.Proyecto)
             .WithMany(p => p.Alertas)
             .HasForeignKey(a => a.ProyectoId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetUserId();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.FechaCreacion = DateTime.UtcNow;
                    entry.Entity.CreadoPor = userId;
                    entry.Entity.FechaModificacion = DateTime.UtcNow;
                    entry.Entity.Activo = true;
                    break;

                case EntityState.Modified:
                    entry.Entity.FechaModificacion = DateTime.UtcNow;
                    entry.Entity.ModificadoPor = userId;
                    break;

                case EntityState.Deleted:
                    // Borra físico → convertido a borrado LÓGICO
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