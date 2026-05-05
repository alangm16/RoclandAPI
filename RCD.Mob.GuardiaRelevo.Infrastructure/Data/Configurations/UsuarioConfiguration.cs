using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("TBL_ROCLAND_RELEVO_USUARIOS");

        // 1. Definimos la nueva llave primaria
        builder.HasKey(u => u.SuperAdminUsuarioId);

        // 2. Le decimos a EF Core que NO la autogenere (IDENTITY), porque el ID viene de SuperAdmin
        builder.Property(u => u.SuperAdminUsuarioId)
               .ValueGeneratedNever();

        // 3. Mapeo del resto de propiedades
        builder.Property(u => u.NumeroEmpleado).HasMaxLength(30).IsRequired();
        builder.HasIndex(u => u.NumeroEmpleado).IsUnique();

        builder.Property(u => u.RolLocal).HasMaxLength(20).IsRequired();

        builder.Property(u => u.Activo).HasDefaultValue(true);
        builder.Property(u => u.FechaCreacion).HasDefaultValueSql("GETDATE()");
    }
}