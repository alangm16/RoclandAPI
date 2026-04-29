using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations;

public class RondinConfiguration : IEntityTypeConfiguration<Rondin>
{
    public void Configure(EntityTypeBuilder<Rondin> b)
    {
        b.ToTable("TBL_ROCLAND_RELEVO_RONDINES");
        b.HasKey(x => x.Id);
        b.Property(x => x.Turno).HasMaxLength(20).IsRequired();
        b.Property(x => x.Estado).HasMaxLength(20).HasDefaultValue("Pendiente").IsRequired();
        b.Property(x => x.NotasFinales).HasMaxLength(1000);
        b.Property(x => x.FechaCreacion).HasDefaultValueSql("GETDATE()");

        b.HasOne(x => x.GuardiaSaliente)
         .WithMany()
         .HasForeignKey(x => x.GuardiaSalienteId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.GuardiaEntrante)
         .WithMany()
         .HasForeignKey(x => x.GuardiaEntranteId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}