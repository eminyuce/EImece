/*
================================================================================
  EImece — Upsert page-theme CMS menus (T1–T8) with main + gallery images
================================================================================
  Idempotent. Does NOT wipe the catalog.

  Creates / updates:
    - Parent menu "Tema Ornekleri" (shown in the storefront nav)
    - Child pages "PT Dummy T1" … "PT Dummy T8" (MenuLink = pages-index)
    - Menus.MainImageId  → FileStorage.Type = MenuMainImage
    - MenuFiles rows     → FileStorage.Type = MenuGallery
      (same association as Admin → Medya:
       /admin/media/?contentId={menuId}&mod=Menus&imageType=MenuGallery)

  Physical JPEGs are written by GenerateSeedImages.ps1 for FileName LIKE
  'menu-theme-%'. FileUrl uses /media/seed/ so CleanupDummyData.sql can remove
  the FileStorage rows.

  Run:
    .\RunSeedDummyData.ps1 -ThemePages
================================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Lang         INT           = 1;
DECLARE @Now          DATETIME      = GETDATE();
DECLARE @SeedMarker   NVARCHAR(32)  = N'SEED';
DECLARE @ParentName   NVARCHAR(100) = N'Tema Ornekleri';
DECLARE @GalleryEach  INT           = 12;   -- covers PageTheme_T7 minCount
DECLARE @AdminUserId  NVARCHAR(128) =
    COALESCE(
        (SELECT TOP 1 Id FROM dbo.AspNetUsers WHERE Email = N'admin@eimece.test'),
        N'seed-admin-000000000001'
    );

DECLARE @Lorem NVARCHAR(MAX) = N'<p>Vivamus ornare, justo non eleifend pulvinar, nisl mauris tincidunt sapien, in tincidunt erat lectus sit amet magna. Integer vitae sapien sit amet lorem tincidunt pulvinar. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.</p><p>Curabitur non nulla sit amet nisl tempus convallis quis ac lectus. Nulla quis lorem ut libero malesuada feugiat. Vestibulum ac diam sit amet quam vehicula elementum sed sit amet dui.</p>';

IF OBJECT_ID(N'tempdb..#Themes') IS NOT NULL DROP TABLE #Themes;
CREATE TABLE #Themes
(
    Theme         NVARCHAR(10)  NOT NULL PRIMARY KEY,
    Name          NVARCHAR(100) NOT NULL,
    Position      INT           NOT NULL,
    Description   NVARCHAR(MAX) NOT NULL
);

INSERT INTO #Themes (Theme, Name, Position, Description) VALUES
(N'T1', N'PT Dummy T1', 1,
 N'<p>Bu sayfa <strong>PageTheme T1</strong> düzenini gösterir. Üstteki büyük görsel menünün ana resmidir (MenuMainImage). Alttaki ızgara menü galerisidir (Admin medya: <em>MenuGallery</em>).</p>' + @Lorem),
(N'T2', N'PT Dummy T2', 2,
 N'<p>Bu sayfa <strong>PageTheme T2</strong> düzenini gösterir. Ana görsel ve menü galerisi seed tarafından üretilir.</p>' + @Lorem),
(N'T3', N'PT Dummy T3', 3,
 N'<p>Bu sayfa <strong>PageTheme T3</strong> düzenini gösterir. Ana görsel ve menü galerisi seed tarafından üretilir.</p>' + @Lorem),
(N'T4', N'PT Dummy T4', 4,
 N'<p>Bu sayfa <strong>PageTheme T4</strong> düzenini gösterir. Ana görsel ve menü galerisi seed tarafından üretilir.</p>' + @Lorem),
(N'T5', N'PT Dummy T5', 5,
 N'<p>Bu sayfa <strong>PageTheme T5</strong> düzenini gösterir. Ana görsel ve menü galerisi seed tarafından üretilir.</p>' + @Lorem),
(N'T6', N'PT Dummy T6', 6,
 N'<p>Bu sayfa <strong>PageTheme T6</strong> düzenini gösterir. Ana görsel ve menü galerisi seed tarafından üretilir.</p>' + @Lorem),
(N'T7', N'PT Dummy T7', 7,
 N'<p>Bu sayfa <strong>PageTheme T7</strong> (büyük görsel galeri) düzenini gösterir. En az 12 menü galeri görseli eklenir.</p>' + @Lorem),
(N'T8', N'PT Dummy T8', 8,
 N'<h2>İletişim</h2><p>Bu sayfa <strong>PageTheme T8</strong> iletişim düzenini gösterir. Form, şirket bilgileri ve harita temada yer alır.</p><p>Sipariş, iade ve ürün sorularınız için <strong>info@eimece.test</strong> adresine yazabilirsiniz.</p><p>Çalışma saatleri: Hafta içi 09:00–18:00</p>');

BEGIN TRANSACTION;

/* ---- Parent: Tema Ornekleri (root, visible in storefront nav) ---- */
DECLARE @ParentId INT =
(
    SELECT TOP 1 Id
    FROM dbo.Menus
    WHERE Name = @ParentName AND Lang = @Lang
    ORDER BY Id
);

