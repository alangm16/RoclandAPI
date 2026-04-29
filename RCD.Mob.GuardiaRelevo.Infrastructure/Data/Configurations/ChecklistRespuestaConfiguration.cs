using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations;

public class ChecklistRespuestaConfiguration : IEntityTypeConfiguration<ChecklistRespuesta>
{
    public void Configure(EntityTypeBuilder<ChecklistRespuesta> b)
    {
        b.ToTable("TBL_ROCLAND_RELEVO_CHECKLIST_RESPUESTAS");
        b.HasKey(x => x.Id);
        b.Property(x => x.Comentario).HasMaxLength(500);
        b.Property(x => x.FechaRespuesta).HasDefaultValueSql("GETDATE()");

        b.HasOne(x => x.Rondin).WithMany(r => r.Respuestas)
         .HasForeignKey(x => x.RondinId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Punto).WithMany()
         .HasForeignKey(x => x.PuntoId).OnDelete(DeleteBehavior.Restrict);
    }
}