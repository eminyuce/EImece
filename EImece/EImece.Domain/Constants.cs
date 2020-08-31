using System.Collections.Immutable;

namespace EImece.Domain
{
    public static class Constants
    {
        public const string IyzicoDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string OrderGuidCookieKey = "orderGuid";
        public const string SUCCESS = "SUCCESS";
        public const string FAILED = "FAILED";

        /*********CACHE KEYS*********/
        public const string Cache30Days = "Cache30Days";
        public const string Cache20Minutes = "Cache20Minutes";
        public const string ImageProxyCaching = "ImageProxyCaching";
        public const string Cache1Hour = "Cache1Hour";
        /********* SETTING KEYS **********/
        public const string AdminEmailHost = "AdminEmailHost";
        public const string AdminEmailPassword = "AdminEmailPassword";
        public const string AdminEmailEnableSsl = "AdminEmailEnableSsl";
        public const string AdminEmailPort = "AdminEmailPort";
        public const string AdminEmailDisplayName = "AdminEmailDisplayName";
        public const string AdminEmail = "AdminEmail";
        public const string AdminEmailUseDefaultCredentials = "AdminEmailUseDefaultCredentials";
        public const string AdminUserName = "AdminUserName";
        public const string WebSiteCompanyEmailAddress = "WebSiteCompanyEmailAddress";
        public const string DefaultImageHeight = "DefaultImageHeight";
        public const string DefaultImageWidth = "DefaultImageWidth";
        public const string ProductsCategoriesControllerRoutingPrefix = "my_category";
        public const string ProductsControllerRoutingPrefix = "my_products";
        public const string SiteIndexMetaTitle = "SiteIndexMetaTitle";
        public const string IsProductPriceEnable = "IsProductPriceEnable";
        public const string GoogleMapScript = "GoogleMapScript";
        public const string SiteIndexMetaDescription = "SiteIndexMetaDescription";
        public const string SiteIndexMetaKeywords = "SiteIndexMetaKeywords";
        public const string SpecialPage = "Special_Page";
        public const string AdminSetting = "AdminSetting";
        public const string TermsAndConditions = "TermsAndConditions";
        public const string AboutUs = "AboutUs";
        public const string DeliveryInfo = "DeliveryInfo";
        public const string WebSiteLogo = "WebSiteLogo";
        public const string CompanyName = "CompanyName";
        public const string OrderConfirmationEmailMailTemplate = "OrderConfirmationEmail";
        public const string ConfirmYourAccountMailTemplate = "ConfirmYourAccount";
        public const string ForgotPasswordMailTemplate = "ForgotPassword";
        public const string ContactUsAboutProductInfoMailTemplate = "ContactUsAboutProductInfo";
        public const string SendMessageToSellerMailTemplate = "SendMessageToSeller";
        public const string WebSiteCompanyPhoneAndLocation = "WebSiteCompanyPhoneAndLocation";
        public const string InstagramWebSiteLink = "InstagramWebSiteLink";
        public const string PinterestWebSiteLink = "PinterestWebSiteLink";
        public const string TwitterWebSiteLink = "TwitterWebSiteLink";
        public const string LinkedinWebSiteLink = "LinkedinWebSiteLink";
        public const string FacebookWebSiteLink = "FacebookWebSiteLink";
        public const string YotubeWebSiteLink = "YotubeWebSiteLink";
        public const string GoogleAnalyticsTrackingScript = "GoogleAnalyticsTrackingScript";
        public const string LastVisit = "LastVisit";
        public const string PrivacyPolicy = "PrivacyPolicy";
        public const string ELanguage = "ELanguage";

        public const string TempDataReturnUrlReferrer = "TempDataReturnUrlReferrer";

        public const string DbConnectionKey = "EImeceDbConnection";
        public const string AdministratorRole = "Admin";
        public const string EditorRole = "NormalUser";
        public const string CustomerRole = "Customer";
        public const string ImageActionName = "Index";
        public const int PartialViewOutputCachingDuration = 86400;
        public const string SelectedLanguage = "SelectedLanguage";

        public const string CargoCompany = "CargoCompany";
        public const string BasketMinTotalPriceForCargo = "BasketMinTotalPriceForCargo";
        public const string CargoPrice = "CargoPrice";  
        public const string CargoDescription = "CargoDescription";