IF @ParentId IS NULL
BEGIN
    INSERT INTO dbo.Menus
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
         Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId,
         ParentId, MainPage, MenuLink, Link, PageTheme, LinkIsActive)
    VALUES
        (@ParentName, @Now, @Now, 1, 20, @Lang,
         N'<p>Sayfa teması örnekleri (T1–T8). Her alt sayfada ana görsel ve menü galerisi vardır.</p>',
         0, N'sayfa,tema,örnek', NULL, @AdminUserId, @SeedMarker,
         0, 1, N'pages-index', NULL, N'T1', 0);

    SET @ParentId = CAST(SCOPE_IDENTITY() AS INT);
    PRINT N'Created parent menu Tema Ornekleri Id=' + CAST(@ParentId AS VARCHAR(20));
END
ELSE
BEGIN
    UPDATE dbo.Menus
    SET MainPage = 1,
        IsActive = 1,
        ParentId = 0,
        MenuLink = N'pages-index',
        LinkIsActive = 0,
        UpdatedDate = @Now,
        Position = CASE WHEN Position = 0 THEN 20 ELSE Position END
    WHERE Id = @ParentId;

    PRINT N'Updated parent menu Tema Ornekleri Id=' + CAST(@ParentId AS VARCHAR(20));
END;

/* ---- Children T1–T8: menu row + main image + gallery ---- */
DECLARE @Theme NVARCHAR(10), @Name NVARCHAR(100), @Pos INT, @Desc NVARCHAR(MAX);
DECLARE @MenuId INT, @FsId INT, @Have INT, @Slot INT, @FileName NVARCHAR(200), @FileUrl NVARCHAR(400);

DECLARE theme_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT Theme, Name, Position, Description FROM #Themes ORDER BY Position;

