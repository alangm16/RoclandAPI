using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> b)
        {
            b.ToTable("TBL_ROCLAND_RELEVO_USUARIOS");
            b.HasKey(x => x.Id);
            b.Property(x => x.Usuario_).HasColumnName("Usuario").HasMaxLength(60).IsRequired();
            b.HasIndex(x => x.Usuario_).IsUnique();
            b.Property(x => x.NombreCompleto).HasMaxLength(150).IsRequired();
            b.Property(x => x.NumeroEmpleado).HasMaxLength(30).IsRequired();
            b.HasIndex(x => x.NumeroEmpleado).IsUnique();
            b.Property(x => x.Email).HasMaxLength(150);
            b.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
            b.Property(x => x.QRCode).HasMaxLength(200).IsRequired();
            b.HasIndex(x => x.QRCode).IsUnique();
            b.Property(x => x.Rol).HasMaxLength(20).IsRequired();
            b.Property(x => x.DeviceToken).HasMaxLength(500);
            b.Property(x => x.FcmToken).HasMaxLength(255);
            b.Property(x => x.Activo).HasDefaultValue(true);
            b.Property(x => x.FechaCreacion).HasDefaultValueSql("GETDATE()");
        }
    }
}
