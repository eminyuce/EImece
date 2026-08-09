-- TOTP authenticator 2FA columns + secure temporary token table
-- Run against the application database before using EnableAuthenticator / AdminLogin 2FA.

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AspNetUsers') AND name = N'TwoFactorAuthenticatorEnabled')
BEGIN
    ALTER TABLE dbo.AspNetUsers
        ADD TwoFactorAuthenticatorEnabled BIT NOT NULL
            CONSTRAINT DF_AspNetUsers_TwoFactorAuthenticatorEnabled DEFAULT (0);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AspNetUsers') AND name = N'AuthenticatorKey')
BEGIN
    ALTER TABLE dbo.AspNetUsers
        ADD AuthenticatorKey NVARCHAR(128) NULL;
END
GO

IF OBJECT_ID(N'dbo.TwoFactorTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TwoFactorTokens
    (
        Id INT IDENTITY(1, 1) NOT NULL,
        UserId NVARCHAR(128) NOT NULL,
        Token NVARCHAR(128) NOT NULL,
        ExpiresUtc DATETIME2 NOT NULL,
        IsUsed BIT NOT NULL CONSTRAINT DF_TwoFactorTokens_IsUsed DEFAULT (0),
        CONSTRAINT PK_TwoFactorTokens PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_TwoFactorTokens_AspNetUsers FOREIGN KEY (UserId)
            REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_TwoFactorTokens_Token
        ON dbo.TwoFactorTokens (Token);

    CREATE NONCLUSTERED INDEX IX_TwoFactorTokens_UserId
        ON dbo.TwoFactorTokens (UserId);
END
GO
