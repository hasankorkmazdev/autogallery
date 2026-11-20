/*
    001 - Seed Id araligi (2026-11-20)

    Seed artik Id bazlidir (DbSeeder): koddaki kayitlar sabit Id'leriyle (100000'in altinda) yalniz yoksa eklenir, var
    olana dokunulmaz.
    Panelden eklenen kayitlar seed Id'leriyle cakismasin diye seed'li int tablolarin identity'si 100000'den baslar
    (AppDbContext.SeedIdLimit). Bu betik mevcut veritabaninda bir sonraki identity degerini 100000 yapar; tekrar
    calistirilabilir (zaten 100000 ve uzerindeyse dokunmaz).

    DIKKAT: Eski (ada gore) seed ile kurulmus veritabaninda katalog, renk, donanim, sahne ve teslimat Id'leri koddaki
    seed Id'leriyle eslesmez; Seed:Enabled acikken seed model / paketleri yanlis markaya ekleyebilir. Boyle bir
    veritabani bu betikle degil, bos veritabaninda FatihTasci-schema.sql + seed ile yeniden kurulur.

    Calistirma: sqlcmd -S <sunucu> -d <veritabani> -I -b -i 001_SeedIdAraligi.sql
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

DECLARE @tables TABLE (Name nvarchar(128) NOT NULL);
INSERT @tables (Name) VALUES
    (N'[Catalog].[Brands]'), (N'[Catalog].[Series]'), (N'[Catalog].[CarModels]'), (N'[Catalog].[Colors]'),
    (N'[Inventory].[Features]'), (N'[Common].[ShowroomScenes]'), (N'[Crm].[Deliveries]');

DECLARE @name nvarchar(128), @last bigint, @reseed int, @sql nvarchar(400);
DECLARE tables_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT Name FROM @tables;
OPEN tables_cursor;
FETCH NEXT FROM tables_cursor INTO @name;
WHILE @@FETCH_STATUS = 0
BEGIN
    -- last_value: son verilen Id; hic satir eklenmemisse NULL (o zaman IDENT_CURRENT tohumu gosterir).
    SELECT @last = CAST(last_value AS bigint) FROM sys.identity_columns WHERE object_id = OBJECT_ID(@name);
    IF ISNULL(@last, IDENT_CURRENT(@name) - 1) < 99999
    BEGIN
        -- Hic satir eklenmemis tabloda sonraki deger RESEED degerinin kendisi, eklenmis tabloda RESEED + 1'dir.
        SET @reseed = CASE WHEN @last IS NULL THEN 100000 ELSE 99999 END;
        SET @sql = N'DBCC CHECKIDENT (''' + @name + N''', RESEED, ' + CAST(@reseed AS nvarchar(10)) + N') WITH NO_INFOMSGS;';
        EXEC sp_executesql @sql;
        PRINT N'Sonraki Id 100000: ' + @name;
    END
    FETCH NEXT FROM tables_cursor INTO @name;
END
CLOSE tables_cursor;
DEALLOCATE tables_cursor;
GO
