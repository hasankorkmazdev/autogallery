/*
    002 - Arac danismani (2026-11-20)

    Her aracin bir satis danismani (panel kullanicisi) olur: Inventory.Vehicles.AdvisorId -> Identity.Users.Id.
    Kullanicinin sitede gorunen iletisim telefonu Identity.Users.Phone'dadir (giris e-postasindan ve galeri
    telefonundan bagimsiz; bossa ilanda galeri telefonu kullanilir). Serbest metin "ilan yetkilisi" kolonlari
    (Vehicles.ContactName, Vehicles.ContactPhone) kaldirilir.

    Mevcut veri, eski kolonlar silinmeden once:
      1. ContactName bir kullanicinin "Ad Soyad"iyla ayniysa (veritabani harmanlamasina gore) o kullanici aracin
         danismani olur; kullanicinin telefonu bossa o araclardaki ContactPhone ona yazilir.
      2. Eslesmeyen ilan yetkilileri sonuc olarak listelenir: panelde kullanici olarak eklenip araca atanabilir.
         Danismani bos kalan araclar sitede eskisi gibi galeri telefonunu gosterir; panelde Fiyat adiminda
         danisman secilmeden kaydedilemez, yayin kontrolunde de eksik gorunur.

    Tekrar calistirilabilir: kolon / anahtar / indeks varsa eklenmez, eski kolonlar yoksa 1. ve 2. adim atlanir.
    Uygulamanin yeni surumuyle birlikte calistirilir (eski surum ContactName kolonunu arar, yeni surum AdvisorId'yi).

    Calistirma: sqlcmd -S <sunucu> -d <veritabani> -I -b -i 002_AracDanismani.sql
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

IF COL_LENGTH(N'[Identity].[Users]', N'Phone') IS NULL
    ALTER TABLE [Identity].[Users] ADD [Phone] nvarchar(20) NULL;
GO

IF COL_LENGTH(N'[Inventory].[Vehicles]', N'AdvisorId') IS NULL
    ALTER TABLE [Inventory].[Vehicles] ADD [AdvisorId] uniqueidentifier NULL;
GO

IF OBJECT_ID(N'[Inventory].[FK_Vehicles_Users_AdvisorId]', N'F') IS NULL
    ALTER TABLE [Inventory].[Vehicles] ADD CONSTRAINT [FK_Vehicles_Users_AdvisorId]
        FOREIGN KEY ([AdvisorId]) REFERENCES [Identity].[Users] ([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Vehicles_AdvisorId' AND object_id = OBJECT_ID(N'[Inventory].[Vehicles]'))
    CREATE INDEX [IX_Vehicles_AdvisorId] ON [Inventory].[Vehicles] ([AdvisorId]);
GO

-- Eski kolonlar yalniz eski veritabaninda var; dinamik SQL ile calisir (yoksa betik derlenirken hata vermesin).
IF COL_LENGTH(N'[Inventory].[Vehicles]', N'ContactName') IS NOT NULL
BEGIN
    -- 1. Ilan yetkilisini ayni ad soyadli kullaniciyla eslestir, telefonu bos kullaniciya yetkili telefonunu yaz.
    EXEC(N'
        UPDATE v SET v.AdvisorId = u.Id
        FROM [Inventory].[Vehicles] v
        JOIN [Identity].[Users] u ON u.IsDelete = 0 AND LTRIM(RTRIM(v.ContactName)) = u.Name + N'' '' + u.Surname
        WHERE v.AdvisorId IS NULL;

        UPDATE u SET u.Phone = p.ContactPhone
        FROM [Identity].[Users] u
        CROSS APPLY (
            SELECT TOP 1 LTRIM(RTRIM(v.ContactPhone)) AS ContactPhone
            FROM [Inventory].[Vehicles] v
            WHERE v.AdvisorId = u.Id
              AND LTRIM(RTRIM(v.ContactName)) = u.Name + N'' '' + u.Surname
              AND LTRIM(RTRIM(ISNULL(v.ContactPhone, N''''))) <> N''''
            ORDER BY ISNULL(v.LastUpdateDate, v.CreateDate) DESC) p
        WHERE u.Phone IS NULL;
    ');

    -- 2. Kullaniciyla eslesmeyen ilan yetkilileri (silinmeden once listelenir).
    PRINT N'Kullaniciyla eslesmeyen ilan yetkilileri (varsa asagida; panelde kullanici ekleyip araca danisman atayin):';
    EXEC(N'
        SELECT v.ListingNo AS IlanNo, v.Title AS Baslik, v.ContactName AS IlanYetkilisi, v.ContactPhone AS Telefon
        FROM [Inventory].[Vehicles] v
        WHERE v.IsDelete = 0
          AND (LTRIM(RTRIM(ISNULL(v.ContactName, N''''))) <> N'''' OR LTRIM(RTRIM(ISNULL(v.ContactPhone, N''''))) <> N'''')
          AND NOT EXISTS (
              SELECT 1 FROM [Identity].[Users] u
              WHERE u.Id = v.AdvisorId AND LTRIM(RTRIM(v.ContactName)) = u.Name + N'' '' + u.Surname)
        ORDER BY v.ListingNo;
    ');

    ALTER TABLE [Inventory].[Vehicles] DROP COLUMN [ContactName];
END
GO

IF COL_LENGTH(N'[Inventory].[Vehicles]', N'ContactPhone') IS NOT NULL
    ALTER TABLE [Inventory].[Vehicles] DROP COLUMN [ContactPhone];
GO
