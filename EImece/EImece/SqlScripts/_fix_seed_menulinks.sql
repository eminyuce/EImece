UPDATE dbo.Menus SET MenuLink = N'home-index' WHERE Id = 6188;
UPDATE dbo.Menus SET MenuLink = N'pages-index' WHERE Id = 6189;
UPDATE dbo.Menus SET MenuLink = N'info-aboutus' WHERE Id = 6190;
UPDATE dbo.Menus SET MenuLink = N'pages-index' WHERE Id = 6191;
UPDATE dbo.Menus SET MenuLink = N'info-deliveryinfo' WHERE Id = 6192;
UPDATE dbo.Menus SET MenuLink = N'pages-index' WHERE Id = 6193;
UPDATE dbo.Menus SET MenuLink = N'pages-index' WHERE Id = 6194;
UPDATE dbo.Menus SET MenuLink = N'stories-index' WHERE Id = 6195;
UPDATE dbo.Menus SET MenuLink = N'info-privacypolicy' WHERE Id = 6196;
UPDATE dbo.Menus SET MenuLink = N'info-termsandconditions' WHERE Id = 6197;
UPDATE dbo.Menus SET MenuLink = N'pages-index' WHERE Id = 6198;
UPDATE dbo.Menus SET MenuLink = N'pages-index' WHERE Id = 6199;

SELECT Id, ParentId, Name, MenuLink, Link, LinkIsActive, Position
FROM dbo.Menus
WHERE Id BETWEEN 6188 AND 6199
ORDER BY Position;
