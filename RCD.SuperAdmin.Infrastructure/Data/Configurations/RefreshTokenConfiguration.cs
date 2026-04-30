
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.SuperAdmin.Domain.Entities;

namespace RCD.SuperAdmin.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("TBL_ROCLAND_SUPERADMIN_REFRESH_TOKENS");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Token).HasMaxLength(200).IsRequired();
            builder.HasIndex(r => r.Token)
                   .IsUnique()
                   .HasDatabaseName("IX_SuperAdmin_RefreshTokens_Token");
            builder.Property(r => r.IpCreacion).HasMaxLength(50);
            builder.Property(r => r.DispositivoInfo).HasMaxLength(255);

            builder.HasOne(r => r.Usuario)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(r => r.UsuarioId);
        }
    }
}
