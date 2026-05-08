using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.SuperAdmin.Domain.Entities;

namespace RCD.SuperAdmin.Infrastructure.Data.Configurations
{
    public class AlertasConfiguration: IEntityTypeConfiguration<Alerta>
    {
        public void Configure(EntityTypeBuilder<Alerta> builder)
        {
            builder.ToTable("TBL_ROCLAND_SUPERADMIN_ALERTAS");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Tipo).HasMaxLength(50).IsRequired();
            builder.Property(a => a.Titulo).HasMaxLength(100).IsRequired();
            builder.Property(a => a.Mensaje).HasMaxLength(500).IsRequired();
            builder.Property(a => a.Fecha).IsRequired();
            builder.Property(a => a.Resuelta).IsRequired();
            builder.Property(a => a.AccionUrl).HasMaxLength(200);
        }
    }
}