        public const string TempPath = "~/media/tempFiles/";
        public const string ServerMapPath = "~/media/images/";
        public const string UrlBase = "/media/images/";
        public const string DeleteURL = "/Media/DeleteFile/?file={0}&contentId={1}&mod={2}&imageType={3}";
        public const string DeleteType = "GET";
        public const string FileUploadDeleteURL = "/FileUpload/DeleteFile/?file=";
        public const string AdminCultureCookieName = "_adminCulture";
        public const string CultureCookieName = "_culture";
        public const string ShoppingCartKey = "ShoppingCartKey1";
        public static ImmutableArray<string> NumbersArr = ImmutableArray.Create(new string[] { "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" });
        public static string[] Countries = new string[] { " Türkiye ", "Amerika Birleşik Devletleri", "Kanada", "Afganistan", "Arnavutluk", "Cezayir", "Amerikan Samoası", "Andorra", "Angola", "Anguilla", "Antarktika", "Antigua ve / veya Barbuda" , "Arjantin", "Ermenistan", "Aruba", "Avustralya", "Avusturya", "Azerbaycan", "Bahamalar", "Bahreyn", "Bangladeş", "Barbados", "Beyaz Rusya", "Belçika", " Belize "," Benin "," Bermuda "," Butan "," Bolivya "," Bosna Hersek "," Botsvana "," Bouvet Adası "," Brezilya "," İngiliz Hint Okyanusu Bölgesi "," Brunei Darussalam ", "Bulgaristan", "Burkina Faso", "Burundi", "Kamboçya", "Kamerun", "Yeşil Burun Adaları", "Cayman Adaları", "Orta Afrika Cumhuriyeti", "Çad", "Şili", "Çin", " Christmas Adası "," Cocos (Keeling) Adaları "," Kolombiya "," Komorlar "," Kongo "," Cook Adaları "," Kosta Rika "," Hırvatistan (Hrvatska) "," Küba "," Kıbrıs "," Çek Cumhuriyeti "," Danimarka "," Cibuti "," Dominika "," Dominik Cumhuriyeti "," Doğu Timor "," Ekvator "," Mısır "," El Salvador "," Ekvator Ginesi "," Eritre "," Estonya "," Etiyopya "," Falkland Adaları (Malvinas) "," Faroe Adaları "," Fiji ", "Finlandiya", "Fransa", "Fransa, Metropolitan", "Fransız Guyanası", "Fransız Polinezyası", "Fransız Güney Toprakları", "Gabon", "Gambiya", "Gürcistan", "Almanya", "Gana", "Cebelitarık", "Yunanistan", "Grönland", "Grenada", "Guadeloupe", "Guam", "Guatemala", "Gine", "Gine-Bissau", "Guyana", "Haiti", "Heard ve Mc Donald Adaları "," Honduras "," Hong Kong "," Macaristan "," İzlanda "," Hindistan "," Endonezya "," İran (İslam Cumhuriyeti) "," Irak "," İrlanda "," İsrail ", "İtalya", "Fildişi Sahili", "Jamaika", "Japonya", "Ürdün", "Kazakistan", "Kenya", "Kiribati", "Kore, Demokratik Halk Cumhuriyeti", "Kore, Cumhuriyeti", " Kosova "," Kuveyt "," Kırgızistan "," Lao Demokratik Halk Cumhuriyeti "," Letonya "," Lübnan "," Lesoto "," Liberya "," Libya Arap Cemahiriyası "," Liechtenstein "," Litvanya "," Lüksemburg "," Makao "," Makedonya "," Madagaskar "," Malavi "," Malezya "," Maldivler "," Mali "," Malta "," Marshall Adaları "," Martinik "," Moritanya "," Mauritius " , "Mayotte", "Meksika", "Mikronezya Federal Devletleri", "Moldova, "," Monako "," Moğolistan "," Montserrat "," Fas "," Mozambik "," Myanmar "," Namibya "," Nauru "," Nepal "," Hollanda "," Hollanda Antilleri "," Yeni Kaledonya "," Yeni Zelanda "," Nikaragua "," Nijer "," Nijerya "," Niue "," Norfork Adası "," Kuzey Mariana Adaları "," Norveç "," Umman "," Pakistan "," Palau "," Panama "," Papua Yeni Gine "," Paraguay "," Peru "," Filipinler "," Pitcairn "," Polonya "," Portekiz "," Porto Riko "," Katar "," Yeniden Birleşme "," Romanya "," Rusya Federasyonu "," Ruanda "," Saint Kitts ve Nevis "," Saint Lucia "," Saint Vincent ve Grenadinler "," Samoa "," San Marino "," Sao Tome ve Principe "," Suudi Arabistan "," Senegal "," Seyşeller "," Sierra Leone "," Singapur "," Slovakya "," Slovenya "," Solomon Adaları "," Somali "," Güney Afrika "," Güney Georgia Güney Sandwich Adaları ", "Güney Sudan", "İspanya", "Sri Lanka", "St. Helena "," St. Pierre ve Miquelon "," Sudan "," Surinam "," Svalbarn ve Jan Mayen Adaları "," Svaziland "," İsveç "," İsviçre "," Suriye Arap Cumhuriyeti "," Tayvan "," Tacikistan "," Tanzanya, Birleşik Cumhuriyeti "," Tayland "," Togo "," Tokelau "," Tonga "," Trinidad ve Tobago "," Tunus "," Türkmenistan "," Türk ve Caicos Adaları "," Tuvalu ", "Uganda", "Ukrayna", "Birleşik Arap Emirlikleri", "Birleşik Krallık", "Amerika Birleşik Devletleri dıştaki küçük adalar", "Uruguay", "Özbekistan", "Vanuatu", "Vatikan Şehir Devleti", "Venezuela", " Vietnam "," Virigan Adaları (İngiliz) "," Virgin Adaları (ABD) "," Wallis ve Futuna Adaları "," Batı Sahra "," Yemen "," Yugoslavya "," Zaire "," Zambiya "," Zimbabve " };

        public static object DeleteButtonHtmlAttribute
        {
            get
            {
                return new { @class = "btn btn-sm btn-danger   glyphicon glyphicon-trash glyphicon-white" };
            }
        }

        public static string OkStyle
        {
            get
            {
                return "class='gridActiveIcon glyphicon glyphicon-ok-circle'";
            }
        }

        public static string CancelStyle
        {
            get
            {
                return "class='gridNotActiveIcon glyphicon  glyphicon-remove-circle'";
            }
        }
    }
}