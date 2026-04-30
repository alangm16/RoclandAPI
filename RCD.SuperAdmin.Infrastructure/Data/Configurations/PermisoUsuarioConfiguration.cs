
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.SuperAdmin.Domain.Entities;

namespace RCD.SuperAdmin.Infrastructure.Data.Configurations
{
    public class PermisoUsuarioConfiguration : IEntityTypeConfiguration<PermisoUsuario>
    {
        public void Configure(EntityTypeBuilder<PermisoUsuario> builder)
        {
            builder.ToTable("TBL_ROCLAND_SUPERADMIN_PERMISOS_USUARIO");
            builder.HasKey(p => p.Id);

            builder.HasIndex(p => new { p.UsuarioId, p.ProyectoId, p.VistaId })
                   .IsUnique()
                   .HasDatabaseName("UQ_SuperAdmin_PermisosUsuario");

            builder.HasOne(p => p.Usuario)
                   .WithMany(u => u.Permisos)
                   .HasForeignKey(p => p.UsuarioId);

            builder.HasOne(p => p.Proyecto)
                   .WithMany(pr => pr.PermisosUsuario)
                   .HasForeignKey(p => p.ProyectoId);

            builder.HasOne(p => p.Vista)
                   .WithMany(v => v.PermisosUsuario)
                   .HasForeignKey(p => p.VistaId)
                   .IsRequired(false);
        }
    }
}
