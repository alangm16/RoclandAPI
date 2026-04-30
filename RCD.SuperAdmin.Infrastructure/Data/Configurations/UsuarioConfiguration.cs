using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.SuperAdmin.Domain.Entities;

namespace RCD.SuperAdmin.Infrastructure.Data.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("TBL_ROCLAND_SUPERADMIN_USUARIOS");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.NombreCompleto).HasMaxLength(150).IsRequired();
            builder.Property(u => u.Username).HasMaxLength(60).IsRequired();
            builder.HasIndex(u => u.Username).IsUnique();
            builder.Property(u => u.Email).HasMaxLength(150);
            builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
            builder.Property(u => u.QRCode).HasMaxLength(200);
            builder.HasIndex(u => u.QRCode).IsUnique();
            builder.Property(u => u.DeviceToken).HasMaxLength(500);
            builder.Property(u => u.FcmToken).HasMaxLength(255);
        }
    }
}
