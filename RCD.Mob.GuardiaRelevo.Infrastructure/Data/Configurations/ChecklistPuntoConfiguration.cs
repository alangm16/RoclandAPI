using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations;

public class ChecklistPuntoConfiguration : IEntityTypeConfiguration<ChecklistPunto>
{
    public void Configure(EntityTypeBuilder<ChecklistPunto> b)
    {
        b.ToTable("TBL_ROCLAND_RELEVO_CHECKLIST_PUNTOS");
        b.HasKey(x => x.Id);
        b.Property(x => x.Categoria).HasMaxLength(100).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(200);
        b.Property(x => x.Activo).HasDefaultValue(true);
    }
}