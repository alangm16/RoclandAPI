using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("TBL_ROCLAND_RELEVO_USUARIOS"); 
        builder.HasKey(x => x.SuperAdminUsuarioId);

        builder.Property(x => x.SuperAdminUsuarioId)
               .HasColumnName("SuperAdminUsuarioId") // Forzamos el nombre exacto del SQL
               .ValueGeneratedNever();

            builder.Property(x => x.NumeroEmpleado)
                   .HasColumnName("NumeroEmpleado")
                   .HasMaxLength(30);

            builder.Property(x => x.RolLocal)
                   .HasColumnName("RolLocal")
                   .HasMaxLength(20);

            builder.Property(x => x.Activo)
                   .HasColumnName("Activo");   
    }
}