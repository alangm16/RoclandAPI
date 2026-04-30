using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.SuperAdmin.Domain.Entities;

namespace RCD.SuperAdmin.Infrastructure.Data.Configurations
{
    public class ProyectoConfiguration : IEntityTypeConfiguration<Proyecto>
    {
        public void Configure(EntityTypeBuilder<Proyecto> builder)
        {
            builder.ToTable("TBL_ROCLAND_SUPERADMIN_PROYECTOS");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Codigo).HasMaxLength(60).IsRequired();
            builder.HasIndex(p => p.Codigo).IsUnique();
            builder.Property(p => p.Nombre).HasMaxLength(150).IsRequired();
            builder.Property(p => p.Plataforma).HasMaxLength(30).IsRequired();
            builder.Property(p => p.UrlBase).HasMaxLength(200);
        }
    }
}
