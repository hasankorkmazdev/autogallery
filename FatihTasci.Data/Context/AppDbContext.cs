using FatihTasci.Core.Entity.Base;
using FatihTasci.Core.Entity.Catalog;
using FatihTasci.Core.Entity.Common;
using FatihTasci.Core.Entity.Crm;
using FatihTasci.Core.Entity.Identity;
using FatihTasci.Core.Entity.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FatihTasci.Data.Context;

/// <summary>
/// Şemalar: Identity, Catalog, Inventory, Crm, Common (tablo şemaları entity'lerin [Table] attribute'unda).
/// </summary>
public class AppDbContext : DbContext
{
    public const string ListingNoSequence = "ListingNumbers";

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<BrandEntity> Brands { get; set; }
    public DbSet<SeriesEntity> Series { get; set; }
    public DbSet<CarModelEntity> CarModels { get; set; }
    public DbSet<ColorEntity> Colors { get; set; }
    public DbSet<VehicleEntity> Vehicles { get; set; }
    public DbSet<VehicleBodyPartEntity> VehicleBodyParts { get; set; }
    public DbSet<FeatureEntity> Features { get; set; }
    public DbSet<VehicleFeatureEntity> VehicleFeatures { get; set; }
    public DbSet<VehicleMediaEntity> VehicleMedia { get; set; }
    public DbSet<LeadEntity> Leads { get; set; }
    public DbSet<LeadMediaEntity> LeadMedia { get; set; }
    public DbSet<DeliveryEntity> Deliveries { get; set; }
    public DbSet<SiteSettingEntity> SiteSettings { get; set; }
    public DbSet<SlugRedirectEntity> SlugRedirects { get; set; }
    public DbSet<ShowroomSceneEntity> ShowroomScenes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureIdentity(modelBuilder);
        ConfigureCatalog(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureCrm(modelBuilder);
        ConfigureCommon(modelBuilder);
        ApplySoftDeleteFilters(modelBuilder);

        // Tüm ilişkilerde zincirleme silme kapalı; kayıtlar soft delete ile silinir.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(x => x.GetForeignKeys()))
        {
            if (!foreignKey.IsOwnership && foreignKey.DeleteBehavior == DeleteBehavior.Cascade)
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(60).IsRequired();
            e.Property(x => x.Surname).HasMaxLength(60).IsRequired();
            e.Property(x => x.EMail).HasMaxLength(150).IsRequired();
            e.Property(x => x.NormalizedEMail).HasMaxLength(150).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.NormalizedEMail).IsUnique().HasFilter("[IsDelete] = 0");
        });
    }

    private static void ConfigureCatalog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BrandEntity>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(60).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(80).IsRequired();
            e.Property(x => x.LogoPath).HasMaxLength(300);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDelete] = 0");
        });

        modelBuilder.Entity<SeriesEntity>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Brand).WithMany(x => x.Series).HasForeignKey(x => x.BrandId);
            e.HasIndex(x => new { x.BrandId, x.Slug }).IsUnique().HasFilter("[IsDelete] = 0");
        });

        modelBuilder.Entity<CarModelEntity>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(140).IsRequired();
            e.HasOne(x => x.Series).WithMany(x => x.Models).HasForeignKey(x => x.SeriesId);
            e.HasIndex(x => new { x.SeriesId, x.Slug }).IsUnique().HasFilter("[IsDelete] = 0");
        });

        modelBuilder.Entity<ColorEntity>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(40).IsRequired();
            e.Property(x => x.HexCode).HasMaxLength(9);
        });
    }

    private static void ConfigureInventory(ModelBuilder modelBuilder)
    {
        // İlan no: 100000'den başlayan sequence
        modelBuilder.HasSequence<int>(ListingNoSequence, "Inventory").StartsAt(100000).IncrementsBy(1);

        modelBuilder.Entity<VehicleEntity>(e =>
        {
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.ListingNo).HasDefaultValueSql($"NEXT VALUE FOR [Inventory].[{ListingNoSequence}]");
            e.HasIndex(x => x.ListingNo).IsUnique();
            e.Property(x => x.Slug).HasMaxLength(200);
            e.Property(x => x.Title).HasMaxLength(150).IsRequired();
            e.Property(x => x.InteriorColor).HasMaxLength(50);
            e.Property(x => x.ContactName).HasMaxLength(80);
            e.Property(x => x.ContactPhone).HasMaxLength(20);
            e.Property(x => x.TramerAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.CampaignPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.VideoUrl).HasMaxLength(300);
            e.Property(x => x.ExpertiseReportPath).HasMaxLength(300);
            e.Property(x => x.WarrantyEndDate).HasColumnType("date");
            e.HasIndex(x => new { x.Status, x.PublishedAt });
            e.HasIndex(x => x.CarModelId);

            e.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId);
            e.HasOne(x => x.Series).WithMany().HasForeignKey(x => x.SeriesId);
            e.HasOne(x => x.CarModel).WithMany().HasForeignKey(x => x.CarModelId);
            e.HasOne(x => x.ExteriorColor).WithMany().HasForeignKey(x => x.ExteriorColorId);
        });

        modelBuilder.Entity<VehicleBodyPartEntity>(e =>
        {
            e.Property(x => x.Note).HasMaxLength(200);
            e.HasOne(x => x.Vehicle).WithMany(x => x.BodyParts).HasForeignKey(x => x.VehicleId);
            e.HasIndex(x => new { x.VehicleId, x.Part }).IsUnique();
        });

        modelBuilder.Entity<FeatureEntity>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.HasIndex(x => new { x.Category, x.Name }).IsUnique();
        });

        modelBuilder.Entity<VehicleFeatureEntity>(e =>
        {
            e.HasKey(x => new { x.VehicleId, x.FeatureId });
            e.HasOne(x => x.Vehicle).WithMany(x => x.Features).HasForeignKey(x => x.VehicleId);
            e.HasOne(x => x.Feature).WithMany().HasForeignKey(x => x.FeatureId);
        });

        modelBuilder.Entity<VehicleMediaEntity>(e =>
        {
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.OriginalPath).HasMaxLength(300).IsRequired();
            e.Property(x => x.SmallPath).HasMaxLength(300).IsRequired();
            e.Property(x => x.MediumPath).HasMaxLength(300).IsRequired();
            e.Property(x => x.LargePath).HasMaxLength(300).IsRequired();
            e.Property(x => x.AltText).HasMaxLength(200);
            e.HasOne(x => x.Vehicle).WithMany(x => x.Media).HasForeignKey(x => x.VehicleId);
            e.HasIndex(x => new { x.VehicleId, x.SortOrder });
        });
    }

    private static void ConfigureCrm(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeadEntity>(e =>
        {
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Message).HasMaxLength(2000);
            e.Property(x => x.SellBrand).HasMaxLength(60);
            e.Property(x => x.SellModel).HasMaxLength(120);
            e.Property(x => x.SellPackage).HasMaxLength(120);
            e.Property(x => x.SellExpectedPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.InternalNote).HasMaxLength(2000);
            e.Property(x => x.IpAddress).HasMaxLength(45);
            e.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId);
            e.HasIndex(x => new { x.Status, x.CreateDate });
        });

        modelBuilder.Entity<LeadMediaEntity>(e =>
        {
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Path).HasMaxLength(300).IsRequired();
            e.HasOne(x => x.Lead).WithMany(x => x.Media).HasForeignKey(x => x.LeadId);
            e.HasIndex(x => new { x.LeadId, x.SortOrder });
        });

        modelBuilder.Entity<DeliveryEntity>(e =>
        {
            e.Property(x => x.VehicleTitle).HasMaxLength(80).IsRequired();
            e.Property(x => x.CustomerName).HasMaxLength(60);
            e.Property(x => x.City).HasMaxLength(40);
            e.Property(x => x.DeliveredAt).HasColumnType("date");
            e.Property(x => x.OriginalPath).HasMaxLength(300);
            e.Property(x => x.MediumPath).HasMaxLength(300);
            e.Property(x => x.LargePath).HasMaxLength(300);
            e.HasIndex(x => new { x.IsActive, x.DeliveredAt });
        });
    }

    private static void ConfigureCommon(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SiteSettingEntity>(e =>
        {
            e.Property(x => x.Key).HasMaxLength(60).IsRequired();
            e.Property(x => x.Value).HasMaxLength(4000);
            e.HasIndex(x => x.Key).IsUnique();
        });

        modelBuilder.Entity<ShowroomSceneEntity>(e =>
        {
            e.Property(x => x.Tag).HasMaxLength(40).IsRequired();
            e.Property(x => x.Title).HasMaxLength(80).IsRequired();
            e.Property(x => x.OriginalPath).HasMaxLength(300);
            e.Property(x => x.MediumPath).HasMaxLength(300);
            e.Property(x => x.LargePath).HasMaxLength(300);
            e.HasIndex(x => x.SortOrder);
        });

        modelBuilder.Entity<SlugRedirectEntity>(e =>
        {
            e.Property(x => x.OldSlug).HasMaxLength(140).IsRequired();
            e.HasIndex(x => new { x.EntityType, x.ParentId, x.OldSlug });
        });
    }

    /// <summary>
    /// ISoftDeletableEntity olan her entity'ye "IsDelete = false" global filtresi eklenir.
    /// </summary>
    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "x");
            var body = Expression.Equal(
                Expression.Property(parameter, nameof(ISoftDeletableEntity.IsDelete)),
                Expression.Constant(false));
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
        }
    }
}
