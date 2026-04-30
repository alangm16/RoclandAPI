
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.SuperAdmin.Domain.Entities;

namespace RCD.SuperAdmin.Infrastructure.Data.Configurations
{
    public class PermisoRolConfiguration : IEntityTypeConfiguration<PermisoRol>
    {
        public void Configure(EntityTypeBuilder<PermisoRol> builder)
        {
            builder.ToTable("TBL_ROCLAND_SUPERADMIN_PERMISOS_ROL");
            builder.HasKey(p => p.Id);

            // UNIQUE (RolId, ProyectoId, VistaId) — del CONSTRAINT del SQL
            builder.HasIndex(p => new { p.RolId, p.ProyectoId, p.VistaId })
                   .IsUnique()
                   .HasDatabaseName("UQ_SuperAdmin_PermisosRol");

            builder.HasOne(p => p.Rol)
                   .WithMany(r => r.Permisos)
                   .HasForeignKey(p => p.RolId);

            builder.HasOne(p => p.Proyecto)
                   .WithMany(pr => pr.PermisosRol)
                   .HasForeignKey(p => p.ProyectoId);

            builder.HasOne(p => p.Vista)
                   .WithMany(v => v.PermisosRol)
                   .HasForeignKey(p => p.VistaId)
                   .IsRequired(false);
        }
    }
}
