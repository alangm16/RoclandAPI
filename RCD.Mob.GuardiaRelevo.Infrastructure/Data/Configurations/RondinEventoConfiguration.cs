using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations;

public class RondinEventoConfiguration : IEntityTypeConfiguration<RondinEvento>
{
    public void Configure(EntityTypeBuilder<RondinEvento> b)
    {
        b.ToTable("TBL_ROCLAND_RELEVO_RONDIN_EVENTOS");
        b.HasKey(x => x.Id);
        b.Property(x => x.TipoEvento).HasMaxLength(20).IsRequired();
        b.Property(x => x.TipoGuardia).HasMaxLength(20).IsRequired();
        b.Property(x => x.FirmaBase64).HasColumnType("NVARCHAR(MAX)");
        b.Property(x => x.Exitoso).HasDefaultValue(true);
        b.Property(x => x.FechaEvento).HasDefaultValueSql("GETDATE()");

        b.HasOne(x => x.Rondin).WithMany(r => r.Eventos)
         .HasForeignKey(x => x.RondinId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Usuario).WithMany()
         .HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}