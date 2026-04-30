
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.SuperAdmin.Domain.Entities;

namespace RCD.SuperAdmin.Infrastructure.Data.Configurations
{
    public class VistaConfiguration : IEntityTypeConfiguration<Vista>
    {
        public void Configure(EntityTypeBuilder<Vista> builder)
        {
            builder.ToTable("TBL_ROCLAND_SUPERADMIN_VISTAS");
            builder.HasKey(v => v.Id);
            builder.Property(v => v.Codigo).HasMaxLength(60).IsRequired();
            builder.Property(v => v.Nombre).HasMaxLength(150).IsRequired();
            builder.Property(v => v.Icono).HasMaxLength(100);

            // UNIQUE (ProyectoId, Codigo) — viene del ALTER TABLE del SQL
            builder.HasIndex(v => new { v.ProyectoId, v.Codigo }).IsUnique();

            builder.HasOne(v => v.Proyecto)
                   .WithMany(p => p.Vistas)
                   .HasForeignKey(v => v.ProyectoId);
        }
    }
}
