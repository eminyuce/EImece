/*
================================================================================
  EImece — Comprehensive Mail Templates Upsert Script
================================================================================
  Idempotent script to insert / update all active and future email templates
  rendered via RazorEngine in EImece.
================================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @Now DATETIME = GETDATE();
DECLARE @Lang INT = 1;
DECLARE @AdminId NVARCHAR(128) = N'admin@eimece.test';


-- -----------------------------------------------------------------------------
-- Template: ConfirmYourAccount
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'ConfirmYourAccount')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'Hesabınızı Doğrulayın - @Model.companyname',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Hesap Doğrulama</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.companyname" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#1a73e8; font-weight:700;">@Model.companyname</h1>
                            }
                        </td>
                    </tr>
                    <!-- Main Body -->
                    <tr>
                        <td style="padding:35px 30px;">
                            <h2 style="margin:0 0 16px; font-size:20px; color:#202124; font-weight:600;">Hoş Geldiniz @Model.Name!</h2>
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                <strong>@Model.companyname</strong> ailesine katıldığınız için teşekkür ederiz. Hesabınızı güvenceye almak ve alışverişe başlamak için lütfen e-posta adresinizi onaylayın.
                            </p>
                            <div style="text-align:center; margin:30px 0;">
                                <a href="@Model.callbackUrl" style="background-color:#1a73e8; color:#ffffff; font-size:15px; font-weight:600; text-decoration:none; padding:12px 32px; border-radius:6px; display:inline-block; box-shadow:0 2px 4px rgba(26,115,232,0.3);">
                                    Hesabımı Doğrula
                                </a>
                            </div>
                            <p style="margin:0 0 10px; font-size:13px; line-height:1.5; color:#718096;">
                                Butona tıklayamıyorsanız aşağıdaki bağlantıyı tarayıcınızın adres çubuğuna yapıştırabilirsiniz:
                            </p>
                            <p style="margin:0 0 20px; font-size:12px; line-height:1.4; word-break:break-all;">
                                <a href="@Model.callbackUrl" style="color:#1a73e8; text-decoration:underline;">@Model.callbackUrl</a>
                            </p>
                            <div style="background-color:#f8fafc; border-left:4px solid #1a73e8; padding:12px 16px; border-radius:4px; margin-top:20px;">
                                <p style="margin:0; font-size:13px; color:#64748b;">
                                    Bu hesabı siz oluşturmadıysanız, bu e-postayı güvenle göz ardı edebilirsiniz.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            <p style="margin:0 0 6px;">© @DateTime.Now.Year @Model.companyname. Tüm hakları saklıdır.</p>
                            <p style="margin:0;">Bu e-posta otomatik olarak gönderilmiştir, lütfen yanıtlamayınız.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 1,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'ConfirmYourAccount';
    PRINT N'Updated MailTemplate: ConfirmYourAccount';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'ConfirmYourAccount', @Now, @Now, 1, 1, @Lang, N'Hesabınızı Doğrulayın - @Model.companyname', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Hesap Doğrulama</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.companyname" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#1a73e8; font-weight:700;">@Model.companyname</h1>
                            }
                        </td>
                    </tr>
                    <!-- Main Body -->
                    <tr>
                        <td style="padding:35px 30px;">
                            <h2 style="margin:0 0 16px; font-size:20px; color:#202124; font-weight:600;">Hoş Geldiniz @Model.Name!</h2>
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                <strong>@Model.companyname</strong> ailesine katıldığınız için teşekkür ederiz. Hesabınızı güvenceye almak ve alışverişe başlamak için lütfen e-posta adresinizi onaylayın.
                            </p>
                            <div style="text-align:center; margin:30px 0;">
                                <a href="@Model.callbackUrl" style="background-color:#1a73e8; color:#ffffff; font-size:15px; font-weight:600; text-decoration:none; padding:12px 32px; border-radius:6px; display:inline-block; box-shadow:0 2px 4px rgba(26,115,232,0.3);">
                                    Hesabımı Doğrula
                                </a>
                            </div>
                            <p style="margin:0 0 10px; font-size:13px; line-height:1.5; color:#718096;">
                                Butona tıklayamıyorsanız aşağıdaki bağlantıyı tarayıcınızın adres çubuğuna yapıştırabilirsiniz:
                            </p>
                            <p style="margin:0 0 20px; font-size:12px; line-height:1.4; word-break:break-all;">
                                <a href="@Model.callbackUrl" style="color:#1a73e8; text-decoration:underline;">@Model.callbackUrl</a>
                            </p>
                            <div style="background-color:#f8fafc; border-left:4px solid #1a73e8; padding:12px 16px; border-radius:4px; margin-top:20px;">
                                <p style="margin:0; font-size:13px; color:#64748b;">
                                    Bu hesabı siz oluşturmadıysanız, bu e-postayı güvenle göz ardı edebilirsiniz.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            <p style="margin:0 0 6px;">© @DateTime.Now.Year @Model.companyname. Tüm hakları saklıdır.</p>
                            <p style="margin:0;">Bu e-posta otomatik olarak gönderilmiştir, lütfen yanıtlamayınız.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: ConfirmYourAccount';
END;

-- -----------------------------------------------------------------------------
-- Template: ForgotPassword
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'ForgotPassword')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'Şifre Sıfırlama Talebi - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Şifre Sıfırlama</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#e53e3e; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <!-- Main Body -->
                    <tr>
                        <td style="padding:35px 30px;">
                            <h2 style="margin:0 0 16px; font-size:20px; color:#202124; font-weight:600;">Şifre Sıfırlama Talebi</h2>
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Merhaba <strong>@Model.Email</strong>,<br>
                                <strong>@Model.CompanyName</strong> hesabınız için şifre sıfırlama talebinde bulundunuz. Yeni şifrenizi belirlemek için aşağıdaki butona tıklayabilirsiniz.
                            </p>
                            <div style="text-align:center; margin:30px 0;">
                                <a href="@Model.ForgotPasswordLink" style="background-color:#e53e3e; color:#ffffff; font-size:15px; font-weight:600; text-decoration:none; padding:12px 32px; border-radius:6px; display:inline-block; box-shadow:0 2px 4px rgba(229,62,62,0.3);">
                                    Şifremi Sıfırla
                                </a>
                            </div>
                            <p style="margin:0 0 10px; font-size:13px; line-height:1.5; color:#718096;">
                                Butona tıklayamıyorsanız aşağıdaki bağlantıyı tarayıcınıza yapıştırın:
                            </p>
                            <p style="margin:0 0 20px; font-size:12px; line-height:1.4; word-break:break-all;">
                                <a href="@Model.ForgotPasswordLink" style="color:#e53e3e; text-decoration:underline;">@Model.ForgotPasswordLink</a>
                            </p>
                            <div style="background-color:#fff5f5; border-left:4px solid #e53e3e; padding:12px 16px; border-radius:4px; margin-top:20px;">
                                <p style="margin:0; font-size:13px; color:#c53030;">
                                    Bu talebi siz yapmadıysanız lütfen bu e-postayı dikkate almayınız. Mevcut şifreniz değişmeyecektir.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            <p style="margin:0 0 6px;">© @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.</p>
                            <p style="margin:0;">Bu e-posta otomatik olarak gönderilmiştir, lütfen yanıtlamayınız.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 2,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'ForgotPassword';
    PRINT N'Updated MailTemplate: ForgotPassword';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'ForgotPassword', @Now, @Now, 1, 2, @Lang, N'Şifre Sıfırlama Talebi - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Şifre Sıfırlama</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#e53e3e; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <!-- Main Body -->
                    <tr>
                        <td style="padding:35px 30px;">
                            <h2 style="margin:0 0 16px; font-size:20px; color:#202124; font-weight:600;">Şifre Sıfırlama Talebi</h2>
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Merhaba <strong>@Model.Email</strong>,<br>
                                <strong>@Model.CompanyName</strong> hesabınız için şifre sıfırlama talebinde bulundunuz. Yeni şifrenizi belirlemek için aşağıdaki butona tıklayabilirsiniz.
                            </p>
                            <div style="text-align:center; margin:30px 0;">
                                <a href="@Model.ForgotPasswordLink" style="background-color:#e53e3e; color:#ffffff; font-size:15px; font-weight:600; text-decoration:none; padding:12px 32px; border-radius:6px; display:inline-block; box-shadow:0 2px 4px rgba(229,62,62,0.3);">
                                    Şifremi Sıfırla
                                </a>
                            </div>
                            <p style="margin:0 0 10px; font-size:13px; line-height:1.5; color:#718096;">
                                Butona tıklayamıyorsanız aşağıdaki bağlantıyı tarayıcınıza yapıştırın:
                            </p>
                            <p style="margin:0 0 20px; font-size:12px; line-height:1.4; word-break:break-all;">
                                <a href="@Model.ForgotPasswordLink" style="color:#e53e3e; text-decoration:underline;">@Model.ForgotPasswordLink</a>
                            </p>
                            <div style="background-color:#fff5f5; border-left:4px solid #e53e3e; padding:12px 16px; border-radius:4px; margin-top:20px;">
                                <p style="margin:0; font-size:13px; color:#c53030;">
                                    Bu talebi siz yapmadıysanız lütfen bu e-postayı dikkate almayınız. Mevcut şifreniz değişmeyecektir.
                                </p>
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            <p style="margin:0 0 6px;">© @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.</p>
                            <p style="margin:0;">Bu e-posta otomatik olarak gönderilmiştir, lütfen yanıtlamayınız.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: ForgotPassword';
END;

-- -----------------------------------------------------------------------------
-- Template: OrderConfirmationEmail
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'OrderConfirmationEmail')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'Siparişiniz Alındı (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Sipariş Onayı</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.ImgLogoSrc)) {
                                <img src="@Model.ImgLogoSrc" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#2b8a3e; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <!-- Success Banner -->
                    <tr>
                        <td style="background-color:#ebfbee; padding:20px 30px; text-align:center; border-bottom:1px solid #d3f9d8;">
                            <h2 style="margin:0 0 6px; font-size:18px; color:#2b8a3e; font-weight:700;">Siparişiniz Başarıyla Alındı!</h2>
                            <p style="margin:0; font-size:14px; color:#40c057;">Sipariş Numaranız: <strong>#@Model.FinishedOrder.OrderNumber</strong></p>
                        </td>
                    </tr>
                    <!-- Main Body -->
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Sayın <strong>@Model.FinishedOrder.Customer.Name</strong>,<br>
                                Siparişiniz bize ulaştı ve en kısa sürede hazırlanıp kargoya verilecektir. Sipariş detaylarınızı aşağıda bulabilirsiniz:
                            </p>
                            
                            <!-- Products Table -->
                            <table width="100%" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse; margin:20px 0; font-size:14px;">
                                <thead>
                                    <tr style="background-color:#f8fafc; border-bottom:2px solid #e2e8f0;">
                                        <th align="left" style="padding:10px 12px; color:#475569; font-weight:600;">Ürün</th>
                                        <th align="center" style="padding:10px 12px; color:#475569; font-weight:600;">Adet</th>
                                        <th align="right" style="padding:10px 12px; color:#475569; font-weight:600;">Tutar</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @if (Model.FinishedOrder.OrderProducts != null) {
                                        foreach (var item in Model.FinishedOrder.OrderProducts) {
                                            <tr style="border-bottom:1px solid #edf2f7;">
                                                <td style="padding:12px; color:#1e293b;">@item.Name</td>
                                                <td align="center" style="padding:12px; color:#64748b;">@item.Count</td>
                                                <td align="right" style="padding:12px; font-weight:600; color:#1e293b;">@item.PriceStr</td>
                                            </tr>
                                        }
                                    }
                                </tbody>
                                <tfoot>
                                    <tr style="background-color:#f8fafc; border-top:2px solid #e2e8f0;">
                                        <td colspan="2" align="right" style="padding:12px; font-size:15px; font-weight:700; color:#1e293b;">Genel Toplam:</td>
                                        <td align="right" style="padding:12px; font-size:16px; font-weight:700; color:#2b8a3e;">@Model.FinishedOrder.OrderPriceWithDiscount</td>
                                    </tr>
                                </tfoot>
                            </table>

                            <!-- Delivery Details -->
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:16px; margin-top:20px;">
                                <h3 style="margin:0 0 10px; font-size:14px; font-weight:600; color:#334155;">Teslimat Bilgileri</h3>
                                <p style="margin:0; font-size:13px; line-height:1.5; color:#64748b;">
                                    <strong>Teslimat Adresi:</strong> @Model.FinishedOrder.OrderAddress<br>
                                    <strong>Sipariş Tarihi:</strong> @Model.FinishedOrder.CreatedDate.ToString("dd.MM.yyyy HH:mm")
                                </p>
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            <p style="margin:0 0 6px;">Sorularınız için: <a href="mailto:@Model.CompanyEmailAddress" style="color:#2b8a3e;">@Model.CompanyEmailAddress</a> | @Model.CompanyPhoneNumber</p>
                            <p style="margin:0;">© @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 3,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'OrderConfirmationEmail';
    PRINT N'Updated MailTemplate: OrderConfirmationEmail';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'OrderConfirmationEmail', @Now, @Now, 1, 3, @Lang, N'Siparişiniz Alındı (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Sipariş Onayı</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.ImgLogoSrc)) {
                                <img src="@Model.ImgLogoSrc" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#2b8a3e; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <!-- Success Banner -->
                    <tr>
                        <td style="background-color:#ebfbee; padding:20px 30px; text-align:center; border-bottom:1px solid #d3f9d8;">
                            <h2 style="margin:0 0 6px; font-size:18px; color:#2b8a3e; font-weight:700;">Siparişiniz Başarıyla Alındı!</h2>
                            <p style="margin:0; font-size:14px; color:#40c057;">Sipariş Numaranız: <strong>#@Model.FinishedOrder.OrderNumber</strong></p>
                        </td>
                    </tr>
                    <!-- Main Body -->
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Sayın <strong>@Model.FinishedOrder.Customer.Name</strong>,<br>
                                Siparişiniz bize ulaştı ve en kısa sürede hazırlanıp kargoya verilecektir. Sipariş detaylarınızı aşağıda bulabilirsiniz:
                            </p>
                            
                            <!-- Products Table -->
                            <table width="100%" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse; margin:20px 0; font-size:14px;">
                                <thead>
                                    <tr style="background-color:#f8fafc; border-bottom:2px solid #e2e8f0;">
                                        <th align="left" style="padding:10px 12px; color:#475569; font-weight:600;">Ürün</th>
                                        <th align="center" style="padding:10px 12px; color:#475569; font-weight:600;">Adet</th>
                                        <th align="right" style="padding:10px 12px; color:#475569; font-weight:600;">Tutar</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @if (Model.FinishedOrder.OrderProducts != null) {
                                        foreach (var item in Model.FinishedOrder.OrderProducts) {
                                            <tr style="border-bottom:1px solid #edf2f7;">
                                                <td style="padding:12px; color:#1e293b;">@item.Name</td>
                                                <td align="center" style="padding:12px; color:#64748b;">@item.Count</td>
                                                <td align="right" style="padding:12px; font-weight:600; color:#1e293b;">@item.PriceStr</td>
                                            </tr>
                                        }
                                    }
                                </tbody>
                                <tfoot>
                                    <tr style="background-color:#f8fafc; border-top:2px solid #e2e8f0;">
                                        <td colspan="2" align="right" style="padding:12px; font-size:15px; font-weight:700; color:#1e293b;">Genel Toplam:</td>
                                        <td align="right" style="padding:12px; font-size:16px; font-weight:700; color:#2b8a3e;">@Model.FinishedOrder.OrderPriceWithDiscount</td>
                                    </tr>
                                </tfoot>
                            </table>

                            <!-- Delivery Details -->
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:16px; margin-top:20px;">
                                <h3 style="margin:0 0 10px; font-size:14px; font-weight:600; color:#334155;">Teslimat Bilgileri</h3>
                                <p style="margin:0; font-size:13px; line-height:1.5; color:#64748b;">
                                    <strong>Teslimat Adresi:</strong> @Model.FinishedOrder.OrderAddress<br>
                                    <strong>Sipariş Tarihi:</strong> @Model.FinishedOrder.CreatedDate.ToString("dd.MM.yyyy HH:mm")
                                </p>
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            <p style="margin:0 0 6px;">Sorularınız için: <a href="mailto:@Model.CompanyEmailAddress" style="color:#2b8a3e;">@Model.CompanyEmailAddress</a> | @Model.CompanyPhoneNumber</p>
                            <p style="margin:0;">© @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: OrderConfirmationEmail';
END;

-- -----------------------------------------------------------------------------
-- Template: CompanyGotNewOrderEmail
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'CompanyGotNewOrderEmail')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'🔔 Yeni Sipariş Alındı (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Yeni Sipariş Bildirimi</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td style="background-color:#1e293b; padding:20px 30px; color:#ffffff;">
                            <h1 style="margin:0; font-size:20px; font-weight:700;">🔔 Yeni Sipariş Bildirimi</h1>
                            <p style="margin:4px 0 0; font-size:13px; color:#94a3b8;">Sipariş No: #@Model.FinishedOrder.OrderNumber</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style="padding:30px;">
                            <div style="background-color:#f1f5f9; border-radius:6px; padding:16px; margin-bottom:20px;">
                                <h3 style="margin:0 0 10px; font-size:14px; color:#334155; font-weight:600;">Müşteri Bilgileri</h3>
                                <p style="margin:0; font-size:13px; line-height:1.6; color:#475569;">
                                    <strong>İsim:</strong> @Model.FinishedOrder.Customer.Name<br>
                                    <strong>E-posta:</strong> @Model.FinishedOrder.Customer.Email<br>
                                    <strong>Telefon:</strong> @Model.FinishedOrder.Customer.PhoneNumber<br>
                                    <strong>Adres:</strong> @Model.FinishedOrder.OrderAddress
                                </p>
                            </div>

                            <h3 style="margin:0 0 12px; font-size:15px; color:#1e293b;">Sipariş Edilen Ürünler</h3>
                            <table width="100%" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse; margin-bottom:20px; font-size:13px;">
                                <thead>
                                    <tr style="background-color:#f8fafc; border-bottom:2px solid #e2e8f0;">
                                        <th align="left" style="padding:8px 10px; color:#475569;">Ürün</th>
                                        <th align="center" style="padding:8px 10px; color:#475569;">Adet</th>
                                        <th align="right" style="padding:8px 10px; color:#475569;">Fiyat</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @if (Model.FinishedOrder.OrderProducts != null) {
                                        foreach (var p in Model.FinishedOrder.OrderProducts) {
                                            <tr style="border-bottom:1px solid #f1f5f9;">
                                                <td style="padding:10px; color:#334155;">@p.Name</td>
                                                <td align="center" style="padding:10px; color:#64748b;">@p.Count</td>
                                                <td align="right" style="padding:10px; font-weight:600; color:#1e293b;">@p.PriceStr</td>
                                            </tr>
                                        }
                                    }
                                </tbody>
                                <tfoot>
                                    <tr style="border-top:2px solid #e2e8f0; background-color:#f8fafc;">
                                        <td colspan="2" align="right" style="padding:10px; font-weight:700; color:#1e293b;">Toplam Tutar:</td>
                                        <td align="right" style="padding:10px; font-weight:700; font-size:15px; color:#0f766e;">@Model.FinishedOrder.OrderPriceWithDiscount</td>
                                    </tr>
                                </tfoot>
                            </table>

                            @if (!string.IsNullOrEmpty(Model.AdminPanelUrl)) {
                                <div style="text-align:center; margin-top:25px;">
                                    <a href="@Model.AdminPanelUrl" style="background-color:#0f766e; color:#ffffff; text-decoration:none; padding:12px 28px; border-radius:6px; font-size:14px; font-weight:600; display:inline-block;">
                                        Yönetim Panelinde Görüntüle →
                                    </a>
                                </div>
                            }
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:16px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName — Yönetim Bildirim Sistemi
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 4,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'CompanyGotNewOrderEmail';
    PRINT N'Updated MailTemplate: CompanyGotNewOrderEmail';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'CompanyGotNewOrderEmail', @Now, @Now, 1, 4, @Lang, N'🔔 Yeni Sipariş Alındı (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Yeni Sipariş Bildirimi</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td style="background-color:#1e293b; padding:20px 30px; color:#ffffff;">
                            <h1 style="margin:0; font-size:20px; font-weight:700;">🔔 Yeni Sipariş Bildirimi</h1>
                            <p style="margin:4px 0 0; font-size:13px; color:#94a3b8;">Sipariş No: #@Model.FinishedOrder.OrderNumber</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style="padding:30px;">
                            <div style="background-color:#f1f5f9; border-radius:6px; padding:16px; margin-bottom:20px;">
                                <h3 style="margin:0 0 10px; font-size:14px; color:#334155; font-weight:600;">Müşteri Bilgileri</h3>
                                <p style="margin:0; font-size:13px; line-height:1.6; color:#475569;">
                                    <strong>İsim:</strong> @Model.FinishedOrder.Customer.Name<br>
                                    <strong>E-posta:</strong> @Model.FinishedOrder.Customer.Email<br>
                                    <strong>Telefon:</strong> @Model.FinishedOrder.Customer.PhoneNumber<br>
                                    <strong>Adres:</strong> @Model.FinishedOrder.OrderAddress
                                </p>
                            </div>

                            <h3 style="margin:0 0 12px; font-size:15px; color:#1e293b;">Sipariş Edilen Ürünler</h3>
                            <table width="100%" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse; margin-bottom:20px; font-size:13px;">
                                <thead>
                                    <tr style="background-color:#f8fafc; border-bottom:2px solid #e2e8f0;">
                                        <th align="left" style="padding:8px 10px; color:#475569;">Ürün</th>
                                        <th align="center" style="padding:8px 10px; color:#475569;">Adet</th>
                                        <th align="right" style="padding:8px 10px; color:#475569;">Fiyat</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @if (Model.FinishedOrder.OrderProducts != null) {
                                        foreach (var p in Model.FinishedOrder.OrderProducts) {
                                            <tr style="border-bottom:1px solid #f1f5f9;">
                                                <td style="padding:10px; color:#334155;">@p.Name</td>
                                                <td align="center" style="padding:10px; color:#64748b;">@p.Count</td>
                                                <td align="right" style="padding:10px; font-weight:600; color:#1e293b;">@p.PriceStr</td>
                                            </tr>
                                        }
                                    }
                                </tbody>
                                <tfoot>
                                    <tr style="border-top:2px solid #e2e8f0; background-color:#f8fafc;">
                                        <td colspan="2" align="right" style="padding:10px; font-weight:700; color:#1e293b;">Toplam Tutar:</td>
                                        <td align="right" style="padding:10px; font-weight:700; font-size:15px; color:#0f766e;">@Model.FinishedOrder.OrderPriceWithDiscount</td>
                                    </tr>
                                </tfoot>
                            </table>

                            @if (!string.IsNullOrEmpty(Model.AdminPanelUrl)) {
                                <div style="text-align:center; margin-top:25px;">
                                    <a href="@Model.AdminPanelUrl" style="background-color:#0f766e; color:#ffffff; text-decoration:none; padding:12px 28px; border-radius:6px; font-size:14px; font-weight:600; display:inline-block;">
                                        Yönetim Panelinde Görüntüle →
                                    </a>
                                </div>
                            }
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:16px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName — Yönetim Bildirim Sistemi
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: CompanyGotNewOrderEmail';
END;

-- -----------------------------------------------------------------------------
-- Template: ContactUsAboutProductInfo
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'ContactUsAboutProductInfo')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'Ürün Bilgi Talebi: @Model.ContactUs.Name - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Ürün Bilgi Talebi</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td style="background-color:#3b82f6; padding:20px 30px; color:#ffffff;">
                            <h2 style="margin:0; font-size:18px;">📦 Ürün Bilgi Talebi</h2>
                            <p style="margin:4px 0 0; font-size:13px; color:#dbeafe;">Ürün ID: #@Model.ContactUs.ItemId</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style="padding:30px;">
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:16px; margin-bottom:20px;">
                                <p style="margin:0 0 8px; font-size:14px;"><strong>Gönderen:</strong> @Model.ContactUs.Name</p>
                                <p style="margin:0 0 8px; font-size:14px;"><strong>E-posta:</strong> <a href="mailto:@Model.ContactUs.Email" style="color:#3b82f6;">@Model.ContactUs.Email</a></p>
                                <p style="margin:0; font-size:14px;"><strong>Telefon:</strong> @Model.ContactUs.PhoneNumber</p>
                            </div>
                            <h3 style="margin:0 0 10px; font-size:14px; color:#475569;">Müşteri Mesajı:</h3>
                            <div style="background-color:#f1f5f9; border-left:4px solid #3b82f6; padding:16px; border-radius:4px; font-size:14px; line-height:1.6; color:#1e293b;">
                                @Model.ContactUs.Description
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:16px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 5,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'ContactUsAboutProductInfo';
    PRINT N'Updated MailTemplate: ContactUsAboutProductInfo';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'ContactUsAboutProductInfo', @Now, @Now, 1, 5, @Lang, N'Ürün Bilgi Talebi: @Model.ContactUs.Name - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Ürün Bilgi Talebi</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td style="background-color:#3b82f6; padding:20px 30px; color:#ffffff;">
                            <h2 style="margin:0; font-size:18px;">📦 Ürün Bilgi Talebi</h2>
                            <p style="margin:4px 0 0; font-size:13px; color:#dbeafe;">Ürün ID: #@Model.ContactUs.ItemId</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style="padding:30px;">
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:16px; margin-bottom:20px;">
                                <p style="margin:0 0 8px; font-size:14px;"><strong>Gönderen:</strong> @Model.ContactUs.Name</p>
                                <p style="margin:0 0 8px; font-size:14px;"><strong>E-posta:</strong> <a href="mailto:@Model.ContactUs.Email" style="color:#3b82f6;">@Model.ContactUs.Email</a></p>
                                <p style="margin:0; font-size:14px;"><strong>Telefon:</strong> @Model.ContactUs.PhoneNumber</p>
                            </div>
                            <h3 style="margin:0 0 10px; font-size:14px; color:#475569;">Müşteri Mesajı:</h3>
                            <div style="background-color:#f1f5f9; border-left:4px solid #3b82f6; padding:16px; border-radius:4px; font-size:14px; line-height:1.6; color:#1e293b;">
                                @Model.ContactUs.Description
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:16px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: ContactUsAboutProductInfo';
END;

-- -----------------------------------------------------------------------------
-- Template: ContactUsForCommunication
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'ContactUsForCommunication')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'İletişim Formu Başvurusu: @Model.ContactUs.Name - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>İletişim Formu Başvurusu</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td style="background-color:#0284c7; padding:20px 30px; color:#ffffff;">
                            <h2 style="margin:0; font-size:18px;">✉️ Yeni İletişim Formu Mesajı</h2>
                            <p style="margin:4px 0 0; font-size:13px; color:#e0f2fe;">@Model.CompanyName web sitesinden gönderildi</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style="padding:30px;">
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:16px; margin-bottom:20px;">
                                <p style="margin:0 0 8px; font-size:14px;"><strong>Gönderen:</strong> @Model.ContactUs.Name</p>
                                <p style="margin:0 0 8px; font-size:14px;"><strong>E-posta:</strong> <a href="mailto:@Model.ContactUs.Email" style="color:#0284c7;">@Model.ContactUs.Email</a></p>
                                <p style="margin:0; font-size:14px;"><strong>Telefon:</strong> @Model.ContactUs.PhoneNumber</p>
                            </div>
                            <h3 style="margin:0 0 10px; font-size:14px; color:#475569;">Mesaj İçeriği:</h3>
                            <div style="background-color:#f1f5f9; border-left:4px solid #0284c7; padding:16px; border-radius:4px; font-size:14px; line-height:1.6; color:#1e293b;">
                                @Model.ContactUs.Description
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:16px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 6,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'ContactUsForCommunication';
    PRINT N'Updated MailTemplate: ContactUsForCommunication';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'ContactUsForCommunication', @Now, @Now, 1, 6, @Lang, N'İletişim Formu Başvurusu: @Model.ContactUs.Name - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>İletişim Formu Başvurusu</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td style="background-color:#0284c7; padding:20px 30px; color:#ffffff;">
                            <h2 style="margin:0; font-size:18px;">✉️ Yeni İletişim Formu Mesajı</h2>
                            <p style="margin:4px 0 0; font-size:13px; color:#e0f2fe;">@Model.CompanyName web sitesinden gönderildi</p>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style="padding:30px;">
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:16px; margin-bottom:20px;">
                                <p style="margin:0 0 8px; font-size:14px;"><strong>Gönderen:</strong> @Model.ContactUs.Name</p>
                                <p style="margin:0 0 8px; font-size:14px;"><strong>E-posta:</strong> <a href="mailto:@Model.ContactUs.Email" style="color:#0284c7;">@Model.ContactUs.Email</a></p>
                                <p style="margin:0; font-size:14px;"><strong>Telefon:</strong> @Model.ContactUs.PhoneNumber</p>
                            </div>
                            <h3 style="margin:0 0 10px; font-size:14px; color:#475569;">Mesaj İçeriği:</h3>
                            <div style="background-color:#f1f5f9; border-left:4px solid #0284c7; padding:16px; border-radius:4px; font-size:14px; line-height:1.6; color:#1e293b;">
                                @Model.ContactUs.Description
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:16px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: ContactUsForCommunication';
END;

-- -----------------------------------------------------------------------------
-- Template: SendMessageToSeller
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'SendMessageToSeller')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'Satıcıya Mesaj Geldi: @Model.Name (#@Model.ItemId)',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Satıcıya Mesaj</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td style="background-color:#6366f1; padding:20px 30px; color:#ffffff;">
                            <h2 style="margin:0; font-size:18px;">💬 Satıcıya Yeni Mesaj</h2>
                            <p style="margin:4px 0 0; font-size:13px; color:#e0e7ff;">Ürün / İlan ID: #@Model.ItemId</p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:16px; margin-bottom:20px;">
                                <p style="margin:0 0 8px; font-size:14px;"><strong>Müşteri:</strong> @Model.Name</p>
                                <p style="margin:0 0 8px; font-size:14px;"><strong>E-posta:</strong> <a href="mailto:@Model.Email" style="color:#6366f1;">@Model.Email</a></p>
                                <p style="margin:0; font-size:14px;"><strong>Telefon:</strong> @Model.PhoneNumber</p>
                            </div>
                            <h3 style="margin:0 0 10px; font-size:14px; color:#475569;">Mesaj:</h3>
                            <div style="background-color:#f1f5f9; border-left:4px solid #6366f1; padding:16px; border-radius:4px; font-size:14px; line-height:1.6; color:#1e293b;">
                                @Model.Description
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:16px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year EImece Pazaryeri
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 7,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'SendMessageToSeller';
    PRINT N'Updated MailTemplate: SendMessageToSeller';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'SendMessageToSeller', @Now, @Now, 1, 7, @Lang, N'Satıcıya Mesaj Geldi: @Model.Name (#@Model.ItemId)', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Satıcıya Mesaj</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td style="background-color:#6366f1; padding:20px 30px; color:#ffffff;">
                            <h2 style="margin:0; font-size:18px;">💬 Satıcıya Yeni Mesaj</h2>
                            <p style="margin:4px 0 0; font-size:13px; color:#e0e7ff;">Ürün / İlan ID: #@Model.ItemId</p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:16px; margin-bottom:20px;">
                                <p style="margin:0 0 8px; font-size:14px;"><strong>Müşteri:</strong> @Model.Name</p>
                                <p style="margin:0 0 8px; font-size:14px;"><strong>E-posta:</strong> <a href="mailto:@Model.Email" style="color:#6366f1;">@Model.Email</a></p>
                                <p style="margin:0; font-size:14px;"><strong>Telefon:</strong> @Model.PhoneNumber</p>
                            </div>
                            <h3 style="margin:0 0 10px; font-size:14px; color:#475569;">Mesaj:</h3>
                            <div style="background-color:#f1f5f9; border-left:4px solid #6366f1; padding:16px; border-radius:4px; font-size:14px; line-height:1.6; color:#1e293b;">
                                @Model.Description
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:16px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year EImece Pazaryeri
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: SendMessageToSeller';
END;

-- -----------------------------------------------------------------------------
-- Template: OrderShipped
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'OrderShipped')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'📦 Siparişiniz Kargoya Verildi (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Siparişiniz Kargoya Verildi</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#2563eb; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <!-- Banner -->
                    <tr>
                        <td style="background-color:#eff6ff; padding:20px 30px; text-align:center; border-bottom:1px solid #dbeafe;">
                            <h2 style="margin:0 0 6px; font-size:18px; color:#1d4ed8; font-weight:700;">📦 Siparişiniz Yola Çıktı!</h2>
                            <p style="margin:0; font-size:14px; color:#3b82f6;">Sipariş No: <strong>#@Model.FinishedOrder.OrderNumber</strong></p>
                        </td>
                    </tr>
                    <!-- Body -->
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Sayın <strong>@Model.FinishedOrder.Customer.Name</strong>,<br>
                                <strong>#@Model.FinishedOrder.OrderNumber</strong> numaralı siparişiniz özenle paketlendi ve kargo firmasına teslim edildi.
                            </p>
                            
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:20px; margin:20px 0;">
                                <h3 style="margin:0 0 12px; font-size:15px; color:#1e293b; font-weight:600;">Kargo Takip Bilgileri</h3>
                                <p style="margin:0 0 8px; font-size:14px; color:#475569;"><strong>Kargo Firması:</strong> @Model.FinishedOrder.CargoCompany</p>
                                <p style="margin:0 0 16px; font-size:14px; color:#475569;"><strong>Takip Numarası:</strong> <span style="font-family:monospace; font-weight:bold; font-size:15px; color:#1d4ed8;">@Model.FinishedOrder.CargoTrackNumber</span></p>
                                
                                @if (!string.IsNullOrEmpty(Model.CargoTrackingUrl)) {
                                    <div style="text-align:center; margin-top:15px;">
                                        <a href="@Model.CargoTrackingUrl" style="background-color:#2563eb; color:#ffffff; text-decoration:none; padding:10px 24px; border-radius:6px; font-size:14px; font-weight:600; display:inline-block;">
                                            Kargomu Takip Et →
                                        </a>
                                    </div>
                                }
                            </div>

                            <div style="background-color:#f8fafc; border-radius:6px; padding:14px; font-size:13px; color:#64748b;">
                                <strong>Teslimat Adresi:</strong> @Model.FinishedOrder.OrderAddress
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            <p style="margin:0 0 6px;">Bizi tercih ettiğiniz için teşekkür ederiz.</p>
                            <p style="margin:0;">© @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 8,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'OrderShipped';
    PRINT N'Updated MailTemplate: OrderShipped';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'OrderShipped', @Now, @Now, 1, 8, @Lang, N'📦 Siparişiniz Kargoya Verildi (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Siparişiniz Kargoya Verildi</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <!-- Header -->
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#2563eb; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <!-- Banner -->
                    <tr>
                        <td style="background-color:#eff6ff; padding:20px 30px; text-align:center; border-bottom:1px solid #dbeafe;">
                            <h2 style="margin:0 0 6px; font-size:18px; color:#1d4ed8; font-weight:700;">📦 Siparişiniz Yola Çıktı!</h2>
                            <p style="margin:0; font-size:14px; color:#3b82f6;">Sipariş No: <strong>#@Model.FinishedOrder.OrderNumber</strong></p>
                        </td>
                    </tr>
                    <!-- Body -->
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Sayın <strong>@Model.FinishedOrder.Customer.Name</strong>,<br>
                                <strong>#@Model.FinishedOrder.OrderNumber</strong> numaralı siparişiniz özenle paketlendi ve kargo firmasına teslim edildi.
                            </p>
                            
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:20px; margin:20px 0;">
                                <h3 style="margin:0 0 12px; font-size:15px; color:#1e293b; font-weight:600;">Kargo Takip Bilgileri</h3>
                                <p style="margin:0 0 8px; font-size:14px; color:#475569;"><strong>Kargo Firması:</strong> @Model.FinishedOrder.CargoCompany</p>
                                <p style="margin:0 0 16px; font-size:14px; color:#475569;"><strong>Takip Numarası:</strong> <span style="font-family:monospace; font-weight:bold; font-size:15px; color:#1d4ed8;">@Model.FinishedOrder.CargoTrackNumber</span></p>
                                
                                @if (!string.IsNullOrEmpty(Model.CargoTrackingUrl)) {
                                    <div style="text-align:center; margin-top:15px;">
                                        <a href="@Model.CargoTrackingUrl" style="background-color:#2563eb; color:#ffffff; text-decoration:none; padding:10px 24px; border-radius:6px; font-size:14px; font-weight:600; display:inline-block;">
                                            Kargomu Takip Et →
                                        </a>
                                    </div>
                                }
                            </div>

                            <div style="background-color:#f8fafc; border-radius:6px; padding:14px; font-size:13px; color:#64748b;">
                                <strong>Teslimat Adresi:</strong> @Model.FinishedOrder.OrderAddress
                            </div>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            <p style="margin:0 0 6px;">Bizi tercih ettiğiniz için teşekkür ederiz.</p>
                            <p style="margin:0;">© @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: OrderShipped';
END;

-- -----------------------------------------------------------------------------
-- Template: OrderDelivered
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'OrderDelivered')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'✅ Siparişiniz Teslim Edildi (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Siparişiniz Teslim Edildi</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#16a34a; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f0fdf4; padding:20px 30px; text-align:center; border-bottom:1px solid #dcfce7;">
                            <h2 style="margin:0 0 6px; font-size:18px; color:#16a34a; font-weight:700;">🎉 Siparişiniz Teslim Edildi!</h2>
                            <p style="margin:0; font-size:14px; color:#22c55e;">Sipariş No: <strong>#@Model.FinishedOrder.OrderNumber</strong></p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Sayın <strong>@Model.FinishedOrder.Customer.Name</strong>,<br>
                                Siparişinizin başarıyla teslim edildiğini bildirmekten mutluluk duyarız. Ürünlerinizi güzel günlerde kullanmanızı dileriz!
                            </p>
                            
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:20px; text-align:center; margin:24px 0;">
                                <h3 style="margin:0 0 10px; font-size:16px; color:#1e293b;">Deneyiminizi Paylaşır mısınız?</h3>
                                <p style="margin:0 0 16px; font-size:13px; color:#64748b;">Görüşleriniz hizmet kalitemizi artırmamıza yardımcı olur.</p>
                                <a href="@Model.ReviewUrl" style="background-color:#16a34a; color:#ffffff; text-decoration:none; padding:12px 28px; border-radius:6px; font-size:14px; font-weight:600; display:inline-block;">
                                    Ürünleri Değerlendir ⭐⭐⭐⭐⭐
                                </a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 9,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'OrderDelivered';
    PRINT N'Updated MailTemplate: OrderDelivered';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'OrderDelivered', @Now, @Now, 1, 9, @Lang, N'✅ Siparişiniz Teslim Edildi (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Siparişiniz Teslim Edildi</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#16a34a; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f0fdf4; padding:20px 30px; text-align:center; border-bottom:1px solid #dcfce7;">
                            <h2 style="margin:0 0 6px; font-size:18px; color:#16a34a; font-weight:700;">🎉 Siparişiniz Teslim Edildi!</h2>
                            <p style="margin:0; font-size:14px; color:#22c55e;">Sipariş No: <strong>#@Model.FinishedOrder.OrderNumber</strong></p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Sayın <strong>@Model.FinishedOrder.Customer.Name</strong>,<br>
                                Siparişinizin başarıyla teslim edildiğini bildirmekten mutluluk duyarız. Ürünlerinizi güzel günlerde kullanmanızı dileriz!
                            </p>
                            
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:20px; text-align:center; margin:24px 0;">
                                <h3 style="margin:0 0 10px; font-size:16px; color:#1e293b;">Deneyiminizi Paylaşır mısınız?</h3>
                                <p style="margin:0 0 16px; font-size:13px; color:#64748b;">Görüşleriniz hizmet kalitemizi artırmamıza yardımcı olur.</p>
                                <a href="@Model.ReviewUrl" style="background-color:#16a34a; color:#ffffff; text-decoration:none; padding:12px 28px; border-radius:6px; font-size:14px; font-weight:600; display:inline-block;">
                                    Ürünleri Değerlendir ⭐⭐⭐⭐⭐
                                </a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: OrderDelivered';
END;

-- -----------------------------------------------------------------------------
-- Template: OrderCancelled
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'OrderCancelled')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'Sipariş İptal Bildirimi (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Sipariş İptali</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#dc2626; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#fef2f2; padding:20px 30px; text-align:center; border-bottom:1px solid #fee2e2;">
                            <h2 style="margin:0 0 6px; font-size:18px; color:#dc2626; font-weight:700;">Siparişiniz İptal Edildi</h2>
                            <p style="margin:0; font-size:14px; color:#ef4444;">Sipariş No: <strong>#@Model.FinishedOrder.OrderNumber</strong></p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Sayın <strong>@Model.FinishedOrder.Customer.Name</strong>,<br>
                                <strong>#@Model.FinishedOrder.OrderNumber</strong> numaralı siparişiniz talebiniz veya stok durumu doğrultusunda iptal edilmiştir.
                            </p>
                            
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:16px; margin:20px 0;">
                                <p style="margin:0 0 8px; font-size:14px; color:#475569;"><strong>İptal Nedeni:</strong> @Model.CancellationReason</p>
                                <p style="margin:0; font-size:14px; color:#475569;"><strong>İade Tutarı:</strong> <strong style="color:#dc2626;">@Model.RefundAmount</strong></p>
                            </div>

                            <p style="margin:0; font-size:13px; line-height:1.5; color:#64748b;">
                                Ödemeniz, bankanızın işlem süreçlerine bağlı olarak 1-7 iş günü içerisinde kartınıza veya hesabınıza yansıyacaktır.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 10,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'OrderCancelled';
    PRINT N'Updated MailTemplate: OrderCancelled';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'OrderCancelled', @Now, @Now, 1, 10, @Lang, N'Sipariş İptal Bildirimi (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Sipariş İptali</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#dc2626; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#fef2f2; padding:20px 30px; text-align:center; border-bottom:1px solid #fee2e2;">
                            <h2 style="margin:0 0 6px; font-size:18px; color:#dc2626; font-weight:700;">Siparişiniz İptal Edildi</h2>
                            <p style="margin:0; font-size:14px; color:#ef4444;">Sipariş No: <strong>#@Model.FinishedOrder.OrderNumber</strong></p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Sayın <strong>@Model.FinishedOrder.Customer.Name</strong>,<br>
                                <strong>#@Model.FinishedOrder.OrderNumber</strong> numaralı siparişiniz talebiniz veya stok durumu doğrultusunda iptal edilmiştir.
                            </p>
                            
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:6px; padding:16px; margin:20px 0;">
                                <p style="margin:0 0 8px; font-size:14px; color:#475569;"><strong>İptal Nedeni:</strong> @Model.CancellationReason</p>
                                <p style="margin:0; font-size:14px; color:#475569;"><strong>İade Tutarı:</strong> <strong style="color:#dc2626;">@Model.RefundAmount</strong></p>
                            </div>

                            <p style="margin:0; font-size:13px; line-height:1.5; color:#64748b;">
                                Ödemeniz, bankanızın işlem süreçlerine bağlı olarak 1-7 iş günü içerisinde kartınıza veya hesabınıza yansıyacaktır.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: OrderCancelled';
END;

-- -----------------------------------------------------------------------------
-- Template: ReturnApproved
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'ReturnApproved')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'İade Talebiniz Onaylandı (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>İade Talebi Onayı</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#8b5cf6; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f5f3ff; padding:20px 30px; text-align:center; border-bottom:1px solid #ede9fe;">
                            <h2 style="margin:0 0 6px; font-size:18px; color:#7c3aed; font-weight:700;">İade Talebiniz Onaylandı</h2>
                            <p style="margin:0; font-size:14px; color:#8b5cf6;">Sipariş No: <strong>#@Model.FinishedOrder.OrderNumber</strong></p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Sayın <strong>@Model.FinishedOrder.Customer.Name</strong>,<br>
                                İade talebiniz incelenmiş ve onaylanmıştır. Ürünü tarafımıza ücretsiz göndermek için aşağıdaki kargo kodunu kullanabilirsiniz:
                            </p>
                            
                            <div style="background-color:#f8fafc; border:2px dashed #7c3aed; border-radius:8px; padding:20px; text-align:center; margin:20px 0;">
                                <p style="margin:0 0 6px; font-size:13px; color:#64748b;">Ücretsiz İade Kargo Kodu</p>
                                <p style="margin:0; font-size:22px; font-weight:700; color:#7c3aed; font-family:monospace; letter-spacing:2px;">@Model.ReturnCargoCode</p>
                            </div>

                            <p style="margin:0; font-size:13px; line-height:1.5; color:#64748b;">
                                Ürün bize ulaşıp kontroller tamamlandıktan sonra <strong>@Model.RefundAmount</strong> tutarındaki iadeniz hesabınıza aktarılacaktır.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 11,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'ReturnApproved';
    PRINT N'Updated MailTemplate: ReturnApproved';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'ReturnApproved', @Now, @Now, 1, 11, @Lang, N'İade Talebiniz Onaylandı (#@Model.FinishedOrder.OrderNumber) - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>İade Talebi Onayı</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#8b5cf6; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f5f3ff; padding:20px 30px; text-align:center; border-bottom:1px solid #ede9fe;">
                            <h2 style="margin:0 0 6px; font-size:18px; color:#7c3aed; font-weight:700;">İade Talebiniz Onaylandı</h2>
                            <p style="margin:0; font-size:14px; color:#8b5cf6;">Sipariş No: <strong>#@Model.FinishedOrder.OrderNumber</strong></p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                Sayın <strong>@Model.FinishedOrder.Customer.Name</strong>,<br>
                                İade talebiniz incelenmiş ve onaylanmıştır. Ürünü tarafımıza ücretsiz göndermek için aşağıdaki kargo kodunu kullanabilirsiniz:
                            </p>
                            
                            <div style="background-color:#f8fafc; border:2px dashed #7c3aed; border-radius:8px; padding:20px; text-align:center; margin:20px 0;">
                                <p style="margin:0 0 6px; font-size:13px; color:#64748b;">Ücretsiz İade Kargo Kodu</p>
                                <p style="margin:0; font-size:22px; font-weight:700; color:#7c3aed; font-family:monospace; letter-spacing:2px;">@Model.ReturnCargoCode</p>
                            </div>

                            <p style="margin:0; font-size:13px; line-height:1.5; color:#64748b;">
                                Ürün bize ulaşıp kontroller tamamlandıktan sonra <strong>@Model.RefundAmount</strong> tutarındaki iadeniz hesabınıza aktarılacaktır.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: ReturnApproved';
END;

-- -----------------------------------------------------------------------------
-- Template: WelcomeCustomer
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'WelcomeCustomer')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'Aramıza Hoş Geldiniz! 🎉 - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Aramıza Hoş Geldiniz</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#d97706; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#fffbeb; padding:25px 30px; text-align:center; border-bottom:1px solid #fef3c7;">
                            <h2 style="margin:0 0 6px; font-size:20px; color:#d97706; font-weight:700;">Aramıza Hoş Geldiniz, @Model.CustomerName! 🎊</h2>
                            <p style="margin:0; font-size:14px; color:#b45309;">Sizi aramızda görmekten büyük mutluluk duyuyoruz.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                <strong>@Model.CompanyName</strong> ayrıcalıklar dünyasına adım attınız. İlk alışverişinize özel hediyenizi kullanmayı unutmayın!
                            </p>
                            
                            @if (!string.IsNullOrEmpty(Model.CouponCode)) {
                                <div style="background-color:#f8fafc; border:2px dashed #d97706; border-radius:8px; padding:20px; text-align:center; margin:20px 0;">
                                    <p style="margin:0 0 6px; font-size:13px; color:#64748b;">İlk Alışveriş İndirim Kuponunuz</p>
                                    <p style="margin:0 0 6px; font-size:22px; font-weight:700; color:#d97706; font-family:monospace;">@Model.CouponCode</p>
                                </div>
                            }

                            <div style="text-align:center; margin:25px 0;">
                                <a href="@Model.ShopUrl" style="background-color:#d97706; color:#ffffff; text-decoration:none; padding:12px 32px; border-radius:6px; font-size:15px; font-weight:600; display:inline-block;">
                                    Alışverişe Başla →
                                </a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 12,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'WelcomeCustomer';
    PRINT N'Updated MailTemplate: WelcomeCustomer';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'WelcomeCustomer', @Now, @Now, 1, 12, @Lang, N'Aramıza Hoş Geldiniz! 🎉 - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Aramıza Hoş Geldiniz</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#d97706; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#fffbeb; padding:25px 30px; text-align:center; border-bottom:1px solid #fef3c7;">
                            <h2 style="margin:0 0 6px; font-size:20px; color:#d97706; font-weight:700;">Aramıza Hoş Geldiniz, @Model.CustomerName! 🎊</h2>
                            <p style="margin:0; font-size:14px; color:#b45309;">Sizi aramızda görmekten büyük mutluluk duyuyoruz.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <p style="margin:0 0 16px; font-size:15px; line-height:1.6; color:#4a5568;">
                                <strong>@Model.CompanyName</strong> ayrıcalıklar dünyasına adım attınız. İlk alışverişinize özel hediyenizi kullanmayı unutmayın!
                            </p>
                            
                            @if (!string.IsNullOrEmpty(Model.CouponCode)) {
                                <div style="background-color:#f8fafc; border:2px dashed #d97706; border-radius:8px; padding:20px; text-align:center; margin:20px 0;">
                                    <p style="margin:0 0 6px; font-size:13px; color:#64748b;">İlk Alışveriş İndirim Kuponunuz</p>
                                    <p style="margin:0 0 6px; font-size:22px; font-weight:700; color:#d97706; font-family:monospace;">@Model.CouponCode</p>
                                </div>
                            }

                            <div style="text-align:center; margin:25px 0;">
                                <a href="@Model.ShopUrl" style="background-color:#d97706; color:#ffffff; text-decoration:none; padding:12px 32px; border-radius:6px; font-size:15px; font-weight:600; display:inline-block;">
                                    Alışverişe Başla →
                                </a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: WelcomeCustomer';
END;

-- -----------------------------------------------------------------------------
-- Template: BackInStock
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'BackInStock')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'Müjde! Takip Ettiğiniz Ürün Yeniden Stokta - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Ürün Yeniden Stokta</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#0284c7; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <h2 style="margin:0 0 12px; font-size:18px; color:#1e293b;">Müjde @Model.CustomerName! Beklediğiniz Ürün Geldi 🎉</h2>
                            <p style="margin:0 0 20px; font-size:14px; color:#4a5568; line-height:1.5;">
                                Daha önce takip listenize eklediğiniz <strong>@Model.ProductName</strong> tekrar stoklarımıza girdi. Tükenmeden hemen inceleyin!
                            </p>
                            
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:8px; padding:20px; text-align:center; margin:20px 0;">
                                @if (!string.IsNullOrEmpty(Model.ProductImageUrl)) {
                                    <img src="@Model.ProductImageUrl" alt="@Model.ProductName" style="max-width:180px; max-height:180px; border-radius:6px; margin-bottom:12px;" /><br>
                                }
                                <h3 style="margin:0 0 6px; font-size:16px; color:#1e293b;">@Model.ProductName</h3>
                                <p style="margin:0 0 16px; font-size:18px; font-weight:700; color:#0284c7;">@Model.ProductPrice</p>
                                <a href="@Model.ProductUrl" style="background-color:#0284c7; color:#ffffff; text-decoration:none; padding:12px 28px; border-radius:6px; font-size:14px; font-weight:600; display:inline-block;">
                                    Ürünü Hemen İncele →
                                </a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 13,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'BackInStock';
    PRINT N'Updated MailTemplate: BackInStock';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'BackInStock', @Now, @Now, 1, 13, @Lang, N'Müjde! Takip Ettiğiniz Ürün Yeniden Stokta - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Ürün Yeniden Stokta</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#0284c7; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <h2 style="margin:0 0 12px; font-size:18px; color:#1e293b;">Müjde @Model.CustomerName! Beklediğiniz Ürün Geldi 🎉</h2>
                            <p style="margin:0 0 20px; font-size:14px; color:#4a5568; line-height:1.5;">
                                Daha önce takip listenize eklediğiniz <strong>@Model.ProductName</strong> tekrar stoklarımıza girdi. Tükenmeden hemen inceleyin!
                            </p>
                            
                            <div style="background-color:#f8fafc; border:1px solid #e2e8f0; border-radius:8px; padding:20px; text-align:center; margin:20px 0;">
                                @if (!string.IsNullOrEmpty(Model.ProductImageUrl)) {
                                    <img src="@Model.ProductImageUrl" alt="@Model.ProductName" style="max-width:180px; max-height:180px; border-radius:6px; margin-bottom:12px;" /><br>
                                }
                                <h3 style="margin:0 0 6px; font-size:16px; color:#1e293b;">@Model.ProductName</h3>
                                <p style="margin:0 0 16px; font-size:18px; font-weight:700; color:#0284c7;">@Model.ProductPrice</p>
                                <a href="@Model.ProductUrl" style="background-color:#0284c7; color:#ffffff; text-decoration:none; padding:12px 28px; border-radius:6px; font-size:14px; font-weight:600; display:inline-block;">
                                    Ürünü Hemen İncele →
                                </a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: BackInStock';
END;

-- -----------------------------------------------------------------------------
-- Template: AbandonedCart
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'AbandonedCart')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'Sepetinizdeki Ürünler Sizi Bekliyor! 🛒 - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Sepetinizi Unutmayın</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#ea580c; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <h2 style="margin:0 0 12px; font-size:18px; color:#1e293b;">Alışverişinizi Tamamlamayı Unuttunuz mu? 🛒</h2>
                            <p style="margin:0 0 20px; font-size:14px; color:#4a5568; line-height:1.5;">
                                Merhaba @Model.CustomerName, sepetinize eklediğiniz ürünler tükenmeden siparişinizi kolayca tamamlayabilirsiniz.
                            </p>
                            
                            <div style="text-align:center; margin:25px 0;">
                                <a href="@Model.CheckoutUrl" style="background-color:#ea580c; color:#ffffff; text-decoration:none; padding:12px 32px; border-radius:6px; font-size:15px; font-weight:600; display:inline-block;">
                                    Sepetime Git ve Tamamla →
                                </a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 14,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'AbandonedCart';
    PRINT N'Updated MailTemplate: AbandonedCart';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'AbandonedCart', @Now, @Now, 1, 14, @Lang, N'Sepetinizdeki Ürünler Sizi Bekliyor! 🛒 - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Sepetinizi Unutmayın</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#ea580c; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <h2 style="margin:0 0 12px; font-size:18px; color:#1e293b;">Alışverişinizi Tamamlamayı Unuttunuz mu? 🛒</h2>
                            <p style="margin:0 0 20px; font-size:14px; color:#4a5568; line-height:1.5;">
                                Merhaba @Model.CustomerName, sepetinize eklediğiniz ürünler tükenmeden siparişinizi kolayca tamamlayabilirsiniz.
                            </p>
                            
                            <div style="text-align:center; margin:25px 0;">
                                <a href="@Model.CheckoutUrl" style="background-color:#ea580c; color:#ffffff; text-decoration:none; padding:12px 32px; border-radius:6px; font-size:15px; font-weight:600; display:inline-block;">
                                    Sepetime Git ve Tamamla →
                                </a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: AbandonedCart';
END;

-- -----------------------------------------------------------------------------
-- Template: PriceDropAlert
-- -----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM dbo.MailTemplates WHERE Name = N'PriceDropAlert')
BEGIN
    UPDATE dbo.MailTemplates
    SET Subject = N'Favori Ürününüzün Fiyatı Düştü! 📉 - @Model.CompanyName',
        Body = N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Fiyat İndirimi Bildirimi</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#e11d48; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <h2 style="margin:0 0 12px; font-size:18px; color:#1e293b;">İndirim Fırsatı @Model.CustomerName! 📉</h2>
                            <p style="margin:0 0 20px; font-size:14px; color:#4a5568; line-height:1.5;">
                                Favorilerinize eklediğiniz <strong>@Model.ProductName</strong> ürününün fiyatı düştü!
                            </p>
                            
                            <div style="background-color:#fff1f2; border:1px solid #fecdd3; border-radius:8px; padding:20px; text-align:center; margin:20px 0;">
                                @if (!string.IsNullOrEmpty(Model.ProductImageUrl)) {
                                    <img src="@Model.ProductImageUrl" alt="@Model.ProductName" style="max-width:180px; max-height:180px; border-radius:6px; margin-bottom:12px;" /><br>
                                }
                                <h3 style="margin:0 0 6px; font-size:16px; color:#1e293b;">@Model.ProductName</h3>
                                <p style="margin:0 0 16px;">
                                    <span style="text-decoration:line-through; color:#94a3b8; font-size:14px; margin-right:8px;">@Model.OldPrice</span>
                                    <span style="font-size:20px; font-weight:700; color:#e11d48;">@Model.NewPrice</span>
                                </p>
                                <a href="@Model.ProductUrl" style="background-color:#e11d48; color:#ffffff; text-decoration:none; padding:12px 28px; border-radius:6px; font-size:14px; font-weight:600; display:inline-block;">
                                    İndirimli Fiyatla Al →
                                </a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>',
        IsActive = 1,
        Position = 15,
        Lang = @Lang,
        UpdatedDate = @Now
    WHERE Name = N'PriceDropAlert';
    PRINT N'Updated MailTemplate: PriceDropAlert';
END
ELSE
BEGIN
    INSERT INTO dbo.MailTemplates
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
    VALUES
        (N'PriceDropAlert', @Now, @Now, 1, 15, @Lang, N'Favori Ürününüzün Fiyatı Düştü! 📉 - @Model.CompanyName', N'<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Fiyat İndirimi Bildirimi</title>
</head>
<body style="margin:0; padding:0; background-color:#f4f6f9; font-family:-apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Arial, sans-serif; color:#333333;">
    <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f4f6f9; padding:20px 0;">
        <tr>
            <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" border="0" style="max-width:600px; width:100%; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                    <tr>
                        <td align="center" style="background-color:#ffffff; padding:25px 20px; border-bottom:2px solid #f0f2f5;">
                            @if (!string.IsNullOrEmpty(Model.WebSiteIconUrl)) {
                                <img src="@Model.WebSiteIconUrl" alt="@Model.CompanyName" style="max-height:48px; max-width:200px; display:block; border:0;" />
                            } else {
                                <h1 style="margin:0; font-size:24px; color:#e11d48; font-weight:700;">@Model.CompanyName</h1>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td style="padding:30px;">
                            <h2 style="margin:0 0 12px; font-size:18px; color:#1e293b;">İndirim Fırsatı @Model.CustomerName! 📉</h2>
                            <p style="margin:0 0 20px; font-size:14px; color:#4a5568; line-height:1.5;">
                                Favorilerinize eklediğiniz <strong>@Model.ProductName</strong> ürününün fiyatı düştü!
                            </p>
                            
                            <div style="background-color:#fff1f2; border:1px solid #fecdd3; border-radius:8px; padding:20px; text-align:center; margin:20px 0;">
                                @if (!string.IsNullOrEmpty(Model.ProductImageUrl)) {
                                    <img src="@Model.ProductImageUrl" alt="@Model.ProductName" style="max-width:180px; max-height:180px; border-radius:6px; margin-bottom:12px;" /><br>
                                }
                                <h3 style="margin:0 0 6px; font-size:16px; color:#1e293b;">@Model.ProductName</h3>
                                <p style="margin:0 0 16px;">
                                    <span style="text-decoration:line-through; color:#94a3b8; font-size:14px; margin-right:8px;">@Model.OldPrice</span>
                                    <span style="font-size:20px; font-weight:700; color:#e11d48;">@Model.NewPrice</span>
                                </p>
                                <a href="@Model.ProductUrl" style="background-color:#e11d48; color:#ffffff; text-decoration:none; padding:12px 28px; border-radius:6px; font-size:14px; font-weight:600; display:inline-block;">
                                    İndirimli Fiyatla Al →
                                </a>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#f8fafc; padding:20px 30px; border-top:1px solid #edf2f7; text-align:center; font-size:12px; color:#94a3b8;">
                            © @DateTime.Now.Year @Model.CompanyName. Tüm hakları saklıdır.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>', @AdminId, N'SEED', 0, 0);
    PRINT N'Inserted MailTemplate: PriceDropAlert';
END;

COMMIT TRANSACTION;

PRINT N'All MailTemplates successfully upserted.';

SELECT Id, Name, Position, Subject, IsActive, UpdatedDate 
FROM dbo.MailTemplates 
ORDER BY Position, Id;
