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
            builder.Property(u => u.UserName).HasMaxLength(60).IsRequired();
            builder.HasIndex(u => u.UserName).IsUnique();
            builder.Property(u => u.Email).HasMaxLength(150);
            builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
        }
    }
}
