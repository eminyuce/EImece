# Admin email template test environment

- **Captured:** 2026-08-13 2:41:03 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

We need an Email Template Test Environment in the admin panel.

### Context
- Existing page: http://localhost:81/admin/mailtemplates/
- SMTP settings already exist at: http://localhost:81/admin/adminsettings/systemsettings/#tab-smtp
- Goal: Allow admins to test any email template by sending a real email (using the configured SMTP) so they can verify the visual design and content before the template is used in actual business logic.

### Requirements

1. Test Email Feature
   - On the mail templates list/detail page, add a "Test Email" / "Send Test" action for each template.
   - When clicked, show a modal or dedicated form with:
     - Recipient email address (required)
     - Optional subject override
     - A way to provide / edit the model data

2. Dummy / Sample Data Generation
   - Email templates can have different model requirements in the future (e.g. @Model.Email, @Model.CompanyName, @Model.ForgotPasswordLink, @Model.WebSiteIconUrl, etc.).
   - Automatically generate sensible dummy data based on the properties used in the template.
   - Allow the user to edit the generated dummy values before sending.
   - Support common property types (string, url, email, etc.).

3. Template Example (Password Reset)
   The system already has (or will have) templates like this:

   <!DOCTYPE html>
   <html>
   <head>
       <meta charset="UTF-8">
       <title>Şifre Sıfırlama Talebi</title>
       <style>
           body {
               font-family: Arial, sans-serif;
               background-color: #f4f4f4;
               margin: 0;
               padding: 0;
           }
           .container {
               width: 100%;
               max-width: 600px;
               margin: 20px auto;
               background-color: #ffffff;
               padding: 20px;
               border-radius: 8px;
               box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
               text-align: center;
           }
           .logo {
               max-width: 150px;
               margin-bottom: 20px;
           }
           h2 {
               color: #333333;
           }
           p {
               color: #555555;
               font-size: 16px;
               line-height: 1.5;
           }
           .button {
               display: inline-block;
               margin-top: 20px;
               padding: 12px 24px;
               background-color: #ede8e9;
               color: #ffffff;
               text-decoration: none;
               font-size: 16px;
               font-weight: bold;
               border-radius: 5px;
               box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
           }
           .button:hover {
               background-color: #dbd3d4;
           }
           .footer {
               margin-top: 20px;
               font-size: 14px;
               color: #888888;
           }
       </style>
   </head>
   <body>
       <div class="container">
           <img src="@Model.WebSiteIconUrl" alt="Şirket Logosu" class="logo">
           <h2>Şifre Sıfırlama Talebi</h2>
           <p>Merhaba <strong>@Model.Email</strong>,</p>
           <p><strong>@Model.CompanyName</strong> üzerinde hesabınız için bir şifre sıfırlama talebi alındı.</p>
           <p>Eğer bu işlemi siz başlatmadıysanız, lütfen bu e-postayı dikkate almayın. Hesabınız güvende ve şifreniz değişmemiştir.</p>
           <p>Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>
           <a href="@Model.ForgotPasswordLink" class="button">Şifremi Sıfırla</a>
           <p>Eğer yukarıdaki buton çalışmıyorsa, aşağıdaki bağlantıyı tarayıcınıza kopyalayabilirsiniz:</p>
           <p><a href="@Model.ForgotPasswordLink">@Model.ForgotPasswordLink</a></p>
           <p class="footer">Teşekkürler, <br><strong>@Model.CompanyName Yönetimi</strong></p>
       </div>
   </body>
   </html>

4. Technical Expectations
   - Use the existing SMTP configuration from System Settings.
   - Render the template with the provided (or generated) model data.
   - Send the email and show clear success/error feedback.
   - Keep the solution extensible so new templates with different model properties can be tested without hardcoding every property.
   - Prefer a clean UX (modal is preferred over a separate page if possible).

### Deliverables
- Backend endpoint(s) to:
  - Parse/inspect template for used model properties
  - Generate dummy data
  - Render template + send test email
- Frontend UI for triggering the test and editing dummy data
- Clear error handling and user feedback

Please implement this feature following the existing project architecture, coding style, and patterns.
