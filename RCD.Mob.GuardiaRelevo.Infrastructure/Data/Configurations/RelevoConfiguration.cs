using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations;

public class RelevoConfiguration : IEntityTypeConfiguration<Relevo>
{
    public void Configure(EntityTypeBuilder<Relevo> builder)
    {
        builder.ToTable("TBL_ROCLAND_RELEVO_RELEVOS");

        builder.HasKey(r => r.Id);

        // La fecha solo debe guardar el Día (sin horas)
        builder.Property(r => r.Fecha)
               .HasColumnType("DATE")
               .IsRequired();

        builder.Property(r => r.Estado)
               .HasMaxLength(20)
               .IsRequired()
               .HasDefaultValue("Pendiente");

        builder.Property(r => r.FechaCreacion)
               .HasDefaultValueSql("GETDATE()");

        // Relaciones con la tabla de Usuarios (Guardias)
        builder.HasOne(r => r.GuardiaSaliente)
               .WithMany()
               .HasForeignKey(r => r.GuardiaSalienteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.GuardiaEntrante)
               .WithMany()
               .HasForeignKey(r => r.GuardiaEntranteId)
               .OnDelete(DeleteBehavior.Restrict);

        // Mapeamos el Constraint UNIQUE que creaste en SQL
        builder.HasIndex(r => new { r.ConfigTurnoId, r.Fecha }).IsUnique();
    }
}
