using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RCD.SuperAdmin.Domain.Entities;

namespace RCD.SuperAdmin.Infrastructure.Data.Configurations
{
    public class RolConfiguration : IEntityTypeConfiguration<Rol>
    {
        public void Configure(EntityTypeBuilder<Rol> builder)
        {
            builder.ToTable("TBL_ROCLAND_SUPERADMIN_ROLES");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Nombre).HasMaxLength(60).IsRequired();
            builder.Property(r => r.Activo).IsRequired();
        }
    }
}
