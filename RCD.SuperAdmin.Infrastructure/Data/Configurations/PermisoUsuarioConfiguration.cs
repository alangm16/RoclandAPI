
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

            builder.HasOne(p => p.Usuario)
                .WithMany(u => u.Permisos)
                .HasForeignKey(p => p.UsuarioId);

            builder.HasOne(p => p.Proyecto)
                .WithMany(pr => pr.Permisos)
                .HasForeignKey(p => p.ProyectoId);

            builder.HasOne(p => p.Vista)
                .WithMany(v => v.Permisos)
                .HasForeignKey(p => p.VistaId)
                .IsRequired(false);
        }
    }
}
