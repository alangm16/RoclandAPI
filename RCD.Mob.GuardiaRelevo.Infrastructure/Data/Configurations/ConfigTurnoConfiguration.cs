using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations;

public class ConfigTurnoConfiguration : IEntityTypeConfiguration<ConfigTurno>
{
    public void Configure(EntityTypeBuilder<ConfigTurno> builder)
    {
        builder.ToTable("TBL_ROCLAND_RELEVO_CONFIG_TURNOS");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
               .HasMaxLength(30)
               .IsRequired();
        builder.HasIndex(c => c.Nombre).IsUnique();

        builder.Property(c => c.HoraInicioVentana).IsRequired();
        builder.Property(c => c.HoraFinVentana).IsRequired();

        builder.Property(c => c.HabilitadoEntrega).HasDefaultValue(true);
        builder.Property(c => c.HabilitadoVerif).HasDefaultValue(true);
        builder.Property(c => c.Activo).HasDefaultValue(true);
    }
}
