using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.SuperAdmin.Domain.Entities;

namespace RCD.SuperAdmin.Infrastructure.Data.Configurations
{
    public class LogAccesoConfiguration : IEntityTypeConfiguration<LogAcceso>
    {
        public void Configure(EntityTypeBuilder<LogAcceso> builder)
        {
            builder.ToTable("TBL_ROCLAND_SUPERADMIN_LOGS_ACCESO");
            builder.HasKey(l => l.Id);
            builder.Property(l => l.UsernameUsado).HasMaxLength(60).IsRequired();
            builder.Property(l => l.IpAddress).HasMaxLength(50);
            builder.Property(l => l.Plataforma).HasMaxLength(50);
            builder.Property(l => l.Detalle).HasMaxLength(255);

            builder.HasIndex(l => new { l.UsuarioId, l.Fecha })
                   .HasDatabaseName("IX_SuperAdmin_LogsAcceso_UsuarioFecha");

            builder.HasOne(l => l.Usuario)
                   .WithMany(u => u.Logs)
                   .HasForeignKey(l => l.UsuarioId)
                   .IsRequired(false); 
        }
    }
}