OPEN theme_cursor;
FETCH NEXT FROM theme_cursor INTO @Theme, @Name, @Pos, @Desc;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @MenuId =
    (
        SELECT TOP 1 Id
        FROM dbo.Menus
        WHERE Name = @Name AND Lang = @Lang
        ORDER BY Id
    );

    IF @MenuId IS NULL
    BEGIN
        INSERT INTO dbo.Menus
            (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
             Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId,
             ParentId, MainPage, MenuLink, Link, PageTheme, LinkIsActive)
        VALUES
            (@Name, @Now, @Now, 1, @Pos, @Lang,
             @Desc, 0, N'sayfa,tema,' + LOWER(@Theme), NULL, @AdminUserId, @SeedMarker,
             @ParentId, 0, N'pages-index', NULL, @Theme, 0);

        SET @MenuId = CAST(SCOPE_IDENTITY() AS INT);
        PRINT N'Created ' + @Name + N' Id=' + CAST(@MenuId AS VARCHAR(20));
    END
    ELSE
    BEGIN
        UPDATE dbo.Menus
        SET ParentId = @ParentId,
            PageTheme = @Theme,
            MenuLink = N'pages-index',
            LinkIsActive = 0,
            IsActive = 1,
            Position = @Pos,
            UpdatedDate = @Now,
            Description = CASE
                WHEN Description IS NULL OR LTRIM(RTRIM(Description)) = N'' THEN @Desc
                ELSE Description
            END
        WHERE Id = @MenuId;

        PRINT N'Updated ' + @Name + N' Id=' + CAST(@MenuId AS VARCHAR(20));
    END;

    /* Main image (MenuMainImage) */
    IF NOT EXISTS (
        SELECT 1 FROM dbo.Menus m
        WHERE m.Id = @MenuId AND m.MainImageId IS NOT NULL AND m.MainImageId > 0
    )
    BEGIN
        SET @FileName = N'menu-theme-' + LOWER(@Theme) + N'-main.jpg';
        SET @FileUrl  = N'/media/seed/images/' + @FileName;
        SET @FsId = (SELECT TOP 1 Id FROM dbo.FileStorages WHERE FileName = @FileName);

        IF @FsId IS NULL
        BEGIN
            INSERT INTO dbo.FileStorages
                (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
                 FileName, FileUrl, MimeType, FileSize, Width, Height, Type, IsFileExist)
            VALUES
                (N'SEED ' + @Name + N' ana görsel', @Now, @Now, 1, 1, @Lang,
                 @FileName, @FileUrl, N'image/jpeg', 85000, 1200, 900, N'MenuMainImage', 0);

            SET @FsId = CAST(SCOPE_IDENTITY() AS INT);
        END
        ELSE
        BEGIN
            UPDATE dbo.FileStorages
            SET Type = N'MenuMainImage', FileUrl = @FileUrl, UpdatedDate = @Now
            WHERE Id = @FsId;
        END;

        UPDATE dbo.Menus
        SET MainImageId = @FsId, ImageState = 1, UpdatedDate = @Now
        WHERE Id = @MenuId;
    END
    ELSE
    BEGIN
        UPDATE dbo.Menus SET ImageState = 1 WHERE Id = @MenuId AND ImageState = 0;
    END;

    /* Gallery (MenuGallery via MenuFiles) — top up to @GalleryEach */
    SET @Have = (SELECT COUNT(*) FROM dbo.MenuFiles WHERE MenuId = @MenuId);
    SET @Slot = @Have + 1;

    WHILE @Slot <= @GalleryEach
    BEGIN
        SET @FileName = N'menu-theme-' + LOWER(@Theme) + N'-g' + RIGHT(N'00' + CAST(@Slot AS NVARCHAR(10)), 2) + N'.jpg';
        SET @FileUrl  = N'/media/seed/images/' + @FileName;
        SET @FsId = (SELECT TOP 1 Id FROM dbo.FileStorages WHERE FileName = @FileName);

        IF @FsId IS NULL
        BEGIN
            INSERT INTO dbo.FileStorages
                (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
                 FileName, FileUrl, MimeType, FileSize, Width, Height, Type, IsFileExist)
            VALUES
                (N'SEED ' + @Name + N' galeri ' + CAST(@Slot AS NVARCHAR(10)), @Now, @Now, 1, @Slot, @Lang,
                 @FileName, @FileUrl, N'image/jpeg', 85000, 1200, 900, N'MenuGallery', 0);

            SET @FsId = CAST(SCOPE_IDENTITY() AS INT);
        END
        ELSE
        BEGIN
            UPDATE dbo.FileStorages
            SET Type = N'MenuGallery', FileUrl = @FileUrl, UpdatedDate = @Now
            WHERE Id = @FsId;
        END;

        IF NOT EXISTS (SELECT 1 FROM dbo.MenuFiles WHERE MenuId = @MenuId AND FileStorageId = @FsId)
        BEGIN
            INSERT INTO dbo.MenuFiles
                (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, MenuId, FileStorageId)
            VALUES
                (N'SEED ' + @Name + N' galeri ' + CAST(@Slot AS NVARCHAR(10)),
                 @Now, @Now, 1, @Slot, @Lang, @MenuId, @FsId);
        END;

        SET @Slot = @Slot + 1;
    END;

    FETCH NEXT FROM theme_cursor INTO @Theme, @Name, @Pos, @Desc;
END;

CLOSE theme_cursor;
DEALLOCATE theme_cursor;

COMMIT TRANSACTION;

PRINT N'Theme pages upsert complete. ParentId=' + CAST(@ParentId AS VARCHAR(20));
PRINT N'Gallery target per page=' + CAST(@GalleryEach AS VARCHAR(10));

SELECT
    m.Id,
    m.Name,
    m.PageTheme,
    m.ParentId,
    m.MainPage,
    m.ImageState,
    m.MainImageId,
    (SELECT COUNT(*) FROM dbo.MenuFiles mf WHERE mf.MenuId = m.Id) AS GalleryCount
FROM dbo.Menus m
WHERE m.Id = @ParentId
   OR m.Name LIKE N'PT Dummy T%'
ORDER BY m.Position, m.Id;
