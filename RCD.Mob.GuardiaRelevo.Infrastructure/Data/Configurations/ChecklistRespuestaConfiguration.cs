using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.Mob.GuardiaRelevo.Domain.Entities;

namespace RCD.Mob.GuardiaRelevo.Infrastructure.Data.Configurations;

public class ChecklistRespuestaConfiguration : IEntityTypeConfiguration<ChecklistRespuesta>
{
    public void Configure(EntityTypeBuilder<ChecklistRespuesta> builder)
    {
        builder.ToTable("TBL_ROCLAND_RELEVO_CHECKLIST_RESPUESTAS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RondinId).IsRequired();
        builder.Property(x => x.PuntoId).IsRequired();

        // El booleano que dice si está limpio (1) o sucio (0)
        builder.Property(x => x.Respuesta).IsRequired();

        builder.Property(x => x.Comentario).HasMaxLength(500);

        builder.Property(x => x.FechaRespuesta)
               .HasDefaultValueSql("GETDATE()")
               .IsRequired();

        // ¡Aquí está la magia de la v3.0! Mapeamos el Constraint UNIQUE
        builder.HasIndex(x => new { x.RondinId, x.PuntoId }).IsUnique();

        // Relación con Rondin (Alineado a Restrict para que no se borren respuestas por accidente)
        builder.HasOne(x => x.Rondin)
               .WithMany(r => r.Respuestas)
               .HasForeignKey(x => x.RondinId)
               .OnDelete(DeleteBehavior.Restrict);

        // Relación con el Catálogo de Puntos
        builder.HasOne(x => x.Punto)
               .WithMany()
               .HasForeignKey(x => x.PuntoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}