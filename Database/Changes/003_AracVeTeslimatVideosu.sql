/*
    003 - Arac ve teslimat videosu (2026-11-21)

    Panelden araca ve teslimata MP4 video yuklenir (en cok 100 MB, sunucuda sikistirma yok). Afis videonun ilk
    karesidir (WebP). VideoWidth / VideoHeight afisin boyutudur (orani videoyla ayni, dikey video icin).

      Inventory.Vehicles : VideoPath, VideoPosterPath (1280 px), VideoPosterSmallPath (480 px), VideoWidth, VideoHeight
      Crm.Deliveries     : VideoPath, VideoPosterPath (1600 px), VideoWidth, VideoHeight

    Vehicles.VideoUrl (YouTube baglantisi) oldugu gibi kalir. Mevcut kayitlarda video yoktur (kolonlar NULL).

    Tekrar calistirilabilir: kolon varsa eklenmez.
    Uygulamanin yeni surumunden once calistirilir (yeni surum bu kolonlari okur; eski surum onlari gormez).

    Calistirma: sqlcmd -S <sunucu> -d <veritabani> -I -b -i 003_AracVeTeslimatVideosu.sql
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

IF COL_LENGTH(N'[Inventory].[Vehicles]', N'VideoPath') IS NULL
    ALTER TABLE [Inventory].[Vehicles] ADD [VideoPath] nvarchar(300) NULL;
GO

IF COL_LENGTH(N'[Inventory].[Vehicles]', N'VideoPosterPath') IS NULL
    ALTER TABLE [Inventory].[Vehicles] ADD [VideoPosterPath] nvarchar(300) NULL;
GO

IF COL_LENGTH(N'[Inventory].[Vehicles]', N'VideoPosterSmallPath') IS NULL
    ALTER TABLE [Inventory].[Vehicles] ADD [VideoPosterSmallPath] nvarchar(300) NULL;
GO

IF COL_LENGTH(N'[Inventory].[Vehicles]', N'VideoWidth') IS NULL
    ALTER TABLE [Inventory].[Vehicles] ADD [VideoWidth] int NULL;
GO

IF COL_LENGTH(N'[Inventory].[Vehicles]', N'VideoHeight') IS NULL
    ALTER TABLE [Inventory].[Vehicles] ADD [VideoHeight] int NULL;
GO

IF COL_LENGTH(N'[Crm].[Deliveries]', N'VideoPath') IS NULL
    ALTER TABLE [Crm].[Deliveries] ADD [VideoPath] nvarchar(300) NULL;
GO

IF COL_LENGTH(N'[Crm].[Deliveries]', N'VideoPosterPath') IS NULL
    ALTER TABLE [Crm].[Deliveries] ADD [VideoPosterPath] nvarchar(300) NULL;
GO

IF COL_LENGTH(N'[Crm].[Deliveries]', N'VideoWidth') IS NULL
    ALTER TABLE [Crm].[Deliveries] ADD [VideoWidth] int NULL;
GO

IF COL_LENGTH(N'[Crm].[Deliveries]', N'VideoHeight') IS NULL
    ALTER TABLE [Crm].[Deliveries] ADD [VideoHeight] int NULL;
GO

PRINT N'003_AracVeTeslimatVideosu tamamlandi.';
GO
