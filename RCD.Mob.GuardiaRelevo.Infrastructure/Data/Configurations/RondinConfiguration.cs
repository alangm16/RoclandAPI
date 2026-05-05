using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations;

public class RondinConfiguration : IEntityTypeConfiguration<Rondin>
{
    public void Configure(EntityTypeBuilder<Rondin> builder)
    {
        builder.ToTable("TBL_ROCLAND_RELEVO_RONDINES");

        builder.HasKey(x => x.Id);

        // Clave foránea hacia el Relevo (nota el error de dedo intencional de la BD: RelevodId)
        builder.Property(x => x.RelevodId).IsRequired();

        builder.Property(x => x.TipoRondin)
               .HasMaxLength(20)
               .IsRequired();

        // El estado ahora arranca en 'EnCurso'
        builder.Property(x => x.Estado)
               .HasMaxLength(20)
               .HasDefaultValue("EnCurso")
               .IsRequired();

        builder.Property(x => x.GuardiaId).IsRequired();

        builder.Property(x => x.FechaInicio)
               .HasDefaultValueSql("GETDATE()")
               .IsRequired();

        builder.Property(x => x.FechaFin);

        builder.Property(x => x.Observaciones)
               .HasMaxLength(500);

        // Relación con el Relevo (Padre)
        builder.HasOne(x => x.Relevo)
               .WithMany(r => r.Rondines)
               .HasForeignKey(x => x.RelevodId)
               .OnDelete(DeleteBehavior.Restrict);

        // Constraint UNIQUE: Evita que existan dos rondines de 'Entrega' en un mismo Relevo
        builder.HasIndex(x => new { x.RelevodId, x.TipoRondin }).IsUnique();

        // Índice para búsquedas rápidas por Relevo
        builder.HasIndex(x => x.RelevodId);
    }
}