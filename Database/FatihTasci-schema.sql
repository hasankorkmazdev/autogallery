/*
    Fatih Tasci Otomotiv (Galeria) - TAM VERITABANI SEMASI (SQL Server)

    BOS bir veritabaninda bir kez calistirilir (yeni kurulum). Mevcut veritabanina calistirmayin: tablolar zaten
    varsa hata verir. Mevcut veritabanindaki degisiklikler Database/Changes/ altindaki betiklerle yapilir.

    Proje EF Core migration KULLANMAZ. Bu dosya guncel modelden uretilir:
      dotnet ef dbcontext script --project FatihTasci.Data --startup-project FatihTasci.Server -o Database/FatihTasci-schema.raw.sql
    ardindan basina bu aciklama ve SET satirlari eklenir; dosya BOM'suz ve yalniz ASCII tutulur (bazi SQL araclari
    BOM'u kod sanip "Incorrect syntax near" hatasi verir).

    Calistirma: SSMS'te dosyayi acip calistirin ya da
      sqlcmd -S <sunucu> -d <veritabani> -U <kullanici> -P <sifre> -I -b -i FatihTasci-schema.sql

    Yalniz sema: katalog (marka / model / paket), renkler, donanimlar, site ayarlari ve Yonetici, uygulama acilirken
    Seed:Enabled aciksa DbSeeder ile Id'leriyle eklenir (ilk kurulumda acik olmali). Seed'li int tablolarin identity'si
    100000'den baslar: seed Id'leri bunun altinda kalir, panelden eklenen kayitlar 100000'den baslar. Yayin
    ayarlarinda ConnectionStrings:DefaultConnection ve JwtSettings:Secret (en az 32 karakter) dolu olmalidir.

    Icerik: 16 tablo, 22 indeks.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;  -- filtreli indeksler icin gerekli (sqlcmd varsayilani OFF)
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
GO

IF SCHEMA_ID(N'Catalog') IS NULL EXEC(N'CREATE SCHEMA [Catalog];');
GO


IF SCHEMA_ID(N'Crm') IS NULL EXEC(N'CREATE SCHEMA [Crm];');
GO


IF SCHEMA_ID(N'Inventory') IS NULL EXEC(N'CREATE SCHEMA [Inventory];');
GO


IF SCHEMA_ID(N'Common') IS NULL EXEC(N'CREATE SCHEMA [Common];');
GO


IF SCHEMA_ID(N'Identity') IS NULL EXEC(N'CREATE SCHEMA [Identity];');
GO


CREATE SEQUENCE [Inventory].[ListingNumbers] AS int START WITH 100000 INCREMENT BY 1 NO CYCLE;
GO


CREATE TABLE [Catalog].[Brands] (
    [Id] int NOT NULL IDENTITY(100000, 1),
    [IsActive] bit NOT NULL,
    [IsDelete] bit NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    [LastUpdateDate] datetime2 NULL,
    [LastDeleteDate] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [LastDeletedBy] uniqueidentifier NULL,
    [LastUpdatedBy] uniqueidentifier NULL,
    [Name] nvarchar(60) NOT NULL,
    [Slug] nvarchar(80) NOT NULL,
    [LogoPath] nvarchar(300) NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_Brands] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Catalog].[Colors] (
    [Id] int NOT NULL IDENTITY(100000, 1),
    [IsActive] bit NOT NULL,
    [Name] nvarchar(40) NOT NULL,
    [HexCode] nvarchar(9) NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_Colors] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Crm].[Deliveries] (
    [Id] int NOT NULL IDENTITY(100000, 1),
    [IsActive] bit NOT NULL,
    [IsDelete] bit NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    [LastUpdateDate] datetime2 NULL,
    [LastDeleteDate] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [LastDeletedBy] uniqueidentifier NULL,
    [LastUpdatedBy] uniqueidentifier NULL,
    [VehicleTitle] nvarchar(80) NOT NULL,
    [CustomerName] nvarchar(60) NULL,
    [City] nvarchar(40) NULL,
    [DeliveredAt] date NOT NULL,
    [OriginalPath] nvarchar(300) NULL,
    [MediumPath] nvarchar(300) NULL,
    [LargePath] nvarchar(300) NULL,
    CONSTRAINT [PK_Deliveries] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Inventory].[Features] (
    [Id] int NOT NULL IDENTITY(100000, 1),
    [IsActive] bit NOT NULL,
    [Category] int NOT NULL,
    [Name] nvarchar(80) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_Features] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Common].[ShowroomScenes] (
    [Id] int NOT NULL IDENTITY(100000, 1),
    [IsActive] bit NOT NULL,
    [IsDelete] bit NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    [LastUpdateDate] datetime2 NULL,
    [LastDeleteDate] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [LastDeletedBy] uniqueidentifier NULL,
    [LastUpdatedBy] uniqueidentifier NULL,
    [Tag] nvarchar(40) NOT NULL,
    [Title] nvarchar(80) NOT NULL,
    [SortOrder] int NOT NULL,
    [OriginalPath] nvarchar(300) NULL,
    [MediumPath] nvarchar(300) NULL,
    [LargePath] nvarchar(300) NULL,
    CONSTRAINT [PK_ShowroomScenes] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Common].[SiteSettings] (
    [Id] int NOT NULL IDENTITY,
    [IsActive] bit NOT NULL,
    [Key] nvarchar(60) NOT NULL,
    [Value] nvarchar(4000) NULL,
    CONSTRAINT [PK_SiteSettings] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Common].[SlugRedirects] (
    [Id] int NOT NULL IDENTITY,
    [IsActive] bit NOT NULL,
    [EntityType] int NOT NULL,
    [ParentId] int NULL,
    [OldSlug] nvarchar(140) NOT NULL,
    [EntityId] int NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    CONSTRAINT [PK_SlugRedirects] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Identity].[Users] (
    [Id] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    [IsDelete] bit NOT NULL,
    [Name] nvarchar(60) NOT NULL,
    [Surname] nvarchar(60) NOT NULL,
    [EMail] nvarchar(150) NOT NULL,
    [NormalizedEMail] nvarchar(150) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [Role] int NOT NULL,
    [LastLoginDate] datetime2 NULL,
    [Phone] nvarchar(20) NULL,
    [CreateDate] datetime2 NOT NULL,
    [LastUpdateDate] datetime2 NULL,
    [LastDeleteDate] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [LastDeletedBy] uniqueidentifier NULL,
    [LastUpdatedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Catalog].[Series] (
    [Id] int NOT NULL IDENTITY(100000, 1),
    [IsActive] bit NOT NULL,
    [IsDelete] bit NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    [LastUpdateDate] datetime2 NULL,
    [LastDeleteDate] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [LastDeletedBy] uniqueidentifier NULL,
    [LastUpdatedBy] uniqueidentifier NULL,
    [BrandId] int NOT NULL,
    [Name] nvarchar(80) NOT NULL,
    [Slug] nvarchar(100) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_Series] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Series_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Catalog].[Brands] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Catalog].[CarModels] (
    [Id] int NOT NULL IDENTITY(100000, 1),
    [IsActive] bit NOT NULL,
    [IsDelete] bit NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    [LastUpdateDate] datetime2 NULL,
    [LastDeleteDate] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [LastDeletedBy] uniqueidentifier NULL,
    [LastUpdatedBy] uniqueidentifier NULL,
    [SeriesId] int NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    [Slug] nvarchar(140) NOT NULL,
    CONSTRAINT [PK_CarModels] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CarModels_Series_SeriesId] FOREIGN KEY ([SeriesId]) REFERENCES [Catalog].[Series] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Inventory].[Vehicles] (
    [Id] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    [IsDelete] bit NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    [LastUpdateDate] datetime2 NULL,
    [LastDeleteDate] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [LastDeletedBy] uniqueidentifier NULL,
    [LastUpdatedBy] uniqueidentifier NULL,
    [ListingNo] int NOT NULL DEFAULT (NEXT VALUE FOR [Inventory].[ListingNumbers]),
    [Slug] nvarchar(200) NULL,
    [Status] int NOT NULL,
    [BrandId] int NOT NULL,
    [SeriesId] int NOT NULL,
    [CarModelId] int NOT NULL,
    [Title] nvarchar(150) NOT NULL,
    [Year] int NULL,
    [Kilometer] int NULL,
    [FuelType] int NULL,
    [Transmission] int NULL,
    [BodyType] int NULL,
    [EngineCc] int NULL,
    [PowerHp] int NULL,
    [Drivetrain] int NULL,
    [BatteryKwh] int NULL,
    [RangeKm] int NULL,
    [ExteriorColorId] int NULL,
    [InteriorColor] nvarchar(50) NULL,
    [Condition] int NOT NULL,
    [HasWarranty] bit NOT NULL,
    [WarrantyEndDate] date NULL,
    [HeavyDamageRecord] bit NOT NULL,
    [PlateOrigin] int NOT NULL,
    [AcceptsTrade] bit NOT NULL,
    [TramerAmount] decimal(18,2) NULL,
    [VideoUrl] nvarchar(300) NULL,
    [ExpertiseReportPath] nvarchar(300) NULL,
    [Price] decimal(18,2) NULL,
    [CampaignPrice] decimal(18,2) NULL,
    [ShowPrice] bit NOT NULL,
    [Description] nvarchar(max) NULL,
    [AdvisorId] uniqueidentifier NULL,
    [IsFeatured] bit NOT NULL,
    [FeaturedOrder] int NOT NULL,
    [IsOpportunity] bit NOT NULL,
    [ViewCount] int NOT NULL,
    [PublishedAt] datetime2 NULL,
    [SoldAt] datetime2 NULL,
    CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Vehicles_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Catalog].[Brands] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Vehicles_CarModels_CarModelId] FOREIGN KEY ([CarModelId]) REFERENCES [Catalog].[CarModels] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Vehicles_Colors_ExteriorColorId] FOREIGN KEY ([ExteriorColorId]) REFERENCES [Catalog].[Colors] ([Id]),
    CONSTRAINT [FK_Vehicles_Series_SeriesId] FOREIGN KEY ([SeriesId]) REFERENCES [Catalog].[Series] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Vehicles_Users_AdvisorId] FOREIGN KEY ([AdvisorId]) REFERENCES [Identity].[Users] ([Id])
);
GO


CREATE TABLE [Crm].[Leads] (
    [Id] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    [IsDelete] bit NOT NULL,
    [CreateDate] datetime2 NOT NULL,
    [LastUpdateDate] datetime2 NULL,
    [LastDeleteDate] datetime2 NULL,
    [CreatedBy] uniqueidentifier NULL,
    [LastDeletedBy] uniqueidentifier NULL,
    [LastUpdatedBy] uniqueidentifier NULL,
    [Type] int NOT NULL,
    [VehicleId] uniqueidentifier NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Email] nvarchar(150) NULL,
    [Message] nvarchar(2000) NULL,
    [PreferredDate] datetime2 NULL,
    [SellBrand] nvarchar(60) NULL,
    [SellModel] nvarchar(120) NULL,
    [SellPackage] nvarchar(120) NULL,
    [SellYear] int NULL,
    [SellKm] int NULL,
    [SellExpectedPrice] decimal(18,2) NULL,
    [SellDamage] int NULL,
    [KvkkConsentAt] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [InternalNote] nvarchar(2000) NULL,
    [IpAddress] nvarchar(45) NULL,
    CONSTRAINT [PK_Leads] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Leads_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Inventory].[Vehicles] ([Id])
);
GO


CREATE TABLE [Inventory].[VehicleBodyParts] (
    [Id] int NOT NULL IDENTITY,
    [IsActive] bit NOT NULL,
    [VehicleId] uniqueidentifier NOT NULL,
    [Part] int NOT NULL,
    [State] int NOT NULL,
    [Note] nvarchar(200) NULL,
    CONSTRAINT [PK_VehicleBodyParts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VehicleBodyParts_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Inventory].[Vehicles] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Inventory].[VehicleFeatures] (
    [VehicleId] uniqueidentifier NOT NULL,
    [FeatureId] int NOT NULL,
    CONSTRAINT [PK_VehicleFeatures] PRIMARY KEY ([VehicleId], [FeatureId]),
    CONSTRAINT [FK_VehicleFeatures_Features_FeatureId] FOREIGN KEY ([FeatureId]) REFERENCES [Inventory].[Features] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_VehicleFeatures_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Inventory].[Vehicles] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Inventory].[VehicleMedia] (
    [Id] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    [VehicleId] uniqueidentifier NOT NULL,
    [OriginalPath] nvarchar(300) NOT NULL,
    [SmallPath] nvarchar(300) NOT NULL,
    [MediumPath] nvarchar(300) NOT NULL,
    [LargePath] nvarchar(300) NOT NULL,
    [Width] int NOT NULL,
    [Height] int NOT NULL,
    [SortOrder] int NOT NULL,
    [IsCover] bit NOT NULL,
    [AltText] nvarchar(200) NULL,
    [CreateDate] datetime2 NOT NULL,
    CONSTRAINT [PK_VehicleMedia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VehicleMedia_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Inventory].[Vehicles] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Crm].[LeadMedia] (
    [Id] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    [LeadId] uniqueidentifier NOT NULL,
    [Path] nvarchar(300) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_LeadMedia] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeadMedia_Leads_LeadId] FOREIGN KEY ([LeadId]) REFERENCES [Crm].[Leads] ([Id]) ON DELETE NO ACTION
);
GO


CREATE UNIQUE INDEX [IX_Brands_Slug] ON [Catalog].[Brands] ([Slug]) WHERE [IsDelete] = 0;
GO


CREATE UNIQUE INDEX [IX_CarModels_SeriesId_Slug] ON [Catalog].[CarModels] ([SeriesId], [Slug]) WHERE [IsDelete] = 0;
GO


CREATE INDEX [IX_Deliveries_IsActive_DeliveredAt] ON [Crm].[Deliveries] ([IsActive], [DeliveredAt]);
GO


CREATE UNIQUE INDEX [IX_Features_Category_Name] ON [Inventory].[Features] ([Category], [Name]);
GO


CREATE INDEX [IX_LeadMedia_LeadId_SortOrder] ON [Crm].[LeadMedia] ([LeadId], [SortOrder]);
GO


CREATE INDEX [IX_Leads_Status_CreateDate] ON [Crm].[Leads] ([Status], [CreateDate]);
GO


CREATE INDEX [IX_Leads_VehicleId] ON [Crm].[Leads] ([VehicleId]);
GO


CREATE UNIQUE INDEX [IX_Series_BrandId_Slug] ON [Catalog].[Series] ([BrandId], [Slug]) WHERE [IsDelete] = 0;
GO


CREATE INDEX [IX_ShowroomScenes_SortOrder] ON [Common].[ShowroomScenes] ([SortOrder]);
GO


CREATE UNIQUE INDEX [IX_SiteSettings_Key] ON [Common].[SiteSettings] ([Key]);
GO


CREATE INDEX [IX_SlugRedirects_EntityType_ParentId_OldSlug] ON [Common].[SlugRedirects] ([EntityType], [ParentId], [OldSlug]);
GO


CREATE UNIQUE INDEX [IX_Users_NormalizedEMail] ON [Identity].[Users] ([NormalizedEMail]) WHERE [IsDelete] = 0;
GO


CREATE UNIQUE INDEX [IX_VehicleBodyParts_VehicleId_Part] ON [Inventory].[VehicleBodyParts] ([VehicleId], [Part]);
GO


CREATE INDEX [IX_VehicleFeatures_FeatureId] ON [Inventory].[VehicleFeatures] ([FeatureId]);
GO


CREATE INDEX [IX_VehicleMedia_VehicleId_SortOrder] ON [Inventory].[VehicleMedia] ([VehicleId], [SortOrder]);
GO


CREATE INDEX [IX_Vehicles_AdvisorId] ON [Inventory].[Vehicles] ([AdvisorId]);
GO


CREATE INDEX [IX_Vehicles_BrandId] ON [Inventory].[Vehicles] ([BrandId]);
GO


CREATE INDEX [IX_Vehicles_CarModelId] ON [Inventory].[Vehicles] ([CarModelId]);
GO


CREATE INDEX [IX_Vehicles_ExteriorColorId] ON [Inventory].[Vehicles] ([ExteriorColorId]);
GO


CREATE UNIQUE INDEX [IX_Vehicles_ListingNo] ON [Inventory].[Vehicles] ([ListingNo]);
GO


CREATE INDEX [IX_Vehicles_SeriesId] ON [Inventory].[Vehicles] ([SeriesId]);
GO


CREATE INDEX [IX_Vehicles_Status_PublishedAt] ON [Inventory].[Vehicles] ([Status], [PublishedAt]);
GO
