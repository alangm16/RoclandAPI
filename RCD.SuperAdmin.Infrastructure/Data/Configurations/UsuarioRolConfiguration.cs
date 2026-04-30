
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.SuperAdmin.Domain.Entities;

namespace RCD.SuperAdmin.Infrastructure.Data.Configurations
{
    public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
    {
        public void Configure(EntityTypeBuilder<UsuarioRol> builder)
        {
            builder.ToTable("TBL_ROCLAND_SUPERADMIN_USUARIO_ROLES");
            builder.HasKey(ur => new {ur.UsuarioId, ur.RolId });

            builder.HasOne(ur => ur.Usuario)
                .WithMany(u => u.Roles)
                .HasForeignKey(ur => ur.UsuarioId);

            builder.HasOne(ur => ur.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(ur => ur.RolId);
        }
    }
}
