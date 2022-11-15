using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silky.EntityFrameworkCore.Extras.Entities;
using Silky.Hero.Common.EntityFrameworkCore.Entities;

namespace Silky.EntityFrameworkCore.Extras.Modeling;

public static class EntityTypeBuilderExtensions
{
    public static void ConfigureByConvention(this EntityTypeBuilder b)
    {
        b.TryConfigureConcurrencyStamp();
        b.TryConfigureFullAuditedEntity();
        b.TryConfigureAuditedEntity();
    }

    private static void TryConfigureConcurrencyStamp(this EntityTypeBuilder b)
    {
        if (b.Metadata.ClrType.IsAssignableTo<IHasConcurrencyStamp>())
        {
            b.Property(nameof(IHasConcurrencyStamp.ConcurrencyStamp))
                .IsConcurrencyToken()
                .HasMaxLength(ConcurrencyStampConsts.MaxLength)
                .HasColumnName(nameof(IHasConcurrencyStamp.ConcurrencyStamp));
        }
    }

    private static void TryConfigureAuditedEntity(this EntityTypeBuilder b)
    {
        if (b.Metadata.ClrType.IsAssignableTo<AuditedEntity>())
        {
            b.Property(nameof(AuditedEntity.CreatedTime))
                .HasColumnName(nameof(FullAuditedEntity.CreatedTime));
            
            b.Property(nameof(AuditedEntity.UpdatedTime))
                .HasColumnName(nameof(FullAuditedEntity.UpdatedTime));

            b.Property(nameof(FullAuditedEntity.TenantId))
                .IsRequired(false)
                .HasColumnName(nameof(FullAuditedEntity.TenantId));
        }
        
        if (b.Metadata.ClrType.IsAssignableTo<AuditedEntityWithGuid>())
        {
            b.Property(nameof(AuditedEntityWithGuid.CreatedTime))
                .HasColumnName(nameof(AuditedEntityWithGuid.CreatedTime));
            
            b.Property(nameof(AuditedEntityWithGuid.UpdatedTime))
                .HasColumnName(nameof(AuditedEntityWithGuid.UpdatedTime));

            b.Property(nameof(AuditedEntityWithGuid.TenantId))
                .IsRequired(false)
                .HasColumnName(nameof(AuditedEntityWithGuid.TenantId));
        }
    }

    private static void TryConfigureFullAuditedEntity(this EntityTypeBuilder b)
    {
        if (b.Metadata.ClrType.IsAssignableTo<FullAuditedEntity>())
        {
            b.Property(nameof(FullAuditedEntity.CreatedBy))
                .IsRequired(false)
                .HasColumnName(nameof(FullAuditedEntity.CreatedBy));
            b.Property(nameof(FullAuditedEntity.CreatedTime))
                .HasColumnName(nameof(FullAuditedEntity.CreatedTime));
            
            b.Property(nameof(FullAuditedEntity.UpdatedBy))
                .IsRequired(false)
                .HasColumnName(nameof(FullAuditedEntity.UpdatedBy));
            b.Property(nameof(FullAuditedEntity.UpdatedTime))
                .HasColumnName(nameof(FullAuditedEntity.UpdatedTime));

            b.Property(nameof(FullAuditedEntity.DeletedBy))
                .IsRequired(false)
                .HasColumnName(nameof(FullAuditedEntity.DeletedBy));
            b.Property(nameof(FullAuditedEntity.IsDeleted))
                .HasColumnName(nameof(FullAuditedEntity.IsDeleted));
            b.Property(nameof(FullAuditedEntity.DeletedTime))
                .IsRequired(false)
                .HasColumnName(nameof(FullAuditedEntity.DeletedTime));
            
            b.Property(nameof(FullAuditedEntity.TenantId))
                .IsRequired(false)
                .HasColumnName(nameof(FullAuditedEntity.TenantId));
        }
        
        if (b.Metadata.ClrType.IsAssignableTo<FullAuditedEntityWithGuid>())
        {
            b.Property(nameof(FullAuditedEntityWithGuid.CreatedBy))
                .IsRequired(false)
                .HasColumnName(nameof(FullAuditedEntityWithGuid.CreatedBy));
            b.Property(nameof(FullAuditedEntityWithGuid.CreatedTime))
                .HasColumnName(nameof(FullAuditedEntityWithGuid.CreatedTime));
            
            b.Property(nameof(FullAuditedEntityWithGuid.UpdatedBy))
                .IsRequired(false)
                .HasColumnName(nameof(FullAuditedEntityWithGuid.UpdatedBy));
            b.Property(nameof(FullAuditedEntityWithGuid.UpdatedTime))
                .HasColumnName(nameof(FullAuditedEntityWithGuid.UpdatedTime));

            b.Property(nameof(FullAuditedEntityWithGuid.DeletedBy))
                .IsRequired(false)
                .HasColumnName(nameof(FullAuditedEntityWithGuid.DeletedBy));
            b.Property(nameof(FullAuditedEntityWithGuid.IsDeleted))
                .HasColumnName(nameof(FullAuditedEntityWithGuid.IsDeleted));
            b.Property(nameof(FullAuditedEntityWithGuid.DeletedTime))
                .IsRequired(false)
                .HasColumnName(nameof(FullAuditedEntityWithGuid.DeletedTime));
            
            b.Property(nameof(FullAuditedEntityWithGuid.TenantId))
                .IsRequired(false)
                .HasColumnName(nameof(FullAuditedEntityWithGuid.TenantId));
        }
    }
}