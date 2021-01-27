﻿using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Helpers
{
    public class GeneralHelper
    {
        private const string KEYID = "9182736450";
        private const string ALPHABETS = "abcdefghijklmnopqrstuvwxyz";
        private static Random random = new Random();

        public static string ReverseString(string srtVarable)
        {
            return new string(srtVarable.Reverse().ToArray());
        }

        public static int RevertId(string result)
        {
            string result2 = new String(result.Where(Char.IsDigit).ToArray());

            var result3 = new StringBuilder();
            char[] result2Ids = result2.ToCharArray();
            char[] keyIds = KEYID.ToCharArray();
            for (int i = 0; i < result2Ids.Length; i++)
            {
                var nnn = result2Ids[i].ToInt();
                for (int j = 0; j < keyIds.Length; j++)
                {
                    if (nnn == keyIds[j].ToInt())
                        result3.Append(j);
                }
            }
            return ReverseString(result3.ToString()).ToInt();
        }

        public static string ModifyId(int id)
        {
            StringBuilder result = new StringBuilder();
            char[] keyIds = KEYID.ToCharArray();
            char[] alpha = ALPHABETS.ToCharArray();
            char[] idCharacters = id.ToString(CultureInfo.InvariantCulture).ToCharArray();
            for (int i = idCharacters.Length - 1; i >= 0; i--)
            {
                var num = idCharacters[i].ToInt();
                result.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", keyIds[num], alpha[num]);
            }
            return result.ToString();
        }

        public static List<T> GetUnique<T>(List<T> clazzes) where T : BaseEntity
        {
            var list = new List<T>();
            foreach (var item in clazzes)
            {
                if (!list.Any(r => r.Id == item.Id))
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public static string RandomNumber(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static String GetIpAddress()
        {
            return (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ??
                 HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]).Split(',')[0].Trim();
        }

        /// <summary>
        /// Generates a Random Password
        /// respecting the given strength requirements.
        /// </summary>
        /// <param name="opts">A valid PasswordOptions object
        /// containing the password strength requirements.</param>
        /// <returns>A random password</returns>
        public static string GenerateRandomPassword()
        {
            int RequiredLength = 8;
            bool RequireDigit = true;
            bool RequireLowercase = true;
            bool RequireUppercase = true;

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "."                        // non-alphanumeric
        };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            //           if (RequireNonAlphanumeric)
            //               chars.Insert(rand.Next(0, chars.Count),
            //                   randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < RequiredLength; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public static int FromBase64Int(string plainText)
        {
            return FromBase64String(plainText).ToInt();
        }

        public static string Base64Encode(int id)
        {
            return Base64Encode(id + "");
        }

        private static string FromBase64String(string plainText)
        {
            //var encodedStr = Base64UrlEncoder.Encode(StringToEncode);
            //var decodedStr = Base64UrlEncoder.Decode(encodedStr);

            return Encoding.UTF8.GetString(System.Convert.FromBase64String(plainText));
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static void SetCultureCookie(HttpResponseBase request, String cultureCookieName, string cultureName = "")
        {
            if (String.IsNullOrEmpty(cultureName))
            {
                cultureName = EnumHelper.GetEnumDescription(((EImeceLanguage)AppConfig.MainLanguage));
            }
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            request.Cookies[cultureCookieName].Value = cultureName;
        }

        /// <summary>
        /// When a client IP can't be determined
        /// </summary>
        public const string UnknownIP = "0.0.0.0";

        private static readonly Regex _ipAddress = new Regex(@"\b([0-9]{1,3}\.){3}[0-9]{1,3}$",
                                                         RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// returns true if this is a private network IP
        /// http://en.wikipedia.org/wiki/Private_network
        /// </summary>
        private static bool IsPrivateIP(string s)
        {
            return (s.StartsWith("192.168.") || s.StartsWith("10.") || s.StartsWith("127.0.0."));
        }

        public static string GetRemoteIP(NameValueCollection ServerVariables)
        {
            string ip = ServerVariables["REMOTE_ADDR"]; // could be a proxy -- beware
            string ipForwarded = ServerVariables["HTTP_X_FORWARDED_FOR"];

            // check if we were forwarded from a proxy
            if (!String.IsNullOrEmpty(ipForwarded))
            {
                ipForwarded = _ipAddress.Match(ipForwarded).Value;
                if (!String.IsNullOrEmpty(ipForwarded) && !IsPrivateIP(ipForwarded))
                    ip = ipForwarded;
            }

            return !String.IsNullOrEmpty(ip) ? ip : UnknownIP;
        }

        public static String GetCultureCookie(HttpResponseBase request, String cultureCookieName)
        {
            string cultureName = null;
            // check the cookie first to get culture name
            HttpCookie cultureCookie = request.Cookies[cultureCookieName];
            if (cultureCookie != null)
            {
                cultureName = cultureCookie.Value;
            }
            if (String.IsNullOrEmpty(cultureName))
            {
                cultureName = EnumHelper.GetEnumDescription(((EImeceLanguage)AppConfig.MainLanguage));
            }

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            if (request.Cookies[cultureCookieName] != null)
            {
                request.Cookies[cultureCookieName].Value = cultureName;
            }
            else
            {
                request.Cookies.Add(cultureCookie);
            }

            return cultureName;
        }

        public static String GetStringTitleCase(string text)
        {
            // Creates a TextInfo based on the "en-US" culture.
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            return myTI.ToTitleCase(text);
        }

        public static String ConvertToDateTimeOffset(DateTime t)
        {
            TimeZoneInfo TargetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            DateTimeOffset outputDate = new DateTimeOffset(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second,
                                     TargetTimeZone.BaseUtcOffset);
            string format = "yyyy-MM-ddTHH:mm:sszzz";
            return outputDate.ToString(format, new CultureInfo("en-US"));
        }

        public static string EncodeForEmailLink(string text)
        {
            text = text.Replace(" ", "%20");
            text = text.Replace((char)10 + "", "%0d");
            text = text.Replace((char)13 + "", "%0a");
            return text;
        }

        public static string GetDescriptionWithBody(string description, int length = 300)
        {
            if (String.IsNullOrEmpty(description))
            {
                return "";
            }
            else
            {
                string stripDesc = StripHtml(description);
                var desc = stripDesc.TruncateAtSentence(length);

                if (String.IsNullOrEmpty(desc))
                {
                    desc = "";
                }

                return desc;
            }
        }

        public static string GetMd5Hash(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (byte t in hash)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }

        public static string EncodeFile(string fileName)
        {
            return System.Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
        }

        public static X509Certificate2 ImportCert(byte[] rawData, String password)
        {
            var x509 = new X509Certificate2();
            x509.Import(rawData, password, X509KeyStorageFlags.Exportable);
            return x509;
        }

        public static X509Certificate2 CreateCert(Byte[] serviceAccountPkCs12FilePath, String password)
        {
            X509Certificate2 cert = new X509Certificate2(serviceAccountPkCs12FilePath, password, X509KeyStorageFlags.Exportable);
            return cert;
        }

        public static X509Certificate2 CreateCert(String serviceAccountPkCs12FilePath, String password)
        {
            X509Certificate2 cert = new X509Certificate2(serviceAccountPkCs12FilePath, password, X509KeyStorageFlags.Exportable);
            return cert;
        }

        public static byte[] ExportCertFile(String serviceAccountPkCs12FilePath, String password)
        {
            X509Certificate2 cert = new X509Certificate2(serviceAccountPkCs12FilePath, password, X509KeyStorageFlags.Exportable);
            byte[] bytes = cert.Export(X509ContentType.Pkcs12);
            return bytes;
        }

        public static string GetDomainPart(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return "";
            }

            var doubleSlashesIndex = url.IndexOf("://");
            var start = doubleSlashesIndex != -1 ? doubleSlashesIndex + "://".Length : 0;
            var end = url.IndexOf("/", start);
            if (end == -1)
                end = url.Length;

            string trimmed = url.Substring(start, end - start);
            if (trimmed.StartsWith("www."))
                trimmed = trimmed.Substring("www.".Length);
            return trimmed;
        }

        public static string SettingSpan(string key, string value)
        {
            return String.Format("<span id='{0}'>{1}</span>", key, value);
        }

        public static string UrlDecode(string adres, bool encode)
        {
            string[] karakter = { "<", ">", "#", "%", "{", "}", "|", @"\", "^", "~", "[", "]", "`", ";", "/", "?", ":", "@", "=", "&", "$" };
            string[] donustur = { "%3C", "%3E", "%23", "%25", "%7B", "%7D", "%7C", "%5C", "%5E", "%7E", "%5B", "%5D", "%60", "%3B", "%2F", "%3F", "%3A", "%40", "%3D", "%26", "%24" };

            if (encode)
            {
                for (int i = 0; i < karakter.Length; i++)
                {
                    adres = adres.Replace(karakter[i], donustur[i]);
                }
            }
            else
            {
                for (int i = 0; i < donustur.Length; i++)
                {
                    adres = adres.Replace(donustur[i], karakter[i]);
                }
            }
            return adres;
        }

        public static Stream GeneratePDF(byte[] pdf)
        {
            //create your pdf and put it into the stream... pdf variable below
            //comes from a class I use to write content to PDF files
            MemoryStream ms = new MemoryStream();
            byte[] byteInfo = pdf;
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;

            return ms;
        }

        public static DataTable LINQToDataTable<T>(IEnumerable<T> varlist)
        {
            DataTable dtReturn = new DataTable();

            // column names
            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;

            foreach (T rec in varlist)
            {
                // Use reflection to get property names, to create table, Only first time, others will follow
                if (oProps == null)
                {
                    oProps = (rec.GetType()).GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition()
                        == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow();

                foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue
                    (rec, null);
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        public static string CapitalizeFirstLetter(string s)
        {
            if (String.IsNullOrEmpty(s))
                return s;
            if (s.Length == 1)
                return s.ToUpper();
            return s.Remove(1).ToUpper() + s.Substring(1);
        }

        public static void RemoveEmptyColumnDataTables(List<DataTable> dtList)
        {
            foreach (var dt in dtList)
            {
                RemoveEmptyColumn(dt);
            }
        }

        public static DataTable RemoveEmptyColumn(DataTable dt)
        {
            bool flag = false;
            int counter = 0;

        EXIT:

            for (int i = counter; i < dt.Columns.Count; i++)
            {
                //if it is empty datatable, that inner loop will never work.
                for (int x = 0; x < dt.Rows.Count; x++)
                {
                    if (string.IsNullOrEmpty(dt.Rows[x][i].ToString()))
                    {
                        flag = true; //means there is an empty value
                    }
                    else
                    {
                        //means if it found non null or empty in rows of a particular column

                        flag = false;

                        counter = i + 1;

                        goto EXIT;
                    }
                }

                if (flag == true)
                {
                    dt.Columns.Remove(dt.Columns[i]);

                    i--;
                }
            }

            return dt;
        }

        public static void RemoveNullColumnFromDataTables(List<DataTable> dtList)
        {
            foreach (var dt in dtList)
            {
                RemoveNullColumnFromDataTable(dt);
            }
        }

        public static void RemoveNullColumnFromDataTable(DataTable dt)
        {
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                if (!System.Convert.IsDBNull(dt.Rows[i][1]))
                {
                    dt.Rows[i].Delete();
                }
            }
            dt.AcceptChanges();
        }

        public static string GetFileExtension(String fileName)
        {
            String extension = "";
            int fileExtPos = fileName.LastIndexOf(".");
            if (fileExtPos >= 0)
            {
                extension = fileName.Substring(fileExtPos);
            }

            return extension;
        }

        public static string RemoveHtmlTags(string content)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            var cleaned = string.Empty;
            string textOnly = string.Empty;
            Regex tagRemove = new Regex(@"<[^>]*(>|$)");
            Regex compressSpaces = new Regex(@"[\s\r\n]+");
            textOnly = tagRemove.Replace(content, " ");
            textOnly = compressSpaces.Replace(textOnly, " ");
            cleaned = textOnly;

            return cleaned;
        }

        public static String GetDescription(string bodyHtml, int length)
        {
            var stripedText = GeneralHelper.RemoveHtmlTags(bodyHtml);
            stripedText = stripedText.HtmlDecode();
            stripedText = stripedText.TruncateAtSentence(length);

            return stripedText;
        }

        public static byte[] GetImageFromUrlFromCache(string url, Dictionary<String, String> dictionary, int minute = 100)
        {
            String key = url;
            byte[] ret = null;

            ret = (byte[])MemoryCache.Default.Get(key);
            if (ret == null)
            {
                ret = GetImageFromUrl(url, dictionary);
                if (ret != null)
                {
                    CacheItemPolicy policy = null;
                    policy = new CacheItemPolicy();
                    policy.Priority = CacheItemPriority.Default;
                    policy.AbsoluteExpiration = DateTime.Now.AddMinutes(minute);
                    MemoryCache.Default.Set(key, ret, policy);
                }
            }
            else
            {
            }
            return ret;
        }

        public static byte[] GetImageFromUrl(string url, Dictionary<String, String> dictionary)
        {
            System.Net.HttpWebRequest request = null;
            System.Net.HttpWebResponse response = null;
            byte[] b = null;

            try
            {
                request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                response = (System.Net.HttpWebResponse)request.GetResponse();

                if (request.HaveResponse)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Stream receiveStream = response.GetResponseStream();
                        BinaryReader br = new BinaryReader(receiveStream);
                        b = br.ReadBytes(500000);
                        br.Close();

                        foreach (var h in response.Headers.AllKeys)
                        {
                            dictionary.Add(h, response.Headers[h]);
                        }
                        dictionary.Add("ContentType", response.ContentType);
                    }
                }
            }
            catch (Exception)
            {
            }
            return b;
        }

        public static Stream DownloadUrl(string url)
        {
            Stream rtn = null;

            HttpWebRequest aRequest = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse aResponse = (HttpWebResponse)aRequest.GetResponse();

            rtn = aResponse.GetResponseStream();
            return rtn;
        }

        public static string StripHtml(string html)
        {
            html = html.ToStr();
            char[] array = new char[html.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < html.Length; i++)
            {
                char let = html[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        public static string PuttingStars(string strOriginal)
        {
            if (String.IsNullOrEmpty(strOriginal))
                return strOriginal;

            string[] lines = strOriginal.Split(new string[] { " " },
                             StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() > 1)
            {
                for (int j = 1; j < lines.Length; j++)
                {
                    var starsLen = lines[j].Length;
                    String stars = "";
                    for (int i = 0; i < starsLen; i++)
                    {
                        stars += "*";
                    }
                    strOriginal = strOriginal.Replace(lines[j], stars);
                }
            }
            return strOriginal;
        }

        public static string ProtectEmail(String email, char character = '*')
        {
            if (String.IsNullOrEmpty(email))
                return email;

            string username = email.Split('@')[0];
            string domain = email.Split('@')[1];
            var c = username.ToCharArray();
            String p = "";
            for (int i = 0; i < c.Length - 2; i++)
            {
                p += Char.ToString(character);
            }
            String m = "";

            if (String.IsNullOrEmpty(p))
            {
                m = String.Format("{0}{1}", c.First(), Char.ToString(character));
            }
            else
            {
                m = String.Format(String.Format("{0}", c.First() + "{0}" + c.Last()), p);
            }

            return m + "@" + domain;
        }

        public static DataTable ConvertToDatatable<T>(IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        public static string GetTextFromFile()
        {
            var fileName = "Jobs.Domain.Helpers.tags.config";
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(fileName);

            if (stream == null)
            {
                throw new FileNotFoundException("Cannot find file.",
                   fileName);
            }

            var _textStreamReader = new StreamReader(stream);
            var rrr = _textStreamReader.ReadToEnd();
            return rrr;
        }

        public static string GetPlainTextFromHtml(string htmlString)
        {
            string htmlTagPattern = "<.*?>";
            var regexCss = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            htmlString = regexCss.Replace(htmlString, string.Empty);
            htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
            htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            htmlString = htmlString.Replace("&nbsp;", string.Empty);

            return htmlString;
        }

        public static byte[] GetHash(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return null;
            }
            HashAlgorithm algorithm = MD5.Create();  // SHA1.Create()
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static string TruncateAtWord(string input, int length)
        {
            if (input == null || input.Length < length)
                return input;
            int iNextSpace = input.LastIndexOf(" ", length);
            return String.Format("{0}", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }

        public static string FileNameToTitle(string FileName)
        {
            string ret;

            ret = Uri.UnescapeDataString(Path.GetFileNameWithoutExtension(FileName));

            // remove unwanted chars
            ret = Regex.Replace(ret, @"[^A-Za-z0-9]", " ");
            ret = ret.Replace('"', ' ');
            ret = Regex.Replace(ret, @"\d{2,3}px", " ");

            // remove digits if there is enough letters
            ret = ret.Count(c => Char.IsLetter(c)) > 5 ? Regex.Replace(ret, @"[0-9]", " ") : ret;

            // CaseStringIntoWords -> Case String Into Words
            Regex r = new Regex("([A-Z]+[a-z]+)");
            ret = r.Replace(ret, m => " " + (m.Value.Length > 3 ? m.Value : m.Value.ToLower()) + " ");

            // shorten the string
            ret = TruncateAtWord(ret, 150);

            // remove multiple spaces
            ret = Regex.Replace(ret, @"\s+", " ");
            ret = ret.Trim();

            return ret;
        }

        public static bool IsNumeric(object Expression)
        {
            bool isNum;
            double retNum;
            isNum = Double.TryParse(Expression.ToStr(), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        public static String[] GetStringArrayBasedOnDelimeter(char[] delimeter, String text)
        {
            String[] textArray = null;
            if (!String.IsNullOrEmpty(text.Trim()))
            {
                textArray = text.Split(delimeter, StringSplitOptions.None);
            }

            return textArray;
        }

        public static Dictionary<String, String> ConvertStringArrayToDictionary(char[] equalDelimeter, string[] imageArray)
        {
            if (imageArray == null)
                return null;

            if (equalDelimeter == null)
                return null;

            var dic = new Dictionary<String, String>();
            foreach (var s in imageArray)
            {
                if (!String.IsNullOrEmpty(s))
                {
                    String[] values = GeneralHelper.GetStringArrayBasedOnDelimeter(equalDelimeter, s);
                    string id = values[0];
                    string isSelected = values[1];
                    if (!dic.ContainsKey(id))
                    {
                        dic.Add(id, isSelected);
                    }
                }
            }

            return dic;
        }

        public static object DeepClone(object obj)
        {
            object objResult = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, obj);

                    ms.Position = 0;
                    objResult = bf.Deserialize(ms);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return objResult;
        }

        public static String getStringTitleCase(string text)
        {
            // Creates a TextInfo based on the "en-US" culture.
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            return myTI.ToTitleCase(text);
        }

        public static String getStringWithoutBreakingWords(string text, int length)
        {
            String[] words = text.Split(' ');
            int len = 0;
            StringBuilder built = new StringBuilder();
            foreach (String word in words)
            {
                len += word.Length + 1;
                if (length > len)
                {
                    built.Append(word + " ");
                }
            }
            return built.ToString();
        }

        public static string GetClientIpAddress(HttpRequestBase request)
        {
            try
            {
                var userHostAddress = request.UserHostAddress;

                // Attempt to parse.  If it fails, we catch below and return "0.0.0.0"
                // Could use TryParse instead, but I wanted to catch all exceptions
                IPAddress.Parse(userHostAddress);

                var xForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(xForwardedFor))
                    return userHostAddress;

                // Get a list of public ip addresses in the X_FORWARDED_FOR variable
                var publicForwardingIps = xForwardedFor.Split(',').Where(ip => !IsPrivateIpAddress(ip)).ToList();

                // If we found any, return the last one, otherwise return the user host address
                return publicForwardingIps.Any() ? publicForwardingIps.Last() : userHostAddress;
            }
            catch (Exception)
            {
                // Always return all zeroes for any failure (my calling code expects it)
                return "0.0.0.0";
            }
        }

        private static bool IsPrivateIpAddress(string ipAddress)
        {
            // http://en.wikipedia.org/wiki/Private_network
            // Private IP Addresses are:
            //  24-bit block: 10.0.0.0 through 10.255.255.255
            //  20-bit block: 172.16.0.0 through 172.31.255.255
            //  16-bit block: 192.168.0.0 through 192.168.255.255
            //  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)

            var ip = IPAddress.Parse(ipAddress);
            var octets = ip.GetAddressBytes();

            var is24BitBlock = octets[0] == 10;
            if (is24BitBlock) return true; // Return to prevent further processing

            var is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
            if (is20BitBlock) return true; // Return to prevent further processing

            var is16BitBlock = octets[0] == 192 && octets[1] == 168;
            if (is16BitBlock) return true; // Return to prevent further processing

            var isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;
            return isLinkLocalAddress;
        }

        public static string EncodeUTF8(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                byte[] bytes = Encoding.Default.GetBytes(value);
                value = Encoding.UTF8.GetString(bytes);
            }

            return value;
        }

        public static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@-]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        public static string GetSHA256Hash(byte[] bytes)
        {
            string ret = "";

            SHA256 sha256 = new SHA256CryptoServiceProvider();
            byte[] bHash = sha256.ComputeHash(bytes);
            ret = BitConverter.ToString(bHash).Replace("-", "");

            return ret;
        }

        public static List<double> ParseRange(string value)
        {
            var ret = new List<double>();
            var parts = value.Split(new char[] { ',', '/', '-' });
            if (parts.Count() == 2)
            {
                ret.Add(ExtractNumbers(parts[0]).ToFloat());
                ret.Add(ExtractNumbers(parts[1]).ToFloat());
            }
            else if (value.Split(new string[] { "to" }, StringSplitOptions.None).Count() == 2)
            {
                parts = value.Split(new string[] { "to" }, StringSplitOptions.None);
                ret.Add(ExtractNumbers(parts[0]).ToFloat());
                ret.Add(ExtractNumbers(parts[1]).ToFloat());
            }

            return ret;
        }

        public static string GetNumberFromStr(string str)
        {
            str = str.Trim();
            var m = Regex.Match(str, @"(\d+)");
            return (m.Value);
        }

        public static string ExtractNumbers(string expr)
        {
            return string.Join(null, Regex.Split(expr, "[^\\d]"));
        }

        public static SelectList GetYears()
        {
            var listItems = GetYearList();
            var sli = new SelectListItem();
            sli.Text = "All";
            sli.Value = "0";
            listItems.Insert(0, sli);
            return new SelectList(listItems, "Value", "Text");
        }

        private static List<SelectListItem> GetYearList()
        {
            var listItems = new List<SelectListItem>();
            int i = DateTime.Now.Year;
            for (i = i - 1; i <= DateTime.Now.Year + 10; i++)
            {
                String year = i.ToStr();
                var sli = new SelectListItem();
                sli.Text = year;
                sli.Value = year;
                listItems.Add(sli);
            }
            return listItems;
        }

        public static SelectList GetMonths()
        {
            var listItems = GetMonthList();
            var sli = new SelectListItem();
            sli.Text = "All";
            sli.Value = "0";
            listItems.Insert(0, sli);
            return new SelectList(listItems, "Value", "Text");
        }

        private static List<SelectListItem> GetMonthList()
        {
            var listItems = new List<SelectListItem>();
            var months = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
            for (int i = 0; i < months.Length; i++)
            {
                if (!String.IsNullOrEmpty(months[i]))
                {
                    int m = i + 1;
                    var sli = new SelectListItem();
                    sli.Text = months[i];
                    sli.Value = m.ToString();
                    listItems.Add(sli);
                }
            }

            return listItems;
        }

        public string ConvertUTF8(string text)
        {
            Encoding iso = Encoding.GetEncoding(text);
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(text);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);

            return msg;
        }

        public static string ConvertTurkishChars(string text)
        {
            if (String.IsNullOrEmpty(text))
                return "";
            String[] olds = { "Ğ", "ğ", "Ü", "ü", "Ş", "ş", "İ", "ı", "I", "Ö", "ö", "Ç", "ç" };
            // String[] news = { "G", "g", "U", "u", "S", "s", "I", "i", "O", "o", "C", "c" };
            String[] news = { "G", "g", "U", "u", "S", "s", "i", "i", "i", "O", "o", "C", "c" };

            for (int i = 0; i < olds.Length; i++)
            {
                text = text.Replace(olds[i], news[i]);
            }

            // text = text.ToUpper();

            return text;
        }

        public static string GetUrlSeoString(string p)
        {
            p = ConvertTurkishChars(p);
            //p = EncodeUTF8(p);
            return GetUrlString(p);
        }

        public static string GetUrlString(string strIn)
        {
            // Replace invalid characters with empty strings.
            strIn = strIn.ToLower();
            strIn = strIn.RemoveCarriage();
            char[] szArr = strIn.ToCharArray();
            var list = new List<char>();
            foreach (char c in szArr)
            {
                int ci = c;
                if ((ci >= 'a' && ci <= 'z') || (ci >= '0' && ci <= '9') || ci <= ' ')
                {
                    list.Add(c);
                }
            }
            return new String(list.ToArray()).Replace(" ", "-");
        }

        public static bool IsHtml(String myString)
        {
            Regex tagRegex = new Regex(@"<[^>]+>");
            bool hasTags = tagRegex.IsMatch(myString);
            return hasTags;
        }

        public static string GetLastNameFromFull(string fullName)
        {
            fullName = fullName.Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return string.Empty;
            }

            char[] spitters = new char[] { ' ', ',' };
            var words = fullName.Split(spitters, StringSplitOptions.RemoveEmptyEntries).ToList();

            string lastName;

            if (words.Count >= 4)
            {
                lastName = string.Join(" ", words.Skip(words.Count - 2));
            }
            else if (words.Count >= 3 && words.LastOrDefault() != null && words.LastOrDefault().StartsWith("("))
            {
                lastName = string.Join(" ", words.Skip(words.Count - 2));
            }
            else if (words.Count >= 1)
            {
                lastName = string.Join(" ", words.Skip(words.Count - 1));
            }
            else
            {
                lastName = fullName;
            }

            return lastName;
        }

        public static bool IsStringContainsLetters(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            Regex noDigit = new Regex(@"[^\d,. ]+");

            if (noDigit.Match(text).Captures.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static String LetterDigitsOnly(string filters)
        {
            if (string.IsNullOrEmpty(filters))
            {
                return "";
            }
            filters = filters.ToLower();
            filters = filters.RemoveCarriage();
            char[] szArr = filters.ToCharArray();
            var list = new List<char>();
            foreach (char c in szArr)
            {
                int ci = c;
                if ((ci >= 'a' && ci <= 'z'))
                {
                    list.Add(c);
                }
            }
            return new String(list.ToArray());
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static string GenerateRandomPassword(int length)
        {
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            char[] chars = new char[length];
            Random rd = new Random();
            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }
            var m = new string(chars);

            return m;
        }

        public static Byte[] ToByteArray(HttpPostedFileBase value)
        {
            if (value == null)
                return null;

            var array = new Byte[value.ContentLength];
            value.InputStream.Position = 0;
            value.InputStream.Read(array, 0, value.ContentLength);
            return array;
        }

        public static String GetSiteDomain(HttpContextBase httpContextBase)
        {
            HttpRequestBase request = httpContextBase.Request;
            String domainName = request.Url.Scheme + Uri.SchemeDelimiter + request.Url.Host +
                                     (request.Url.IsDefaultPort ? "" : ":" + request.Url.Port);
            domainName = GeneralHelper.GetDomainPart(domainName);

            return domainName;
        }

        public static string[] PropertiesFromType(object atype)
        {
            if (atype == null) return new string[] { };
            Type t = atype.GetType();
            PropertyInfo[] props = t.GetProperties();
            List<string> propNames = new List<string>();
            foreach (PropertyInfo prp in props)
            {
                propNames.Add(prp.Name);
            }
            return propNames.ToArray();
        }
    }
}